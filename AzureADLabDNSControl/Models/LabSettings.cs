using Infra;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureADLabDNSControl.Models
{
    public class LabSettings : TableEntity
    {
        public LabSettings()
        {

        }
        public LabSettings(string instructorUpn, DateTime labDate, string city)
        {
            PartitionKey = instructorUpn;
            RowKey = JsonConvert.SerializeObject(labDate);
            LabDate = labDate;
            LabCode = Util.CreatePassword(8);
            City = city;
        }

        [JsonProperty("labDate")]
        public DateTime LabDate { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("labCode")]
        public string LabCode { get; set; }
    }
}