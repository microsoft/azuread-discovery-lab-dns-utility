using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace DocDBLib
{
    public class DocModelBase : Resource
    {
        [JsonProperty(PropertyName = "docType")]
        public string DocType { get; set; }
    }
}