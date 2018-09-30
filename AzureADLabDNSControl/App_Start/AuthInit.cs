using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using AzureADLabDNSControl.Infra;
using Graph;
using Infra;
using Microsoft.Owin.Security.Cookies;

namespace AzureADLabDNSControl
{
    public partial class Startup
    {
        internal static async Task AuthInit(CookieResponseSignInContext ctx)
        {
            var hctx =
                (HttpContextWrapper)
                    ctx.OwinContext.Environment.Single(e => e.Key == "System.Web.HttpContextBase").Value;

            var aud = ctx.Identity.GetClaim("aud");
            if (aud == LabUserClientId)
            {
                ctx.Identity.AddClaim(new Claim(CustomClaimTypes.AuthType, CustomAuthType.LabUser));
                ctx.Identity.AddClaim(new Claim(ClaimTypes.Role, CustomRoles.LabUser));

                //call Graph to get additional custom claims
                var tenantId = AdalLib.GetUserTenantId(ctx.Identity);
                var oid = ctx.Identity.GetClaim(CustomClaimTypes.ObjectIdentifier);
                var control = await AADLinkControl.CreateAsync(tenantId, hctx);
                var codes = await control.GetCodes(oid);
                ctx.Identity.AddClaim(new Claim(CustomClaimTypes.LabCode, codes.labCode));
                ctx.Identity.AddClaim(new Claim(CustomClaimTypes.TeamCode, codes.teamCode));
                if (codes.teamCode != null)
                {
                    ctx.Identity.AddClaim(new Claim(ClaimTypes.Role, CustomRoles.LabUserAssigned));
                }

                //add these to session too
                hctx.Session["labCode"] = codes.labCode;
                hctx.Session["teamCode"] = codes.teamCode;

            }
            else if (aud == LabAdminClientId)
            {
                ctx.Identity.AddClaim(new Claim(CustomClaimTypes.AuthType, CustomAuthType.LabAdmin));
                ctx.Identity.AddClaim(new Claim(ClaimTypes.Role, CustomRoles.LabAdmin));
            }
        }
    }
}