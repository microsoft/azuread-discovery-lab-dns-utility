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
    [Authorize]
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        [Route("Admin/LabReport/{labDate}")]
        public async Task<ActionResult> LabReport(string labDate)
        {
            var lab = await TableStorage.GetLab(User.Identity.Name, DateTime.Parse(labDate));
            return View(lab);
        }
    }
}