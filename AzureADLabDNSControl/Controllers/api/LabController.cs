using AzureADLabDNSControl.Models;
using Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AzureADLabDNSControl.Controllers.api
{
    [Authorize]
    public class LabController : ApiController
    {
        [HttpPost]
        public async Task<IEnumerable<LabDTO>> AddLab(NewLabDTO lab)
        {
            var newLab = new LabSettings(User.Identity.Name, lab.LabDate, lab.City);
            var data = await TableStorage.AddNewLab(newLab, Settings.DomainList.ToArray());
            return data;
        }

        public async Task<IEnumerable<LabDTO>> GetLabs()
        {
            var labList = await TableStorage.GetLabs(User.Identity.Name);
            return labList;
        }
    }
}
