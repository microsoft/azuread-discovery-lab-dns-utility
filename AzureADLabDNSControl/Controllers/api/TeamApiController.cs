using Graph;
using Lab.Common;
using Lab.Common.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AzureADLabDNSControl.Controllers.api
{
    [Authorize(Roles = CustomRoles.LabUser)]
    public class TeamApiController : ApiController
    {
        [HttpGet]
        public async Task<bool> CheckDomainValidation()
        {
            var labCode = User.Identity.GetClaim(CustomClaimTypes.LabCode);
            var teamCode = User.Identity.GetClaim(CustomClaimTypes.TeamCode);

            var team = await LabRepo.GetDomAssignment(labCode, teamCode);
            var tenantId = AdalLib.GetUserTenantId(User.Identity);
            var checkTenantId = AdalLib.GetAADTenantId(team.TeamAssignment.DomainName);
            return (tenantId == checkTenantId);
        }
    }
}
