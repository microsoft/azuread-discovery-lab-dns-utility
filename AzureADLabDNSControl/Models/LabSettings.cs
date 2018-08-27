using DocDBLib;
using Infra;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureADLabDNSControl.Models
{
    public class LabSettings : DocModelBase, IDocModelBase
    {

        [JsonProperty(PropertyName = "primaryInstructor")]
        public string PrimaryInstructor { get; set; }

        [JsonProperty(PropertyName = "labDate")]
        public DateTime LabDate { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "labCode")]
        public string LabCode { get; set; }

        [JsonProperty(PropertyName = "createDate")]
        public DateTime CreateDate { get; set; }


        [JsonProperty(PropertyName = "instructors")]
        public IEnumerable<string> Instructors { get; set; }

        [JsonProperty(PropertyName = "domAssignments")]
        public List<DomAssignment> DomAssignments { get; set; }

        public LabSettings()
        {
            DomAssignments = new List<DomAssignment>();
        }

        public static string GenLabCode()
        {
            return Util.CreatePassword(8);
        }
    }
}