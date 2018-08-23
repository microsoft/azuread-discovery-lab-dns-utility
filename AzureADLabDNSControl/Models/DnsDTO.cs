using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureADLabDNSControl.Models
{
    public class DnsDTO
    {
        public string TxtRecord { get; set; }
        public string DomainName { get; set; }
    }
}