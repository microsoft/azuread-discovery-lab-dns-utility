using AzureADLabDNSControl.Infra;
using AzureADLabDNSControl.Models;
using Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AzureADLabDNSControl.Controllers.api
{
    [Authorize(Roles = CustomRoles.LabAdmin)]
    public class LabController : ApiController
    {
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
            var res = await LabRepo.UpdateLab(lab);
            return MapLabs(res);
        }

        [HttpPost]
        public async Task<IEnumerable<LabSettings>> ResetLabCode(LabSettings lab)
        {
            var res = await LabRepo.ResetLabCode(lab);
            return res;
        }

        [HttpPost]
        public async Task UnlinkAllDomains()
        {

        }
        
        [HttpPost]
        public async Task UnlinkDomain(string domainName)
        {

        }

        [HttpPost]
        public async Task<LabSettings> ResetAssignment(TeamDTO team)
        {
            //detach domain from tenant
            var tenantId = User.Identity.GetClaim(CustomClaimTypes.TenantId);

            var control = await AADLinkControl.CreateAsync(tenantId, new HttpContextWrapper(HttpContext.Current));
            var adalResponse = await control.DeleteDomain(tenantId, team.TeamAssignment.DomainName);
            
            //remove zone record from zone
            using (var dns = new DnsAdmin())
            {
                await dns.ClearTxtRecord(team.TeamAssignment.DomainName);
            }

            //update record in Cosmos
            var res = await LabRepo.ResetAssignment(team);
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
    }
}
