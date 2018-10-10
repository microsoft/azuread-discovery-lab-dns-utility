using AzureADLabDNSControl.Infra;
using AzureADLabDNSControl.Models;
using Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AzureADLabDNSControl.Controllers.api
{
    [Authorize(Roles = CustomRoles.LabAdmin)]
    public class LabController : ApiController
    {
        //Lab Operations
        [HttpPost]
        public async Task<IEnumerable<LabDTO>> AddLab(LabSettings lab)
        {
            var res = await LabRepo.AddNewLab(lab, Settings.DomainList.ToArray());
            return MapLabs(res);
        }

        public async Task<bool> CheckLabDate([FromUri] DateTime labDate)
        {
            var res = await LabRepo.CheckLabDate(labDate);
            return res;
        }

        public async Task<IEnumerable<LabDTO>> GetLabs()
        {
            var res = await LabRepo.GetLabs(User.Identity.Name);
            return MapLabs(res);
        }

        public async Task<IEnumerable<DateTime>> GetLabDates()
        {
            var res = await LabRepo.GetLabDates();
            return res;
        }

        public async Task<LabSettings> GetLab(string id)
        {
            var res = await LabRepo.GetLab(id);
            return res;
        }

        [HttpPost]
        public async Task<IEnumerable<LabDTO>> UpdateLab(LabSettings lab)
        {
            var res = await LabRepo.UpdateLab(lab, User.Identity.Name);
            return MapLabs(res);
        }

        [HttpPost]
        public async Task<IEnumerable<LabSettings>> ResetLabCode(LabSettings lab)
        {
            var res = await LabRepo.ResetLabCode(lab, User.Identity.Name);
            return res;
        }
        [HttpPost]
        public async Task<IEnumerable<LabDTO>> DeleteLab(string id)
        {
            var res = await LabRepo.RemoveLab(id, User.Identity.Name);
            return MapLabs(res);
        }
        private IEnumerable<LabDTO> MapLabs(IEnumerable<LabSettings> list)
        {
            return list.Select(l => new LabDTO
            {
                City = l.City,
                Id = l.Id,
                LabDate = l.LabDate,
                PrimaryInstructor = l.PrimaryInstructor
            });
        }

        //Team Operations
        [HttpPost]
        public async Task<LabSettingsDTO> CheckDomainAssignment(TeamDTO team)
        {
            var res = new LabSettingsDTO();
            try
            {
                var control = await AADLinkControl.CreateAsync(team.TeamAssignment.AssignedTenantId, new HttpContextWrapper(HttpContext.Current));
                var adalResponse = await control.GetDomain(team.TeamAssignment.DomainName);
                if (adalResponse.Successful)
                {
                    res.ResponseMessage = adalResponse.ResponseContent;
                } else
                {
                    res.ResponseMessage += string.Format("Domain operation {0}, message: {1}", ((adalResponse.Successful) ? "successful" : "failed"), adalResponse.Message);
                }
            }
            catch (Exception ex)
            {
                res.ResponseMessage = "ERROR: " + ex.Message;
            }
            return res;
        }

        [HttpPost]
        public async Task<LabSettingsDTO> UnlinkAllDomains(TeamDTO team)
        {
            var res = new LabSettingsDTO();
            var responseMessage = new StringBuilder();
            try
            {
                AADLinkControl control = null;
                foreach(var teamAssignment in team.Lab.DomAssignments)
                {
                    responseMessage.AppendLine(string.Format("Resetting {0}...", teamAssignment.DomainName));
                    //detach domain from tenant
                    control = await AADLinkControl.CreateAsync(teamAssignment.AssignedTenantId, new HttpContextWrapper(HttpContext.Current));
                    var adalResponse = await control.DeleteDomain(teamAssignment.DomainName);
                    responseMessage.AppendLine(string.Format("    Domain operation {0}, message: {1}", ((adalResponse.Successful) ? "successful" : "failed"), adalResponse.Message));
                    teamAssignment.AssignedTenantId = null;

                    //reset TXT records
                    using (var dns = new DnsAdmin())
                    {
                        await dns.ClearTxtRecord(team.TeamAssignment.DomainName);
                    }
                    //update record in Cosmos
                    teamAssignment.DnsTxtRecord = null;
                }
                var upd = await LabRepo.UpdateLab(team.Lab, User.Identity.Name);
                res.Settings = upd.Single(l => l.Id == team.Lab.Id);
            }
            catch (Exception ex)
            {
                responseMessage.AppendLine("ERROR: " + ex.Message);
            }
            finally
            {
                res.ResponseMessage = responseMessage.ToString();
                res.Settings = await LabRepo.GetLab(team.Lab.Id);
            }

            return res;
        }


        [HttpPost]
        public async Task<LabSettingsDTO> DeleteAllLabDomains(TeamDTO team)
        {
            var res = new LabSettingsDTO();
            var responseMessage = new StringBuilder();
            try
            {
                foreach (var teamAssignment in team.Lab.DomAssignments)
                {
                    responseMessage.AppendLine(string.Format("Deleting {0}...", teamAssignment.DomainName));
                    //reset TXT records
                    using (var dns = new DnsAdmin())
                    {
                        await dns.RemoveChildZone(team.TeamAssignment.DomainName, team.TeamAssignment.ParentZone);
                    }
                }

                res.Settings = team.Lab;
            }
            catch (Exception ex)
            {
                responseMessage.AppendLine("ERROR: " + ex.Message);
            }
            finally
            {
                res.ResponseMessage = responseMessage.ToString();
                res.Settings = await LabRepo.GetLab(team.Lab.Id);
            }

            return res;
        }

        /// <summary>
        /// In JS, called from btnRemoveDomain
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<LabSettingsDTO> UnlinkDomain(TeamDTO team)
        {
            var res = new LabSettingsDTO();
            try
            {
                //detach domain from tenant
                var control = await AADLinkControl.CreateAsync(team.TeamAssignment.AssignedTenantId, new HttpContextWrapper(HttpContext.Current));
                var adalResponse = await control.DeleteDomain(team.TeamAssignment.DomainName);
                if (adalResponse.Message == "ObjectInUse")
                {
                    var references = await control.GetDomainReferences(team.TeamAssignment.DomainName);
                    res.Object = references.Object;
                }

                res.ResponseMessage += string.Format("Domain operation {0}, message: {1}", ((adalResponse.Successful) ? "successful" : "failed"), adalResponse.Message);
                res.Settings = await LabRepo.GetLab(team.Lab.Id);
            }
            catch (Exception ex)
            {
                res.ResponseMessage = "ERROR: " + ex.Message;
            }
            return res;
        }

        [HttpPost]
        public async Task<LabSettingsDTO> DeleteDirectoryObject(DelObjectDTO team)
        {
            var res = new LabSettingsDTO();
            try
            {
                //detach domain from tenant
                var control = await AADLinkControl.CreateAsync(team.TeamAssignment.AssignedTenantId, new HttpContextWrapper(HttpContext.Current));
                var adalResponse = await control.DeleteObject(team.DelObjId);

                res.ResponseMessage += string.Format("Delete operation {0}, message: {1}", ((adalResponse.Successful) ? "successful" : "failed"), adalResponse.Message);
                res.Settings = await LabRepo.GetLab(team.Lab.Id);
            }
            catch (Exception ex)
            {
                res.ResponseMessage = "ERROR: " + ex.Message;
            }
            return res;

        }
        /// <summary>
        /// In JS, called from btnResetTxtRecord
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<LabSettingsDTO> ResetTxtAssignment(TeamDTO team)
        {
            var res = new LabSettingsDTO();
            try
            {
                //remove zone record from zone
                using (var dns = new DnsAdmin())
                {
                    await dns.ClearTxtRecord(team.TeamAssignment.DomainName);
                }
                //update record in Cosmos
                await LabRepo.UpdateDnsRecord(team);
                res.ResponseMessage = "TXT record reset";
                res.Settings = await LabRepo.GetLab(team.Lab.Id);
            }
            catch (Exception ex)
            {
                res.ResponseMessage = "ERROR: " + ex.Message;
            }

            return res;
        }

        /// <summary>
        /// In JS, called from btnResetTeamAuth
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<LabSettingsDTO> ResetTeamCode(TeamDTO team)
        {
            var res = new LabSettingsDTO();
            try
            {
                //update record in Cosmos
                await LabRepo.ResetTeamCode(team);
                res.Settings = await LabRepo.GetLab(team.Lab.Id);
            }
            catch (Exception ex)
            {
                res.ResponseMessage = "ERROR: " + ex.Message;
            }
            return res;
        }
    }

    public class DelObjectDTO : TeamDTO
    {
        public string DelObjId { get; set; }
    }
}
