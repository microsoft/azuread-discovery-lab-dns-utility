using AzureADLabDNSControl.Models;
using Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AzureADLabDNSControl.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Admin");
            }

            //todo: setup a web job to check for a lab and set it in a static var daily
            var lab = await LabRepo.GetTodaysLab();
            ViewBag.IsLive = (lab != null);
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
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

            await LabRepo.UpdateTeamSession(data, Session.SessionID);
            Session["labId"] = data.Lab.Id;
            ViewBag.LabId = data.Lab.Id;
            var res = DnsDTO.FromTeamDTO(data);
            return View("TeamLogin", res);
        }

        [Route("~/TeamLogin")]
        public async Task <ActionResult> TeamLogin()
        {
            var data = await LabRepo.GetDomAssignment(Session.SessionID, Session["labId"]);
            if (data == null)
            {
                ViewBag.ErrorHeader = "Session Expired";
                ViewBag.Error = "Your session has expired - please enter your codes again.";
                ViewBag.IsLive = true;
                return View("Index");
            }

            var res = DnsDTO.FromTeamDTO(data);
            return View(res);
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
            if (data.TeamAssignment.SessionID != null && data.TeamAssignment.SessionID != Session.SessionID)
            {
                return new ErrorInfo
                {
                    ErrorHeader = "Unauthorized",
                    Error = "You are logging in from a different system than the one you started with. If you need to restart, please ask your instructor to reset your team."
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

        [HttpPost]
        public async Task<ActionResult> UpdateAssignment(DnsDTO item)
        {
            var data = await LabRepo.GetDomAssignment(item);

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
                    await dns.SetTxtRecord(item.TxtRecord, item.DomainName);
                };
                //updating 
                await LabRepo.UpdateDnsRecord(data, item.TxtRecord);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorHeader = "DNS Update Failed";
                ViewBag.Error = ex.Message;
                item.TxtRecord = "";
            }
            return View("TeamLogin", item);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Azure Active Directory Lab Utility";

            return View();
        }
    }
    public class ErrorInfo
    {
        public string ErrorHeader { get; set; }
        public string Error { get; set; }
    }
}