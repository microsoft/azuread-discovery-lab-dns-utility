using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Graph;
using System.Security.Principal;
using Lab.Common;
using Lab.Common.Repo;
using Lab.Common.Infra;

namespace AzureADLabDNSControl.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.Cookies["tzo"] == null)
            {
                if (Session["loadTX"] != null && Session["loadTX"].ToString() == "true")
                {
                    return View("NeedJS");
                }
                Session["loadTZ"] = true;
                return View("Reload");
            }
            int tzo = int.Parse(Request.Cookies["tzo"].Value);

            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole(CustomRoles.LabAdmin))
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Team");
            }

            //todo: setup a web job to check for a lab and set it in a static var daily
            //var lab = await LabRepo.GetTodaysLab(tzo);
           // var isLive = (lab.Count() == 1);
            //ViewBag.IsLive = isLive;
            //var data = (isLive) ? lab.Single() : null;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Azure Active Directory Lab Utility";

            return View();
        }

        [Authorize]
        public ActionResult Claims()
        {
            return View();
        }
        public ActionResult Error()
        {
            return View();
        }

        public ActionResult IssueInfo()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> IssueInfo(string comments)
        {
            if (HttpContext.Session == null) return View();

            var eid = (HttpContext.Session != null && HttpContext.Session["ErrorID"] != null)
                ? HttpContext.Session["ErrorID"].ToString()
                : Request.Form["et"];

            var emgr = new ErrorMgr(new RequestDTO(HttpContext));
            try
            {
                var eo = await emgr.ReadError(eid, false);
                if (eo != null)
                {
                    eo.UserComment = comments;
                    await emgr.SaveError(eo);
                }
                else
                {
                    //Writing to node WEL
                    emgr.WriteToAppLog("Unable to save user comments to. Comment: " + comments, System.Diagnostics.EventLogEntryType.Error);
                }
            }
            catch (Exception ex)
            {
                //Writing to node WEL
                emgr.WriteToAppLog("Unable to save user comments. \r\nError: " + ex.Message + ". \r\nComment: " + comments, System.Diagnostics.EventLogEntryType.Error);
            }

            return View();
        }
    }
    public class ErrorInfo
    {
        public string ErrorHeader { get; set; }
        public string Error { get; set; }
    }
}