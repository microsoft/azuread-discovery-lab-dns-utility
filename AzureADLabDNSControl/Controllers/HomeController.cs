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
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(TeamAuthentication auth)
        {
            var data = await TableStorage.GetDomAssignment(auth.LabCode, auth.TeamAuth);
            if (data == null)
            {
                ViewBag.ErrorHeader = "Invalid Codes";
                ViewBag.Error = "Sorry, your codes don't match - please confirm and try again.";
                return View("Index");
            }
            if (data.DnsTxtRecord != null)
            {
                ViewBag.ErrorHeader = "Configuration Completed";
                ViewBag.Error = "You have already configured your custom domain. Please ask your instructor to reset if you need to update the information.";
                return View("Index");
            }
            return View("TeamLogin", data);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Azure Active Directory Lab Utility";

            return View();
        }
    }
}