using Infra;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureADLabDNSControl.Models
{
    public class DomAssignment: TableEntity
    {
        public DomAssignment()
        {

        }
        public DomAssignment(string labCode, string TeamAuth, string domainName)
        {
            PartitionKey = labCode;
            RowKey = TeamAuth;
            CreateDate = DateTime.UtcNow;
            DomainName = domainName;
        }

        [JsonProperty("createDate")]
        public DateTime CreateDate { get; set; }

        [JsonProperty("dnsTxtRecord")]
        public string DnsTxtRecord { get; set; }

        [JsonProperty("domainName")]
        public string DomainName { get; set; }
    }
}