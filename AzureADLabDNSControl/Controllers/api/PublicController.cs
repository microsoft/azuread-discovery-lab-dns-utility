using Lab.Common;
using Lab.Common.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AzureADLabDNSControl.Controllers.api
{
    public class PublicController : ApiController
    {

        public async Task GetThing(int offset)
        {
            var lab = await LabRepo.GetTodaysLab(offset);
        }
    }
}
