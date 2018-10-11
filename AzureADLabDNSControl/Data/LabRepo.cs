using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public event EventHandler<LabActivityArgs> LabActivity;
        private int _totalActivities;
        private int _activitiesCompleted;
        private Stream _outputStream;

        public LabRepo(Stream stream)
        {
            _outputStream = stream;
        }

        protected virtual void UpdateActivity(LabActivityArgs e)
        {
            LabActivity?.Invoke(this, e);
        }

        private void Activity(string Message)
        {
            _activitiesCompleted++;

            UpdateActivity(new LabActivityArgs
            {
                ActivitiesCompleted = _activitiesCompleted,
                TotalActivities = _totalActivities,
                Update = Message,
                OutputStream = _outputStream
            });
        }

        public async Task<IEnumerable<LabSettings>> AddNewLab(LabSettings lab, string[] domains)
        {
            try
            {
                _activitiesCompleted = 0;
                lab.LabCode = LabSettings.GenLabCode();
                lab.CreateDate = DateTime.UtcNow;
                var city = lab.City.ToLower().Replace(" ", "");
                city += (lab.LabDate.Month.ToString() + lab.LabDate.Day.ToString());

                string auth = null;
                var counter = 1;
                foreach (string dom in domains)
                {
                    for (var x = 0; x < 4; x++)
                    {
                        //create 4 teams/child domains per parent domain name
                        var team = string.Format("{0}{1}", city, counter);
                        auth = DomAssignment.GenAuthCode(team);
                        lab.DomAssignments.Add(new DomAssignment
                        {
                            ParentZone = dom,
                            TeamName = team,
                            DomainName = string.Format("{0}.{1}", team, dom),
                            TeamAuth = auth
                        });
                        counter++;
                    }
                }
                _totalActivities = counter + 2;
                Activity("Saving Lab...");

                LabSettings newLab = await SetLabSettingsAsync(lab);
                Activity("Lab Saved, updating DNS...");

                using (var dns = new DnsAdmin())
                {
                    foreach (var dom in lab.DomAssignments)
                    {
                        await dns.CreateNewChildZone(dom.ParentZone, dom.TeamName, dom.DomainName);
                        Activity(string.Format("Saved {0}...", dom.DomainName));
                    }
                }

                return await GetLabs(lab.PrimaryInstructor);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<LabSettings>> RemoveLab(string labId, string instructor)
        {
            try
            {
                _activitiesCompleted = 0;

                var lab = await GetLab(labId);
                _totalActivities = lab.DomAssignments.Count() + 1;

                Activity("Deleting Lab...");
                await DocDBRepo.DB<LabSettings>.DeleteItemAsync(lab);

                using (var dns = new DnsAdmin())
                {
                    foreach (var team in lab.DomAssignments)
                    {
                        await dns.RemoveChildZone(team.ParentZone, team.TeamName, team.DomainName);
                        Activity(string.Format("Deleted {0}...", team.DomainName));
                    }
                }

                return await GetLabs(instructor);
            }
            catch (Exception)
            {
                throw;
            }
        }

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

        public static async Task ResetTeamCode(TeamDTO data)
        {
            var newCode = DomAssignment.GenAuthCode(data.TeamAssignment.TeamName);
            await DocDBRepo.DB<LabSettings>.UpdateTeamParms(data.Lab.Id, data.TeamAssignment.TeamAuth, "teamAuth", newCode);
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

    public class LabActivityArgs: EventArgs
    {
        public string Update { get; set; }
        public int TotalActivities { get; set; }
        public int ActivitiesCompleted { get; set; }
        public Stream OutputStream { get; set; }
    }
}