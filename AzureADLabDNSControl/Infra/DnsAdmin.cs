using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AzureADLabDNSControl;
using Infra;
using Microsoft.Azure.Management.Dns.Fluent;
using Microsoft.Azure.Management.Dns.Fluent.Models;
using Microsoft.Rest.Azure.Authentication;

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
            var serviceCreds = await ApplicationTokenProvider.LoginSilentAsync(Startup.tenantId, Startup.clientId, Startup.clientSecret);
            _client = new DnsManagementClient(serviceCreds)
            {
                SubscriptionId = Settings.AzureSubscriptionId
            };
            
        }

        public async Task<IEnumerable<ZoneInner>> GetZoneList()
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

        public async Task SetTxtRecord(string record, string zoneName)
        {
            try
            {
                RecordSetInner parms = new RecordSetInner();
                TxtRecord txt = new TxtRecord();
                txt.Value.Add(record);
                parms.TxtRecords.Add(txt);
                var res = await _client.RecordSets.CreateOrUpdateAsync(Settings.DnsZoneRG, zoneName, "@", RecordType.TXT, parms);
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
}