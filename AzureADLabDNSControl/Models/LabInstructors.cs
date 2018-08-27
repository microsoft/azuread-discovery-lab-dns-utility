using DocDBLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureADLabDNSControl.Models
{
    /// <summary>
    /// PartitionKey - InstructorAlias
    /// RowKey - 
    /// </summary>
    public class LabInstructor : DocModelBase, IDocModelBase
    {
        public string Instructor { get; set; }
        public DateTime LabDate { get; set; }

        public LabInstructor(string AssistantInstructor, string PrimaryInstructor, DateTime LabDate)
        {
            PartitionKey = AssistantInstructor;
            RowKey = PrimaryInstructor;
            this.LabDate = LabDate;
        }
    }
}