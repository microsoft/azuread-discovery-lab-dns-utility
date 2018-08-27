using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocDBLib
{ 
    public interface IDocModelBase
    {
        [JsonProperty(PropertyName = "id")]
        string Id { get; set; }

        [JsonProperty(PropertyName = "docType")]
        string DocType { get; set; }
    }
}
