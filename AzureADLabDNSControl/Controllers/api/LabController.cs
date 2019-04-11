using AzureADLabDNSControl.Infra;
using Lab.Common;
using Lab.Common.Infra;
using Lab.Common.Repo;
using Lab.Data.Models;
using Lab.Infra;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AzureADLabDNSControl.Controllers.api
{
    [AdminAuthorize(Roles = CustomRoles.LabAdmin)]
    public class LabController : ApiController
    {
        private RGRepo _repo;

        public LabController()
        {
            _repo = new RGRepo();

        }

        //Lab Operations
        [HttpPost]
        public async Task<IEnumerable<LabDTO>> AddLab(LabSettings lab)
        {
            var res = await LabRepo.AddNewLab(lab, User.Identity.Name);
            SetQueue(lab.Id);
            return MapLabs(res);
        }

        [HttpPost]
        public async Task<IEnumerable<LabDTO>> UpdateLab(LabSettings lab)
        {
            var res = await LabRepo.UpdateLab(lab, User.Identity.Name);
            SetQueue(lab.Id);
            return MapLabs(res);
        }

        [HttpPost]
        public async Task<IEnumerable<LabDTO>> DeleteLab(string id)
        {
            var res = await LabRepo.RemoveLab(id, User.Identity.Name);
            SetQueue(id);
            return MapLabs(res);
        }

        private void SetQueue(string id)
        {
            var cli = LabQueue.GetQueueClient();
            var queue = LabQueue.GetQueue(cli, Settings.LabQueueName);
            var msg = new LabQueueDTO
            {
                LabId = id,
                UserName = User.Identity.Name
            };

            var message = JsonConvert.SerializeObject(msg);
            LabQueue.AddMessage(queue, message);
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

        public async Task<LabSettingsFull> GetLab(string id)
        {
            var res = await LabRepo.GetLabAndSettings(id);
            return res;
        }

        [HttpPost]
        public async Task<IEnumerable<LabSettings>> ResetLabCode(LabSettings lab)
        {
            var res = await LabRepo.ResetLabCode(lab, User.Identity.Name);
            return res;
        }

        private IEnumerable<LabDTO> MapLabs(IEnumerable<LabSettings> list)
        {
            return list.Select(l => new LabDTO
            {
                City = l.City,
                Id = l.Id,
                LabDate = l.LabDate,
                PrimaryInstructor = l.PrimaryInstructor,
                State = l.State
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
                res.Settings = await LabRepo.GetLabAndSettings(team.Lab.Id);
                if (adalResponse.Message != "ObjectInUse" && adalResponse.Successful)
                {
                    var res2 = await ResetTxtAssignment(team);
                    res.Settings = res2.Settings;
                }
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
                res.Settings = await LabRepo.GetLabAndSettings(team.Lab.Id);
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
                var data = await LabRepo.GetDomAssignment(team.Lab.LabCode, team.TeamAssignment.TeamAuth);

                //remove zone record from zone
                using (var dns = new DnsAdmin())
                {
                    var domGroup = await _repo.GetGroup(data.Lab.AzureSubscriptionId, data.Lab.DnsZoneRG);

                    await dns.InitAsync();
                    dns.SetClient(domGroup);
                    await dns.ClearTxtRecord(team.TeamAssignment.DomainName);
                }
                //update record in Cosmos
                var assignment = await LabRepo.GetDomAssignment(team.Lab.LabCode, team.TeamAssignment.TeamAuth);
                assignment.TeamAssignment.DnsTxtRecord = null;
                await LabRepo.UpdateTeamAssignment(assignment.TeamAssignment);
                res.ResponseMessage = "TXT record reset";
                res.Settings = await LabRepo.GetLabAndSettings(team.Lab.Id);
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
                var assignment = await LabRepo.ResetTeamCode(team);
                
                res.Settings = await LabRepo.GetLabAndSettings(team.Lab.Id);
                res.ResponseMessage = assignment.TeamAssignment.TeamAuth;
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
