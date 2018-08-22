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
            return View("TeamLogin", data);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Azure Active Directory Lab Utility";

            return View();
        }
    }
    public class TeamAuthentication
    {
        public string LabCode { get; set; }
        public string TeamAuth { get; set; }
    }
}