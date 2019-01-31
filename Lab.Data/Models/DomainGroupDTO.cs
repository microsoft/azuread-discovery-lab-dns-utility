using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab.Data.Models
{
    public class DomainGroupDTO
    {
        public string AzureSubscriptionId { get; set; }
        public string DnsZoneRG { get; set; }
        public int ZoneCount { get; set; }

        public DomainGroupDTO()
        {

        }
        public DomainGroupDTO(string subId, string zone)
        {
            AzureSubscriptionId = subId;
            DnsZoneRG = zone;
        }
    }
}
