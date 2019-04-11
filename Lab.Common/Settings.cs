using DocDBLib;
using Infra.Auth;
using Lab.Data.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Lab.Common
{
    public static class Settings
    {
        public static string AppRootPath { get; set; }
        //public static IEnumerable<DomainResourceGroup> DomainGroups { get; set; }

        public static string StorageConnectionString { get; set; }
        public static string LabQueueName { get; set; }
        public static string LabAdminClientId { get; set; }
        public static string LabAdminSecret { get; set; }
        public static string LabAdminTenantId { get; set; }
        public static string Authority { get; set; }
        public static string AdminAuthority { get; set; }

        public static string LabUserClientId { get; set; }
        public static string LabUserSecret { get; set; }
        public static string UserAuthority { get; set; }

        public static string GraphResource { get; set; }

        public static void Init(NameValueCollection appSettings, string appRoot)
        {
            AppRootPath = appRoot;

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
            LabAdminClientId = appSettings["ida:ClientId"];
            LabAdminSecret = appSettings["ida:ClientSecret"];
            LabAdminTenantId = appSettings["ida:TenantId"];
            Authority = "https://login.microsoftonline.com/{0}";
            AdminAuthority = string.Format(Authority, LabAdminTenantId);

            LabUserClientId = appSettings["LinkAdminClientId"];
            LabUserSecret = appSettings["LinkAdminSecret"];
            UserAuthority = string.Format(Authority, "common");
            AESEncryption.Password = LabAdminSecret;

            StorageConnectionString = appSettings["StorageConnectionString"];
            LabQueueName = appSettings["LabQueueName"];

            GraphResource = "https://graph.microsoft.com";

            var client = DocDBRepo.Initialize().Result;
        }
    }
}