using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocDBLib;
using System.Linq.Expressions;
using Lab.Data.Models;

namespace Lab.Common.Repo
{
    public class RGRepo : BaseRepo<DomainResourceGroup>, IRepo<DomainResourceGroup>
    {
        public string ErrorMsg { get; set; }

        public override async Task<int> Delete(string id)
        {
            DomainResourceGroup item = await GetAsync(id);
            //see if group is being referenced in any future scheduled events
            var events = await DocDBRepo.DB<LabSettings>.GetItemsAsync(l => l.DnsZoneRG == item.DnsZoneRG && l.LabDate >= DateTime.UtcNow);
            if (events.Count() > 0)
            {
                var msg = string.Format("Error deleting resource group: one or more future scheduled labs are dependent on resource \"{0}\". Please clear the labs first.", item.DnsZoneRG);
                throw new Exception(msg);
            }
            await DocDBRepo.DB<DomainResourceGroup>.DeleteItemAsync(item);
            return 1;
        }

        public async Task<DomainResourceGroup> GetGroup(string subId, string zoneRg)
        {
            var res = await DocDBRepo.DB<DomainResourceGroup>.GetItemsAsync(g => g.AzureSubscriptionId == subId && g.DnsZoneRG == zoneRg);
            return res.SingleOrDefault();
        }
    }
}
