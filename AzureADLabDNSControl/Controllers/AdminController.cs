using AzureADLabDNSControl.Infra;
using Lab.Common;
using Lab.Common.Repo;
using Lab.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AzureADLabDNSControl.Reports;

namespace AzureADLabDNSControl.Controllers
{
    [AdminAuthorize(Roles = CustomRoles.LabAdmin)]
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public async Task<FileResult> LabReport(string id)
        {
            var lab = await LabRepo.GetLabAndSettings(id);
            var res = RenderRazorViewToString("LabReport", lab);

            var fileName = string.Format("LabReport-{0}.pdf", lab.LabName);
            string contentType = MimeMapping.GetMimeMapping(fileName);
            var cd = new ContentDisposition
            {
                FileName = fileName,
                Inline = false
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());
            return File(Generator.GenReport(res), contentType);
        }

        public async Task<ActionResult> LabReportCsv(string id)
        {
            var lab = await LabRepo.GetLabAndSettings(id);
            var teams = lab.DomAssignments;

            var res = new List<string>
            {
                "City,Date,Instructor,LabCode"
            };
            res.Add(string.Format("{0},{1},{2},{3}", lab.City, lab.LabDate, lab.PrimaryInstructor, lab.LabCode));
            res.Add("");

            res.Add("Domain,TeamAuthKey");
            res.AddRange(teams.Select(t => string.Format("{0},{1}", t.DomainName, t.TeamAuth)));

            var fileArray = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, res.ToArray()));
            var fileName = string.Format("Lab-{0}.csv", lab.LabName);
            string contentType = MimeMapping.GetMimeMapping(fileName);
            var cd = new ContentDisposition
            {
                FileName = fileName,
                Inline = false
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());
            return File(fileArray, contentType);
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

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}