using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AzureADLabDNSControl.Models;
using DocDBLib;
using Newtonsoft.Json;

namespace Infra
{
    public class LabRepo
    {

        #region Gets
        public static async Task<LabSettings> GetLab(string LabId)
        {
            var res = await DocDBRepo.DB<LabSettings>.GetItemAsync(LabId);
            return res;
        }

        public static async Task<bool> CheckLabDate(DateTime labDate)
        {
            var res = await DocDBRepo.DB<LabSettings>.GetItemsAsync(d => d.LabDate == labDate);
            return (res.Count() == 0);
        }

        public static async Task<IEnumerable<LabSettings>> GetTodaysLab(int offset)
        {
            var today = DateTime.UtcNow;
            today = today.AddMinutes(offset * -1).Date;
            var res = await DocDBRepo.DB<LabSettings>.GetItemsAsync(d => d.LabDate == today);
            return res;
        }

        public static async Task<IEnumerable<LabSettings>> GetLabs(string instructor)
        {
            var res = await DocDBRepo.DB<LabSettings>.GetItemsAsync(d => d.PrimaryInstructor == instructor || d.Instructors.Contains(instructor));
            return res.OrderBy(l => l.LabDate).ToList();
        }
        public static async Task<IEnumerable<DateTime>> GetLabDates()
        {
            var res = (await DocDBRepo.DB<LabSettings>.GetItemsAsync()).OrderBy(l => l.LabDate).Select(l => l.LabDate).ToList();
            return res;
        }

        public static async Task<TeamDTO> GetDomAssignment(DnsDTO item)
        {
            var res = (await DocDBRepo.DB<LabSettings>.GetItemsAsync(d => d.Id == item.LabId)).SingleOrDefault();
            if (res == null)
                return null;

            return new TeamDTO
            {
                Lab = res,
                TeamAssignment = res.DomAssignments.SingleOrDefault(d => d.DomainName == item.DomainName)
            };
        }

        public static async Task<TeamDTO> GetDomAssignment(string labCode, string teamAuth)
        {
            var res = (await DocDBRepo.DB<LabSettings>.GetItemsAsync(d => d.LabCode == labCode)).SingleOrDefault();
            if (res == null)
                return null;

            return new TeamDTO
            {
                Lab = res,
                TeamAssignment = res.DomAssignments.SingleOrDefault(d => d.TeamAuth == teamAuth)
            };
        }

        public static async Task<IEnumerable<DomAssignment>> GetDomAssignments(string labCode)
        {
            var res = (await DocDBRepo.DB<LabSettings>.GetItemsAsync(d => d.LabCode == labCode)).SingleOrDefault();
            if (res == null)
                return null;
            return res.DomAssignments;
        }

        public static async Task<IEnumerable<string>> GetInstructors(string labId)
        {
            var res = (await DocDBRepo.DB<LabSettings>.GetItemsAsync(d => d.Id == labId)).SingleOrDefault();
            if (res == null)
                return null;
            return res.Instructors;
        }
        #endregion

        #region Updates

        public static async Task<IEnumerable<LabSettings>> UpdateLab(LabSettings lab)
        {
            await DocDBRepo.DB<LabSettings>.UpdateItemAsync(lab);
            return await GetLabs(lab.PrimaryInstructor);
        }

        public static async Task<IEnumerable<LabSettings>> ResetLabCode(LabSettings lab)
        {
            lab.LabCode = LabSettings.GenLabCode();
            return await UpdateLab(lab);
        }
        public static async Task<LabSettings> ResetAssignment(TeamDTO team)
        {
            var newTeam = team.Lab.DomAssignments.Single(d => d.DomainName == team.TeamAssignment.DomainName);
            newTeam.TeamAuth = DomAssignment.GenAuthCode();
            newTeam.DnsTxtRecord = null;
            var res =  await UpdateLab(team.Lab);
            return res.Single(l => l.Id == team.Lab.Id);
        }

        public static async Task UpdateTenantId(TeamDTO data, string tenantId)
        {
            await DocDBRepo.DB<LabSettings>.UpdateTeamParms(data.Lab.Id, data.TeamAssignment.TeamAuth, "assignedTenantId", tenantId);
        }

        public static async Task UpdateDnsRecord(TeamDTO data)
        {
            var lab = await DocDBRepo.DB<LabSettings>.UpdateTeamParms(data.Lab.Id, data.TeamAssignment.TeamAuth, "dnsTxtRecord", data.TeamAssignment.DnsTxtRecord);
        }

        #endregion

        #region Inserts
        public static async Task<IEnumerable<LabSettings>> AddNewLab(LabSettings lab, string[] domains)
        {
            try
            {
                lab.LabCode = LabSettings.GenLabCode();
                lab.CreateDate = DateTime.UtcNow;
                
                string auth = null;
                foreach (string dom in domains)
                {
                    auth = DomAssignment.GenAuthCode();
                    lab.DomAssignments.Add(new DomAssignment
                    {
                        DomainName = dom,
                        TeamAuth = auth
                    });
                }

                LabSettings newLab = await SetLabSettingsAsync(lab);


                return await GetLabs(lab.PrimaryInstructor);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<LabSettings> SetLabSettingsAsync(LabSettings item)
        {
            try
            {
                var lab = await DocDBRepo.DB<LabSettings>.CreateItemAsync(item);
                return lab;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Deletes
        public static async Task<IEnumerable<LabSettings>> RemoveLab(string labId, string instructor)
        {
            var lab = await GetLab(labId);
            await DocDBRepo.DB<LabSettings>.DeleteItemAsync(lab);
            return await GetLabs(instructor);
        }
        #endregion
    }
}