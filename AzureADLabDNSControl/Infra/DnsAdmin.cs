using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AzureADLabDNSControl;
using Infra;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
//using Microsoft.Azure.Management.Dns.Fluent;
//using Microsoft.Azure.Management.Dns.Fluent.Models;
using Microsoft.Rest.Azure.Authentication;
using Newtonsoft.Json;

namespace Infra
{
    public class DnsAdmin : IDisposable
    {
        private DnsManagementClient _client;

        public DnsAdmin()
        {
            var task = Task.Run(async () => {
                await InitAsync();
            });
            task.Wait();
        }

        private async Task InitAsync()
        {
            // Build the service credentials and DNS management client
            var serviceCreds = await ApplicationTokenProvider.LoginSilentAsync(Startup.LabAdminTenantId, Startup.LabAdminClientId, Startup.LabAdminSecret);
            _client = new DnsManagementClient(serviceCreds)
            {
                SubscriptionId = Settings.AzureSubscriptionId
            };

        }

        public async Task<IEnumerable<Zone>> GetZoneList()
        {
            var res = await _client.Zones.ListByResourceGroupAsync(Settings.DnsZoneRG);
            return res.ToList();
        }

        public async Task ResetAllZones()
        {
            try
            {
                var zones = await GetZoneList();
                foreach (var zone in zones)
                {
                    var RSList = await _client.RecordSets.ListAllByDnsZoneAsync(Settings.DnsZoneRG, zone.Name.ToString());
                    foreach (var rs in RSList)
                    {
                        if (rs.Type == "NS") continue;
                        if (rs.Type == "SOA") continue;
                        RecordType t = (RecordType)Enum.Parse(typeof(RecordType), rs.Type);
                        await _client.RecordSets.DeleteAsync(Settings.DnsZoneRG, zone.Name, rs.Name, t);
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task ClearTxtRecord(string zoneName)
        {
            await _client.RecordSets.DeleteAsync(Settings.DnsZoneRG, zoneName, "@", RecordType.TXT);
        }

        public async Task RemoveChildZone(string childZoneName, string parentZoneName)
        {
            try
            {
                await _client.Zones.DeleteAsync(Settings.DnsZoneRG, childZoneName);
                await _client.RecordSets.DeleteAsync(Settings.DnsZoneRG, parentZoneName, childZoneName, RecordType.NS);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task CreateNewChildZone(string parentZoneName, string teamName, string newChildZoneName)
        {
            try
            {
                //get parent NS records
                var RS = await _client.RecordSets.GetAsync(Settings.DnsZoneRG, parentZoneName, "@", RecordType.NS);

                var parms = new RecordSet();
                parms.TTL = 3600;

                parms.NsRecords = new List<NsRecord>();

                foreach (var item in RS.NsRecords)
                {
                    parms.NsRecords.Add(item);
                }
                //create new NS record in parent for child domain
                await _client.RecordSets.CreateOrUpdateAsync(Settings.DnsZoneRG, parentZoneName, teamName, RecordType.NS, parms);

                //create new child zone
                var zone = new Zone("global");
                zone.ZoneType = ZoneType.Public;
                var newZone = await _client.Zones.CreateOrUpdateAsync(Settings.DnsZoneRG, newChildZoneName, zone);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task SetTxtRecord(string record, string zoneName)
        {
            try
            {
                var recordSetParams = new RecordSet();
                recordSetParams.TTL = 3600;

                recordSetParams.TxtRecords = new List<TxtRecord>();
                var txt = new TxtRecord();
                txt.Value = new List<string>();
                txt.Value.Add(record);
                recordSetParams.TxtRecords.Add(txt);

                // Create the actual record set in Azure DNS
                var recordSet = await _client.RecordSets.CreateOrUpdateAsync(Settings.DnsZoneRG, zoneName, "@", RecordType.TXT, recordSetParams);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
    public class TxtRecs
    {
        [JsonProperty(PropertyName = "value")]
        public List<string> Value { get; set; }
        public TxtRecs(string value)
        {
            Value = new List<string>();
            Value.Add(value);
        }
    }

    public class DNSProps
    {
        public int TTL { get; set; }
        public List<TxtRecs> TxtRecords {get; set;}
        public DNSProps()
        {
            TxtRecords = new List<TxtRecs>();
        }
    }

    public class DNSBody
    {
        [JsonProperty(PropertyName = "properties")]
        public DNSProps Properties { get; set; }
        public DNSBody()
        {
            Properties = new DNSProps();
        }
    }
}