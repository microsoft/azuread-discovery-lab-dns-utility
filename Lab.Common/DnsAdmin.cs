using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Lab.Data.Models;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Rest.Azure.Authentication;
using Newtonsoft.Json;

namespace Lab.Common
{
    public class DnsAdmin : IDisposable
    {
        private DnsManagementClient _client;
        private DomainResourceGroup _domainRG;
        private bool _isInit;

        private async Task _initAsync(DomainResourceGroup domainGroup)
        {
            _domainRG = domainGroup;

            // Build the service credentials and DNS management client
            var serviceCreds = await ApplicationTokenProvider.LoginSilentAsync(Settings.LabAdminTenantId, Settings.LabAdminClientId, Settings.LabAdminSecret);
            _client = new DnsManagementClient(serviceCreds)
            {
                SubscriptionId = _domainRG.AzureSubscriptionId
            };
            _isInit = true;
        }

        public async Task InitAsync(DomainResourceGroup domainGroup)
        {
            await _initAsync(domainGroup);
        }

        private void CheckInit()
        {
            if (!_isInit)
            {
                throw new Exception("The DNSAdmin utility has to be initialized asynchronously with a DomainResourceGroup object.");
            }
        }

        public async Task<IEnumerable<Zone>> GetZoneList()
        {
            try
            {
                CheckInit();

                var res = await _client.Zones.ListByResourceGroupAsync(_domainRG.DnsZoneRG);
                var res2 = res.Where(d => d.Tags.Any(t => t.Key == "RootLabDomain" && t.Value == "true")).ToList();
                return res2;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ResetAllZones()
        {
            CheckInit();

            try
            {
                var zones = await GetZoneList();
                foreach (var zone in zones)
                {
                    var RSList = await _client.RecordSets.ListAllByDnsZoneAsync(_domainRG.DnsZoneRG, zone.Name.ToString());
                    foreach (var rs in RSList)
                    {
                        if (rs.Type == "NS") continue;
                        if (rs.Type == "SOA") continue;
                        RecordType t = (RecordType)Enum.Parse(typeof(RecordType), rs.Type);
                        await _client.RecordSets.DeleteAsync(_domainRG.DnsZoneRG, zone.Name, rs.Name, t);
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
            CheckInit();
            await _client.RecordSets.DeleteAsync(_domainRG.DnsZoneRG, zoneName, "@", RecordType.TXT);
        }

        public async Task RemoveChildZone(string parentZoneName, string teamName, string childZoneName)
        {
            CheckInit();

            try
            {
                await _client.Zones.DeleteAsync(_domainRG.DnsZoneRG, childZoneName);
                await _client.RecordSets.DeleteAsync(_domainRG.DnsZoneRG, parentZoneName, teamName, RecordType.NS);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task CreateNewChildZone(string parentZoneName, string teamName, string newChildZoneName)
        {
            CheckInit();

            try
            {
                //create new child zone
                var zone = new Zone("global");
                zone.ZoneType = ZoneType.Public;
                var newZone = await _client.Zones.CreateOrUpdateAsync(_domainRG.DnsZoneRG, newChildZoneName, zone);

                var parms = new RecordSet();
                parms.TTL = 3600;

                parms.NsRecords = new List<NsRecord>();

                foreach (var item in newZone.NameServers)
                {
                    parms.NsRecords.Add(new NsRecord(item));
                }

                //create new NS record in parent for child domain
                await _client.RecordSets.CreateOrUpdateAsync(_domainRG.DnsZoneRG, parentZoneName, teamName, RecordType.NS, parms);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task SetTxtRecord(string record, string zoneName)
        {
            CheckInit();

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
                var recordSet = await _client.RecordSets.CreateOrUpdateAsync(_domainRG.DnsZoneRG, zoneName, "@", RecordType.TXT, recordSetParams);
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