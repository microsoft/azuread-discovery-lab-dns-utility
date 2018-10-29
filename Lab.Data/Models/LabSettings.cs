using DocDBLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lab.Data.Helper;

namespace Lab.Data.Models
{
    public partial class LabSettings : DocModelBase, IDocModelBase
    {
        [JsonProperty(PropertyName = "state")]
        public LabState State { get; set; }

        [JsonProperty(PropertyName = "primaryInstructor")]
        public string PrimaryInstructor { get; set; }

        [JsonProperty(PropertyName = "labDate")]
        public DateTime LabDate { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "labCode")]
        public string LabCode { get; set; }

        [JsonProperty(PropertyName = "attendeeCount")]
        public int AttendeeCount { get; set; }

        [JsonProperty(PropertyName = "createDate")]
        public DateTime CreateDate { get; set; }

        [JsonProperty(PropertyName = "azureSubscriptionId")]
        public string AzureSubscriptionId { get; set; }

        [JsonProperty(PropertyName = "dnsZoneRg")]
        public string DnsZoneRG { get; set; }

        [JsonProperty(PropertyName = "instructors")]
        public IEnumerable<string> Instructors { get; set; }

        public static string GenLabCode()
        {
            return Utils.CreatePassword(8);
        }
    }
}