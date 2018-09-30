using Infra;
using DocDBLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AzureADLabDNSControl.Models
{
    public class DomAssignment
    {
        [Display(Name = "DNS TXT Record")]
        [JsonProperty(PropertyName = "dnsTxtRecord")]
        public string DnsTxtRecord { get; set; }

        [Display(Name = "Domain Name")]
        [JsonProperty(PropertyName = "domainName")]
        public string DomainName { get; set; }

        [JsonProperty(PropertyName = "teamAuth")]
        public string TeamAuth { get; set; }

        [JsonProperty(PropertyName = "assignedTenantId")]
        public string AssignedTenantId { get; set; }

        public static string GenAuthCode()
        {
            return string.Format("Team-{0}", Util.CreatePassword(5));
        }
    }
}