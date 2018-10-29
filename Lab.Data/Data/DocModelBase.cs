using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace DocDBLib
{
    public class DocModelBase : Document
    {
        [JsonProperty(PropertyName = "docType")]
        public string DocType { get; set; }
    }
}