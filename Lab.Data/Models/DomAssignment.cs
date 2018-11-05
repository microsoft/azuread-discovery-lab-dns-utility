using DocDBLib;
using Newtonsoft.Json;

namespace Lab.Data.Models
{
    public class DomAssignment : DocModelBase, IDocModelBase
    {
        [JsonProperty(PropertyName = "labSettingsId")]
        public string LabSettingsId { get; set; }

        [JsonProperty(PropertyName = "labCode")]
        public string LabCode { get; set; }

        [JsonProperty(PropertyName = "dnsTxtRecord")]
        public string DnsTxtRecord { get; set; }

        [JsonProperty(PropertyName = "domainName")]
        public string DomainName { get; set; }

        [JsonProperty(PropertyName = "parentZone")]
        public string ParentZone { get; set; }

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