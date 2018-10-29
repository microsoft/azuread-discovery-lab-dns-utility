using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lab.Data.Models
{
    public class DnsDTO
    {
        public string TxtRecord { get; set; }
        public string DomainName { get; set; }
        public string LabId { get; set; }

        public static DnsDTO FromTeamDTO(TeamDTO data)
        {
            return new DnsDTO
            {
                DomainName = data.TeamAssignment.DomainName,
                LabId = data.Lab.Id,
                TxtRecord = data.TeamAssignment.DnsTxtRecord
            };
        }
    }
}