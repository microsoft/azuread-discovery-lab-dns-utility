using AzureADLabDNSControl.Infra;
using Lab.Common;
using Lab.Common.Repo;
using Lab.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AzureADLabDNSControl.Controllers.api
{
    [AdminAuthorize(Roles = CustomRoles.LabAdmin)]
    public class DnsController : ApiController
    {
        public async Task UpdateTxtRecord(DnsDTO data)
        {
            using (var dns = new DnsAdmin())
            {
                await dns.SetTxtRecord(data.TxtRecord, data.DomainName);
            }
        }

        public async Task<IEnumerable<System.Web.Mvc.SelectListItem>> GetDnsResourceGroups()
        {
            var groups = await LabRepo.GetLabStats();
            var data = SiteUtils.LoadListFromDictionary(groups.ToDictionary(o => o.AzureSubscriptionId + ":" + o.DnsZoneRG, o => o.DnsZoneRG + " (" + o.ZoneCount + " assigned)"));
            return data;
        }
    }
}
