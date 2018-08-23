using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureADLabDNSControl.Models
{
    public class LabDTO
    {
        public LabSettings Lab { get; set; }
        public IEnumerable<DomAssignment> Assignments { get; set; }
    }
}