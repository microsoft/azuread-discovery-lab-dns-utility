using Infra;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Display(Name = "Create Date")]
        public DateTime CreateDate { get; set; }

        [JsonProperty("dnsTxtRecord")]
        [Display(Name = "DNS TXT Record")]
        public string DnsTxtRecord { get; set; }

        [JsonProperty("domainName")]
        [Display(Name = "Domain Name")]
        public string DomainName { get; set; }
    }
}