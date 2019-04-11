using System;
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

        [JsonProperty("ownerAlias")]
        public string OwnerAlias { get; set; }

        [JsonProperty("createDate")]
        public DateTime CreateDate { get; set; }

        [JsonProperty("shared")]
        public bool Shared { get; set; }

        public DomainResourceGroup()
        {
        }

        public DomainResourceGroup(string subscriptionId, string dnsZoneRG, string ownerAlias)
        {
            AzureSubscriptionId = subscriptionId;
            OwnerAlias = ownerAlias;
            DnsZoneRG = dnsZoneRG;
            DomainList = new List<string>();
            CreateDate = DateTime.UtcNow;
        }
    }
}
