using DocDBLib;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Lab.Data.Models
{
    public class DomainResourceGroup : DocModelBase, IDocModelBase
    {
        [JsonProperty("azureSubscriptionId")]
        public string AzureSubscriptionId { get; set; }

        [JsonProperty("dnsZoneRg")]
        public string DnsZoneRG { get; set; }

        [JsonProperty("domainList")]
        public List<string> DomainList { get; set; }

        public DomainResourceGroup()
        {
        }

        public DomainResourceGroup(string subscriptionId, string dnsZoneRG)
        {
            AzureSubscriptionId = subscriptionId;
            DnsZoneRG = dnsZoneRG;
            DomainList = new List<string>();
        }
    }
}
