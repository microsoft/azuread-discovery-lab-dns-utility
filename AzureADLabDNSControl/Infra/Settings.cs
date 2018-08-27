using DocDBLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Infra
{
    public static class Settings
    {
        public static string AzureSubscriptionId { get; set; }
        public static string DnsZoneRG { get; set; }

        public static List<string> DomainList { get; set; }

        public static async Task Init(NameValueCollection appSettings)
        {
            AzureSubscriptionId = appSettings["AzureSubscriptionId"];
            DnsZoneRG = appSettings["DnsZoneRGName"];

            //DocDB config
            DocDBRepo.Settings.DocDBUri = appSettings["DocDBUri"];
            DocDBRepo.Settings.DocDBAuthKey = appSettings["DocDBAuthKey"];
            DocDBRepo.Settings.DocDBName = appSettings["DocDBName"];
            DocDBRepo.Settings.DocDBCollection = appSettings["DocDBCollection"];

            var client = DocDBRepo.Initialize().Result;
            var s = client.AuthKey;

            DomainList = new List<string>();
            using (var dns = new DnsAdmin())
            {
                var zones = await dns.GetZoneList();
                foreach(var zone in zones)
                {
                    DomainList.Add(zone.Name);
                }
            }
        }
    }
}