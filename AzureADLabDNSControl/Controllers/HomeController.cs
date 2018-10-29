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

namespace AzureADLabDNSControl.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole(CustomRoles.LabAdmin))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (User.IsInRole(CustomRoles.LabUserAssigned))
                {
                    return RedirectToAction("Index", "Team");
                }
            }

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

            //todo: setup a web job to check for a lab and set it in a static var daily
            var lab = await LabRepo.GetTodaysLab(tzo);
            var isLive = (lab.Count() == 1);
            ViewBag.IsLive = isLive;
            var data = (isLive) ? lab.Single() : null;
            return View(data);
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
    }
    public class ErrorInfo
    {
        public string ErrorHeader { get; set; }
        public string Error { get; set; }
    }
}