using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Lab.Data.Models;
using DocDBLib;
using Newtonsoft.Json;
using Lab.Data.Helper;

namespace Lab.Common.Repo
{
    public static class LabRepo
    {
        public static async Task<IEnumerable<DomainGroupDTO>> GetLabStats()
        {
            var list = new List<DomainGroupDTO>();

            foreach (var group in Settings.DomainGroups)
            {
                list.Add(new DomainGroupDTO
                {
                    DnsZoneRG = group.DnsZoneRG,
                    AzureSubscriptionId = group.AzureSubscriptionId
                });
            }

            var labs = await DocDBRepo.DB<LabSettings>.GetItemsAsync();
            foreach(var lab in labs)
            {
                list.Single(d => d.DnsZoneRG == lab.DnsZoneRG).ZoneCount += lab.AttendeeCount;
            }
            return list;
        }

        public static async Task<IEnumerable<LabSettings>> AddNewLab(LabSettings lab, string user)
        {
            try
            {
                lab.LabCode = LabSettings.GenLabCode();
                lab.CreateDate = DateTime.UtcNow;
                lab.State = LabState.Queued;

                //setting this in AdminController:Index
                var arr = lab.DnsZoneRG.Split(':');
                lab.AzureSubscriptionId = arr[0];
                lab.DnsZoneRG = arr[1];
                var group = Settings.DomainGroups.Single(d => d.DnsZoneRG == lab.DnsZoneRG);
                lab.AzureSubscriptionId = group.AzureSubscriptionId;
                LabSettings newLab = await SetLabSettingsAsync(lab);

                return await GetLabs(user);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task AddLabAssignments(LabSettings lab)
        {
            //lab.AttendeeCount
            var domGroup = Settings.DomainGroups.Single(d => d.AzureSubscriptionId == lab.AzureSubscriptionId && d.DnsZoneRG == lab.DnsZoneRG);
            var domains = domGroup.DomainList;
            //var city = lab.City.ToLower().Replace(" ", "").Replace(".", "").Replace("-", "");
            //city += (lab.LabDate.Month.ToString() + lab.LabDate.Day.ToString());
            string auth = null;

            using (var dns = new DnsAdmin())
            {
                await dns.InitAsync();
                dns.SetClient(domGroup);
                var itemsPerDomain = (lab.AttendeeCount / domains.Count()) + 1;
                var counter = 0;
                foreach (var dom in domains)
                {
                    if (counter == lab.AttendeeCount)
                        continue;

                    for (var x = 0; x < itemsPerDomain; x++)
                    {
                        if (counter == lab.AttendeeCount)
                            continue;

                        //create [itemsPerDomain] teams/child domains per parent domain name
                        var team = string.Format("{0}{1}", lab.LabName, (counter + 1));
                        auth = DomAssignment.GenAuthCode(team);
                        var newTeamItem = new DomAssignment
                        {
                            ParentZone = dom,
                            TeamName = team,
                            DomainName = string.Format("{0}.{1}", team, dom),
                            TeamAuth = auth,
                            LabCode = lab.LabCode,
                            LabSettingsId = lab.Id
                        };
                        await DocDBRepo.DB<DomAssignment>.CreateItemAsync(newTeamItem);

                        await dns.CreateNewChildZone(newTeamItem.ParentZone, newTeamItem.TeamName, newTeamItem.DomainName);
                        counter++;
                    }
                }
            }

            //update lab
            lab.State = LabState.Ready;
            await DocDBRepo.DB<LabSettings>.UpdateItemAsync(lab);
        }

        public static async Task<TeamDTO> GetDomAssignment(string labCode, string teamCode)
        {
            var lab = (await DocDBRepo.DB<LabSettings>.GetItemsAsync(l => l.LabCode == labCode)).SingleOrDefault();
            var team = (await DocDBRepo.DB<DomAssignment>.GetItemsAsync(d => d.TeamAuth == teamCode && d.LabCode == labCode)).SingleOrDefault();
            return new TeamDTO
            {
                Lab = lab,
                TeamAssignment = team
            };
        }

        /// <summary>
        /// called from the web job, removes assignments then deletes the lab
        /// </summary>
        /// <returns></returns>
        public static async Task RemoveLabAssignments(LabSettings lab)
        {
            var assignments = await GetDomAssignments(lab.Id);

            try
            {
                var domGroup = Settings.DomainGroups.Single(d => d.AzureSubscriptionId == lab.AzureSubscriptionId && d.DnsZoneRG == lab.DnsZoneRG);
                
                using (var dns = new DnsAdmin())
                {
                    await dns.InitAsync();
                    dns.SetClient(domGroup);
                    foreach (var item in assignments)
                    {
                        await dns.RemoveChildZone(item.ParentZone, item.TeamName, item.DomainName);
                        await DocDBRepo.DB<DomAssignment>.DeleteItemAsync(item);
                    }
                }

                //all zones and teams deleted, remove lab
                await DocDBRepo.DB<LabSettings>.DeleteItemAsync(lab);
            }
            catch (Exception ex)
            {
                await Logging.WriteDebugInfoToErrorLog(string.Format("Unable to remove domain assignments for labid {0}", lab.Id), ex);
                throw ex;
            }
        }

        /// <summary>
        /// sets lab state to deleting
        /// </summary>
        /// <param name="labId"></param>
        /// <param name="instructor"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<LabSettings>> RemoveLab(string labId, string instructor)
        {
            try
            {
                var lab = await GetLab(labId);
                lab.State = LabState.QueuedToDelete;
                await DocDBRepo.DB<LabSettings>.UpdateItemAsync(lab);

                return await GetLabs(instructor);

            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Gets
        public static async Task<LabSettingsFull> GetLabAndSettings(string labId)
        {
            var res = await DocDBRepo.DB<LabSettingsFull>.GetItemAsync(labId);
            if (res == null)
            {
                //lab has been deleted
                return null;
            }
            res.DomAssignments = await DocDBRepo.DB<DomAssignment>.GetItemsAsync(d => d.LabSettingsId == labId);
            return res;
        }

        public static async Task<LabSettings> GetLab(string labId)
        {
            var res = await DocDBRepo.DB<LabSettings>.GetItemAsync(labId);
            return res;
        }

        public static async Task<bool> CheckLabDate(DateTime labDate)
        {
            var res = await DocDBRepo.DB<LabSettings>.GetItemsAsync(d => d.LabDate == labDate);
            return (res.Count() == 0);
        }

        public static async Task<IEnumerable<LabSettings>> GetTodaysLab(int offset)
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
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

        public static async Task<IEnumerable<string>> GetInstructors(string labId)
        {
            var res = (await DocDBRepo.DB<LabSettings>.GetItemsAsync(d => d.Id == labId)).SingleOrDefault();
            if (res == null)
                return null;
            return res.Instructors;
        }
        #endregion

        #region Updates
        public static async Task<IEnumerable<LabSettings>> UpdateLab(LabSettings lab, string instructor)
        {
            await DocDBRepo.DB<LabSettings>.UpdateItemAsync(lab);
            return await GetLabs(instructor);
        }

        public static async Task<IEnumerable<LabSettings>> ResetLabCode(LabSettings lab, string instructor)
        {
            lab.LabCode = LabSettings.GenLabCode();
            return await UpdateLab(lab, instructor);
        }

        //Team Operations
        public static async Task<IEnumerable<DomAssignment>> GetDomAssignments(string labId)
        {
            var res = await DocDBRepo.DB<DomAssignment>.GetItemsAsync(d => d.LabSettingsId == labId);
            return res;
        }

        public static async Task<DomAssignment> UpdateTeamAssignment(DomAssignment teamAssignment)
        {
            var res = await DocDBRepo.DB<DomAssignment>.UpdateItemAsync(teamAssignment);
            return res;
        }

        public static async Task<TeamDTO> ResetTeamCode(TeamDTO data)
        {
            var newCode = DomAssignment.GenAuthCode(data.TeamAssignment.TeamName);
            var res = await GetDomAssignment(data.Lab.LabCode, data.TeamAssignment.TeamAuth);
            res.TeamAssignment.TeamAuth = newCode;
            res.TeamAssignment = await UpdateTeamAssignment(res.TeamAssignment);
            return res;
        }

        public static async Task<TeamDTO> UpdateTenantId(TeamDTO data, string tenantId, string tenantName, string tenantAdmin)
        {
            var res = await GetDomAssignment(data.Lab.LabCode, data.TeamAssignment.TeamAuth);
            res.TeamAssignment.AssignedTenantId = tenantId;
            res.TeamAssignment.AssignedTenantName = tenantName;
            res.TeamAssignment.TenantAdminUpn = tenantAdmin;
            res.TeamAssignment = await UpdateTeamAssignment(res.TeamAssignment);
            return res;
        }

        public static async Task<TeamDTO> UpdateDnsRecord(TeamDTO data)
        {
            var res = await GetDomAssignment(data.Lab.LabCode, data.TeamAssignment.TeamAuth);
            res.TeamAssignment.DnsTxtRecord = data.TeamAssignment.DnsTxtRecord;
            res.TeamAssignment = await UpdateTeamAssignment(res.TeamAssignment);
            return res;
        }

        #endregion

        #region Inserts
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
    }
}