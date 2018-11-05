using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocDBLib;
using Lab.Common;
using Lab.Data.Models;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace LabManageJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {

            var dir = AppContext.BaseDirectory;

            var task = Task.Run(async () => {
                await Init(ConfigurationManager.AppSettings, dir);
            });
            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error during initialization", System.Diagnostics.EventLogEntryType.Error, ex);
                throw ex;
            }

            var config = new JobHostConfiguration
            {
                DashboardConnectionString = Settings.StorageConnectionString,
                StorageConnectionString = Settings.StorageConnectionString
            };

            config.NameResolver = new CustomNameResolver();

            var host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }

        private static async Task Init(NameValueCollection appSettings, string appRoot)
        {
            //DocDB config
            DocDBRepo.Settings.AppRootPath = appRoot;
            DocDBRepo.Settings.DocDBUri = appSettings["DocDBUri"];
            DocDBRepo.Settings.DocDBAuthKey = appSettings["DocDBAuthKey"];
            DocDBRepo.Settings.DocDBName = appSettings["DocDBName"];
            DocDBRepo.Settings.DocDBCollection = appSettings["DocDBCollection"];

            //IMPORTANT: Set Regions before setting CurrentRegion
            DocDBRepo.Settings.DocDBRegions = (appSettings["DocDBRegions"] as string).Split(',');
            DocDBRepo.Settings.DocDBCurrentRegion = Environment.GetEnvironmentVariable("REGION_NAME");

            //Identity config
            Settings.LabAdminClientId = appSettings["ida:ClientId"];
            Settings.LabAdminSecret = appSettings["ida:ClientSecret"];
            Settings.LabAdminTenantId = appSettings["ida:TenantId"];
            Settings.Authority = "https://login.microsoftonline.com/{0}";
            Settings.AdminAuthority = string.Format(Settings.Authority, Settings.LabAdminTenantId);

            Settings.LabUserClientId = appSettings["LinkAdminClientId"];
            Settings.LabUserSecret = appSettings["LinkAdminSecret"];
            Settings.UserAuthority = string.Format(Settings.Authority, "common");

            Settings.StorageConnectionString = appSettings["StorageConnectionString"];
            Settings.LabQueueName = appSettings["LabQueueName"];

            Settings.GraphResource = "https://graph.microsoft.com";

            var client = await DocDBRepo.Initialize();

            //DNS config
            Settings.DomainGroups = await DocDBRepo.DB<DomainResourceGroup>.GetItemsAsync();

            using (var dns = new DnsAdmin())
            {
                foreach (var group in Settings.DomainGroups)
                {
                    group.DomainList = new List<string>();
                    await dns.InitAsync(group);
                    var zones = await dns.GetZoneList();
                    foreach (var zone in zones)
                    {
                        group.DomainList.Add(zone.Name);
                    }
                }
            }

        }
    }
}
