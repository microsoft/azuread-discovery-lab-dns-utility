using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using AzureADLabDNSControl.Infra;
using DocDBLib;
using Graph;
using Lab.Common;
using Lab.Common.Infra;
using Lab.Data.Models;
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
                var tenantName = AdalLib.GetUserUPNSuffix(identity);
                var oid = identity.GetClaim(CustomClaimTypes.ObjectIdentifier);
                var team = (await DocDBRepo.DB<DomAssignment>.GetItemsAsync(d => d.AssignedTenantName == tenantName)).FirstOrDefault();

                //var control = await AADLinkControl.CreateAsync(tenantId, hctx);
                //var codes = await control.GetCodes(oid);
                if (team != null)
                {
                    identity.AddClaim(new Claim(CustomClaimTypes.LabCode, team.LabCode));
                    identity.AddClaim(new Claim(CustomClaimTypes.TeamCode, team.TeamAuth));
                    identity.AddClaim(new Claim(CustomClaimTypes.TenantName, tenantName));
                    if (team.TeamAuth != null)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, CustomRoles.LabUserAssigned));
                    }
                    //add these to session too
                    //SiteUtils.UpsertCookie(hctx, "labCode", codes.labCode);
                    //SiteUtils.UpsertCookie(hctx, "teamCode", codes.teamCode);
                    //SiteUtils.UpsertCookie(hctx, "tenantName", tenantName);

                    hctx.Session["labCode"] = team.LabCode;
                    hctx.Session["teamCode"] = team.TeamAuth;
                    hctx.Session["tenantName"] = tenantName;
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