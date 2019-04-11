using AzureADLabDNSControl.Infra;
using Infra.Auth;
using Lab.Common;
using Lab.Common.Repo;
using Lab.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace AzureADLabDNSControl.Controllers.api
{
    [AdminAuthorize(Roles = CustomRoles.LabAdmin)]
    public class RGAPIController : ApiController
    {
        private RGRepo _repo;
        private ArmApi _api;
        private bool _isSiteAdmin;

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            _repo = new RGRepo();
            var oid = User.Identity.GetClaim(TokenCacheClaimTypes.ObjectId);
            _isSiteAdmin = User.Identity.IsInRole(CustomRoles.SiteAdmin);

            var host = Utils.GetFQDN(controllerContext);
            _api = new ArmApi(oid, controllerContext);
            base.Initialize(controllerContext);
        }

        [HttpGet]
        public async Task<IEnumerable<DomainResourceGroup>> GetItems()
        {
            IEnumerable<DomainResourceGroup> RGs;
            RGs = await ((_isSiteAdmin) ? _repo.GetItemsAsync() : _repo.GetItemsAsync(g => g.OwnerAlias == User.Identity.Name));

            return RGs;
        }

        [HttpPost]
        public async Task<IEnumerable<DomainResourceGroup>> SaveRG(DomainResourceGroup rg)
        {
            rg.CreateDate = DateTime.UtcNow;
            rg.OwnerAlias = User.Identity.Name;
            var res = await _repo.Upsert(rg);
            return await GetItems();
        }

        [HttpPost]
        public async Task<IEnumerable<DomainResourceGroup>> RefreshDomains(DomainResourceGroup rg)
        {
            var zones = await _api.GetDNSZones(rg.AzureSubscriptionId, rg.DnsZoneRG);
            rg.DomainList.Clear();
            rg.DomainList.AddRange(zones.Where(z => z.Tags.Count > 0 && z.Tags["RootLabDomain"] == "true").Select(z => z.Name).ToList());
            var res = await _repo.Upsert(rg);
            return await GetItems();
        }

        [HttpPost]
        public async Task<IEnumerable<DomainResourceGroup>> DeleteRG(DomainResourceGroup rg)
        {
            var res = await _repo.Delete(rg.Id);
            return await GetItems();
        }

        [HttpGet]
        public async Task<dynamic> GetSubscriptions()
        {
            var res = await _api.GetSubscriptions();
            return res;
        }

        [HttpGet]
        public async Task<dynamic> GetRGs(string id)
        {
            var res = await _api.GetResourceGroups(id);
            return res;
        }

        [HttpGet]
        public async Task<dynamic> GetDnsZones([FromUri]string subid, string rgname)
        {
            var res = await _api.GetDNSZones(subid, rgname);
            return res;
        }
    }
}
