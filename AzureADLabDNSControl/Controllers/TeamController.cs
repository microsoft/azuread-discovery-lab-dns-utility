using AzureADLabDNSControl.Infra;
using AzureADLabDNSControl.Models;
using Graph;
using Infra;
using System;
using System.Collections.Generic;
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
                } else
                {
                    return RedirectToAction("SignIn", "Account", routeValues: new { force = "true"});
                }

                team = await LabRepo.GetDomAssignment(labCode, teamCode);
                if (team == null)
                {
                    //something weird has happened with the team assignment when this user registered
                    //go re-register
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
            var oid = User.Identity.GetClaim(CustomClaimTypes.ObjectIdentifier);
            var control = await AADLinkControl.CreateAsync(tenantId, HttpContext);

            await control.LinkUserToTeam(oid, auth.TeamAuth, auth.LabCode);
            await LabRepo.UpdateTenantId(new TeamDTO { Lab = data.Lab, TeamAssignment = data.TeamAssignment }, tenantId);

            //add these to session too
            Session["labCode"] = auth.LabCode;
            Session["teamCode"] = auth.TeamAuth;
            Session["labId"] = data.Lab.Id;

            ViewBag.LabId = data.Lab.Id;
            var res = DnsDTO.FromTeamDTO(data);
            return View(res);
        }

        /// <summary>
        /// Postback from Index view
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> UpdateAssignment(DnsDTO item)
        {
            //domain name, lab id missing, deleted from form
            //var data = await LabRepo.GetDomAssignment(item);

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