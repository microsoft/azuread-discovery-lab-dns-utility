using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureADLabDNSControl.Models
{
    public class LabDTO
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "labDate")]
        public DateTime LabDate { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "primaryInstructor")]
        public string PrimaryInstructor { get; set; }
    }

    public class TeamDTO
    {
        public LabSettings Lab { get; set; }
        public DomAssignment TeamAssignment { get; set; }
    }
}