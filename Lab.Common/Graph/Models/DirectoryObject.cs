using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Graph.Models
{
    public class DirectoryObjects
    {
        [JsonProperty(PropertyName = "@odata.context")]
        public string Context { get; set; }

        [JsonProperty(PropertyName = "value")]
        public List<DirectoryObject> Value { get; set; }

        public DirectoryObjects()
        {
            Value = new List<DirectoryObject>();
        }

        public class DirectoryObject
        {
            [JsonProperty(PropertyName = "@odata.type")]
            public string Type { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id;

            [JsonProperty(PropertyName = "userPrincipalName")]
            public string UserPrincipalName { get; set; }
        }
    }
}