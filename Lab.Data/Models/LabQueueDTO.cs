using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab.Data.Models
{
    public class LabQueueDTO
    {
        [JsonProperty("labId")]
        public string LabId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }
    }
}
