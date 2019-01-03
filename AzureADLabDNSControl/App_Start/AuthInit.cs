using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Graph;
using Lab.Common;
using Lab.Common.Infra;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;

namespace AzureADLabDNSControl
{
    public partial class Startup
    {
        internal static async Task AuthInit(CookieResponseSignInContext ctx)
        {
            var hctx =
                (HttpContextWrapper)
                    ctx.OwinContext.Environment.Single(e => e.Key == "System.Web.HttpContextBase").Value;
            await AuthInit(hctx, ctx.Identity);
        }

        internal static async Task AuthInit(HttpContextWrapper hctx, ClaimsIdentity identity)
        {

            var aud = identity.GetClaim("aud");
            if (aud == Settings.LabUserClientId)
            {
                identity.AddClaim(new Claim(CustomClaimTypes.AuthType, CustomAuthType.LabUser));
                identity.AddClaim(new Claim(ClaimTypes.Role, CustomRoles.LabUser));

                //call Graph to get additional custom claims
                var tenantId = AdalLib.GetUserTenantId(identity);
                var oid = identity.GetClaim(CustomClaimTypes.ObjectIdentifier);
                var control = await AADLinkControl.CreateAsync(tenantId, hctx);
                var codes = await control.GetCodes(oid);
                if (codes != null)
                {
                    identity.AddClaim(new Claim(CustomClaimTypes.LabCode, codes.labCode));
                    identity.AddClaim(new Claim(CustomClaimTypes.TeamCode, codes.teamCode));
                    if (codes.teamCode != null)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, CustomRoles.LabUserAssigned));
                    }
                    //add these to session too
                    hctx.Session["labCode"] = codes.labCode;
                    hctx.Session["teamCode"] = codes.teamCode;
                }
            }
            else if (aud == Settings.LabAdminClientId)
            {
                identity.AddClaim(new Claim(CustomClaimTypes.AuthType, CustomAuthType.LabAdmin));
                identity.AddClaim(new Claim(ClaimTypes.Role, CustomRoles.LabAdmin));
            }
        }
    }
}