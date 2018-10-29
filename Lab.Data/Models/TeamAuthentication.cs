using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Lab.Data.Models
{
    public class TeamAuthentication
    {
        [Display(Name = "Lab Code")]
        public string LabCode { get; set; }

        [Display(Name = "Team Auth Code")]
        public string TeamAuth { get; set; }
    }
}