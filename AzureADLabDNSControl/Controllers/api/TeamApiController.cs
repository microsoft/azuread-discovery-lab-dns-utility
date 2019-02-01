using Graph;
using Lab.Common;
using Lab.Common.Infra;
using Lab.Common.Repo;
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
    [Authorize(Roles = CustomRoles.LabUser)]
    public class TeamApiController : ApiController
    {
        [HttpGet]
        public async Task<AdalResponse<Graph.Models.Domain>> CheckDomainValidation()
        {
            var labCode = User.Identity.GetClaim(CustomClaimTypes.LabCode);
            var teamCode = User.Identity.GetClaim(CustomClaimTypes.TeamCode);

            var team = await LabRepo.GetDomAssignment(labCode, teamCode);
            var tenantId = AdalLib.GetUserTenantId(User.Identity);
            var hctx = new HttpContextWrapper(System.Web.HttpContext.Current);

            var control = await AADLinkControl.CreateAsync(tenantId, hctx);
            return await control.GetDomain(team.TeamAssignment.DomainName);
        }
    }
}
