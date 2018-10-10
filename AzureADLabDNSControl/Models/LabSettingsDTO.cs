using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureADLabDNSControl.Models
{
    public class LabSettingsDTO
    {
        public LabSettings Settings { get; set; }
        public string ResponseMessage { get; set; }
        public dynamic Object { get; set; }
    }
}