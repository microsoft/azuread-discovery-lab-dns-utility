using AzureADLabDNSControl.Infra;
using Lab.Common;
using Lab.Common.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AzureADLabDNSControl.Controllers
{
    [AdminAuthorize(Roles = CustomRoles.LabAdmin)]
    public class ResourceGroupController : Controller
    {
        // GET: ResourceGroup
        public ActionResult Index()
        {
            return View();
        }
    }
}