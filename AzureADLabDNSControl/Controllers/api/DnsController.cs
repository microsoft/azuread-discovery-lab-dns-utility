using Lab.Common;
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
    [Authorize(Roles = CustomRoles.LabAdmin)]
    public class DnsController : ApiController
    {
        public async Task UpdateTxtRecord(DnsDTO data)
        {
            using (var dns = new DnsAdmin())
            {
                await dns.SetTxtRecord(data.TxtRecord, data.DomainName);
            }
        }
    }


}
