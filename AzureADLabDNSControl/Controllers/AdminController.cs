using AzureADLabDNSControl.Infra;
using Lab.Common;
using Lab.Common.Repo;
using Lab.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AzureADLabDNSControl.Controllers
{
    [Authorize(Roles = CustomRoles.LabAdmin)]
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            var groups = Settings.DomainGroups.Select(d => new DomainGroupDTO(d.AzureSubscriptionId, d.DnsZoneRG));
            var data = SiteUtils.LoadListFromDictionary(groups.ToDictionary(o => o.AzureSubscriptionId + ":" + o.DnsZoneRG, o => o.DnsZoneRG));
            return View(data);
        }

        public async Task<ActionResult> LabReport(string id)
        {
            var lab = await LabRepo.GetLabAndSettings(id);
            return View(lab);
        }
    }
    public class DomainGroupDTO
    {
        public string AzureSubscriptionId { get; set; }
        public string DnsZoneRG { get; set; }

        public DomainGroupDTO(string subId, string zone)
        {
            AzureSubscriptionId = subId;
            DnsZoneRG = zone;
        }
    }
}