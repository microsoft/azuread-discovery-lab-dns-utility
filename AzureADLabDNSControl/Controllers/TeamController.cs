using AzureADLabDNSControl.Infra;
using Graph;
using Lab.Common;
using Lab.Common.Infra;
using Lab.Common.Repo;
using Lab.Data.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AzureADLabDNSControl.Controllers
{
    [Authorize(Roles = CustomRoles.LabUser)]
    public class TeamController : Controller
    {
        // GET: Team
        public async Task<ActionResult> Index()
        {
            TeamDTO team = null;

            if (User.IsInRole(CustomRoles.LabUserAssigned))
            {
                string labCode, teamCode;

                if (Session["labCode"] != null)
                {
                    labCode = Session["labCode"].ToString();
                    teamCode = Session["teamCode"].ToString();
                }
                else
                {
                    return RedirectToAction("SignIn", "Account", routeValues: new { force = "true"});
                }

                team = await LabRepo.GetDomAssignment(labCode, teamCode);
                if (team == null || team.TeamAssignment == null)
                {
                    //something weird has happened with the team assignment when this user registered
                    //go re-register
                    ViewBag.ErrorMessage = "Your saved team information has been changed, or expired. Please re-associate your login with your team information.";
                    return View("Register");
                }
                var res = DnsDTO.FromTeamDTO(team);
                return View(res);
            }

            return View("Register");
        }

        /// <summary>
        /// Direct call to re-register
        /// </summary>
        /// <returns></returns>
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Postback from Register view
        /// </summary>
        /// <param name="auth"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(TeamAuthentication auth)
        {
            var data = await LabRepo.GetDomAssignment(auth.LabCode, auth.TeamAuth);
            var test = ContinueEditingAssignment(data);
            if (test != null)
            {
                ViewBag.ErrorHeader = test.ErrorHeader;
                ViewBag.Error = test.Error;
                ViewBag.IsLive = true;
                return View("Index");
            }

            var tenantId = AdalLib.GetUserTenantId(User.Identity);
            var tenantName = AdalLib.GetUserUPNSuffix(User.Identity);
            var tenantAdmin = User.Identity.GetClaim(ClaimTypes.Upn);

            var oid = User.Identity.GetClaim(CustomClaimTypes.ObjectIdentifier);

            await LabRepo.UpdateTenantId(new TeamDTO { Lab = data.Lab, TeamAssignment = data.TeamAssignment }, tenantId, tenantName, tenantAdmin);

            return RedirectToAction("refresh", "account");
        }

        /// <summary>
        /// Postback from Index view
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> UpdateAssignment(DnsDTO item)
        {
            string labCode = Session["labCode"].ToString();
            string teamCode = Session["teamCode"].ToString();

            var data = await LabRepo.GetDomAssignment(labCode, teamCode);

            var test = ContinueEditingAssignment(data);
            if (test != null)
            {
                ViewBag.ErrorHeader = test.ErrorHeader;
                ViewBag.Error = test.Error;
                ViewBag.IsLive = true;
                return View("Index");
            }
            try
            {
                //updating DNS record
                using (var dns = new DnsAdmin())
                {
                    var domGroup = Settings.DomainGroups.Single(d => d.AzureSubscriptionId == data.Lab.AzureSubscriptionId && d.DnsZoneRG == data.Lab.DnsZoneRG);
                    await dns.InitAsync();
                    dns.SetClient(domGroup);
                    await dns.SetTxtRecord(item.TxtRecord, data.TeamAssignment.DomainName);
                };
                //updating 
                data.TeamAssignment.DnsTxtRecord = item.TxtRecord;
                await LabRepo.UpdateDnsRecord(data);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorHeader = "DNS Update Failed";
                ViewBag.Error = ex.Message;
                item.TxtRecord = "";
            }
            item.DomainName = data.TeamAssignment.DomainName;
            item.LabId = data.Lab.Id;
            return View("Index", item);
        }

        private ErrorInfo ContinueEditingAssignment(TeamDTO data)
        {
            ErrorInfo res = null;
            if (data == null || data.TeamAssignment == null)
            {
                return new ErrorInfo
                {
                    ErrorHeader = "Invalid Codes",
                    Error = "Sorry, your codes don't match - please confirm and try again."
                };
            }
            if (data.TeamAssignment.DnsTxtRecord != null)
            {
                return new ErrorInfo
                {
                    ErrorHeader = "Configuration Completed",
                    Error = "You have already configured your custom domain. Please ask your instructor to reset if you need to update the information."
                };
            }
            return res;
        }

    }
}