using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab.Data.Models
{
    public partial class LabSettingsFull : LabSettings
    {
        [JsonProperty(PropertyName = "domAssignments")]
        public IEnumerable<DomAssignment> DomAssignments { get; set; }
    }
}
