using DocDBLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Lab.Data.Models
{
    public class DomAssignment : DocModelBase, IDocModelBase
    {
        [JsonProperty(PropertyName = "labSettingsId")]
        public string LabSettingsId { get; set; }

        [JsonProperty(PropertyName = "labCode")]
        public string LabCode { get; set; }

        [Display(Name = "DNS TXT Record")]
        [JsonProperty(PropertyName = "dnsTxtRecord")]
        public string DnsTxtRecord { get; set; }

        [Display(Name = "Domain Name")]
        [JsonProperty(PropertyName = "domainName")]
        public string DomainName { get; set; }

        [Display(Name = "Parent Zone")]
        [JsonProperty(PropertyName = "parentZone")]
        public string ParentZone { get; set; }

        [Display(Name = "Team Name")]
        [JsonProperty(PropertyName = "teamName")]
        public string TeamName { get; set; }

        [JsonProperty(PropertyName = "teamAuth")]
        public string TeamAuth { get; set; }

        [JsonProperty(PropertyName = "assignedTenantId")]
        public string AssignedTenantId { get; set; }

        public static string GenAuthCode(string teamName)
        {
            return string.Format("{1}-{0}", teamName, Utils.CreatePassword(5));
        }
    }
}