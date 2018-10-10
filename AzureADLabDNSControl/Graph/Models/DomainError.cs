using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Graph.Models
{
    /// <summary>
    /// Error response object from AADLinkControl REST calls. Sample:
    /// 
    ///   {
    ///      "error": {
    ///        "code": "Request_BadRequest",
    ///        "message": "Domain deletion attempt failed.",
    ///        "innerError": {
    ///          "request-id": "eb8c1cd6-589b-47c6-a5b4-1a0ccecff1b3",
    ///          "date": "2018-10-09T04:39:54"
    ///        },
    ///        "details": [
    ///          {
    ///            "target": "id",
    ///            "code": "ObjectInUse"
    ///          }
    ///        ]
    ///      }
    ///   }
    /// </summary>
    public class DomainError
    {
        [JsonProperty(PropertyName = "error")]
        public DomError Error { get; set; }

        /// <summary>
        /// 
        ///   "error": {
        ///     "code": "Request_BadRequest",
        ///     "message": "Domain deletion attempt failed.",
        ///     "innerError": {
        ///       "request-id": "eb8c1cd6-589b-47c6-a5b4-1a0ccecff1b3",
        ///       "date": "2018-10-09T04:39:54"
        ///     },
        ///     "details": [
        ///       {
        ///         "target": "id",
        ///         "code": "ObjectInUse"
        ///       }
        ///     ]
        ///   }
        /// </summary>
        public class DomError
        {
            [JsonProperty(PropertyName = "code")]
            public string Code { get; set; }

            [JsonProperty(PropertyName = "message")]
            public string Message { get; set; }

            [JsonProperty(PropertyName = "innerError")]
            public DomInnerError InnerError { get; set; }

            [JsonProperty(PropertyName = "details")]
            public List<DomDetails> Details { get; set; }

            public DomError()
            {
                Details = new List<DomDetails>();
            }

            /// <summary>
            /// 
            ///   "innerError": {
            ///     "request-id": "eb8c1cd6-589b-47c6-a5b4-1a0ccecff1b3",
            ///     "date": "2018-10-09T04:39:54"
            ///   },
            /// </summary>
            public class DomInnerError
            {
                [JsonProperty(PropertyName = "request-id")]
                public string RequestId { get; set; }

                [JsonProperty(PropertyName = "date")]
                public DateTime Date { get; set; }
            }

            /// <summary>
            /// 
            ///   "details": [
            ///     {
            ///       "target": "id",
            ///       "code": "ObjectInUse"
            ///     }
            ///   ]
            /// </summary>
            public class DomDetails
            {
                [JsonProperty(PropertyName = "target")]
                public string Target { get; set; }

                [JsonProperty(PropertyName = "code")]
                public string Code { get; set; }
            }
        }
    }
}
