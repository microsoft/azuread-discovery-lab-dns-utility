using AzureADLabDNSControl.Infra;
using Lab.Common;
using Lab.Common.Repo;
using Lab.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AzureADLabDNSControl.Controllers
{
    [AdminAuthorize(Roles = CustomRoles.LabAdmin)]
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

        public async Task<ActionResult> ExportTenants(string id)
        {
            var lab = await LabRepo.GetLabAndSettings(id);
            var teams = lab.DomAssignments;
            var res = new List<string>
            {
                "Tenant,AdminUpn,TenantID,AssignedDNS"
            };
            res.AddRange(teams.Select(t => t.AssignedTenantName + "," + t.TenantAdminUpn + "," + t.AssignedTenantId + "," + t.DomainName));
            var city = lab.City.ToLower().Replace(" ", "").Replace(".", "").Replace("-", "");
            city += (lab.LabDate.Month.ToString() + lab.LabDate.Day.ToString());

            var fileArray = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, res.ToArray()));
            var fileName = string.Format("Lab-{0}.csv", city);
            string contentType = MimeMapping.GetMimeMapping(fileName);
            var cd = new ContentDisposition
            {
                FileName = fileName,
                Inline = false
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());
            return File(fileArray, contentType);
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