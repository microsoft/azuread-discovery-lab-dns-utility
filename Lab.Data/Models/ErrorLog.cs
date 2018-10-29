using DocDBLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Lab.Data.Models
{
    public class ErrorLog : DocModelBase, IDocModelBase
    {
        [JsonProperty(PropertyName = "errorDate")]
        public DateTime ErrorDate { get; set; }

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "userAgent")]
        public string UserAgent { get; set; }

        [JsonProperty(PropertyName = "ipAddress")]
        public string IPAddress { get; set; }

        [JsonProperty(PropertyName = "userComment")]
        public string UserComment { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "validationErrors")]
        public string ValidationErrors { get; set; }

        [JsonProperty(PropertyName = "errorSource")]
        public string ErrorSource { get; set; }

        [JsonProperty(PropertyName = "stackTrace")]
        public string StackTrace { get; set; }

        [JsonProperty(PropertyName = "innerExceptionSource")]
        public string InnerExceptionSource { get; set; }

        [JsonProperty(PropertyName = "qsData")]
        public string QSData { get; set; }

        [JsonProperty(PropertyName = "postData")]
        public string PostData { get; set; }

        [JsonProperty(PropertyName = "referrer")]
        public string Referrer { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "innerExceptionMessage")]
        public string InnerExceptionMessage { get; set; }

        [JsonProperty(PropertyName = "additionalMessage")]
        public string AdditionalMessage { get; set; }
    }
}
