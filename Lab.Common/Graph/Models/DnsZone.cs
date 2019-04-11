using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab.Common.Graph.Models
{
    public class DnsZones
    {
        [JsonProperty(PropertyName = "value")]
        public List<DnsZone> Value { get; set; }

        [JsonProperty(PropertyName = "nextLink")]
        public string NextLink { get; set; }
    }

    public class DnsZone
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public Dictionary<string,string> Tags { get; set; }
    }
}

/*
{
    "id":"/subscriptions/aed7eb10-0c55-4e2f-9789-56a40fe42f16/resourceGroups/aadlabdns/providers/Microsoft.Network/dnszones/aadpoc.net",
    "name":"aadpoc.net",
    "type":"Microsoft.Network/dnszones",
    "etag":"00000002-0000-0000-79b5-a6860839d401",
    "location":"global",
    "tags":
        {
            "InUse":"false",
            "Project":"",
            "ProjectOwnerAlias":"",
            "RootLabDomain":"true"
        },
    "properties":
        {
            "maxNumberOfRecordSets":5000,
            "maxNumberOfRecordsPerRecordSet":null,
            "nameServers":
                ["ns1-05.azure-dns.com.","ns2-05.azure-dns.net.","ns3-05.azure-dns.org.","ns4-05.azure-dns.info."],
            "numberOfRecordSets":12,
            "zoneType":"Public"
        }
    }
*/
