using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab.Common;
using Lab.Common.Repo;
using Lab.Data.Helper;
using Lab.Data.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace LabManageJob
{
    public class Functions
    {
        /// <summary>
        /// Pickup Lab document ID from queue
        /// </summary>
        /// <param name="message"></param>
        /// <param name="log"></param>
        public static async Task ProcessQueueMessage([QueueTrigger("%logqueue%")] string message, TextWriter log)
        {
            var data = JsonConvert.DeserializeObject<LabQueueDTO>(message);
            var lab = await LabRepo.GetLab(data.LabId);
            if (lab.State == LabState.Error)
                return;

            try
            {
                switch (lab.State)
                {
                    case LabState.QueuedToDelete:
                        lab.State = LabState.Deleting;
                        await LabRepo.UpdateLab(lab, data.UserName);
                        await LabRepo.RemoveLabAssignments(lab);

                        //all zones and teams deleted, remove lab
                        await LabRepo.DeleteLab(lab);
                        break;
                    case LabState.Queued:
                        lab.State = LabState.Creating;
                        await LabRepo.UpdateLab(lab, data.UserName);
                        await LabRepo.AddLabAssignments(lab);
                        break;
                    case LabState.QueuedToUpdate:
                        lab.State = LabState.Updating;
                        await LabRepo.UpdateLab(lab, data.UserName);
                        await LabRepo.UpdateLabAssignments(lab);
                        break;
                    default:
                        return;
                }
                await Logging.WriteMessageToErrorLog(string.Format("{4} called \"{5}\" for {0} zones associated with lab \"{1} ({2})\", using RG \"{3}\"", lab.AttendeeCount, lab.City, lab.LabDate.ToShortDateString(), lab.DnsZoneRG, data.UserName, lab.State.ToString()));
                log.WriteLine(message);
            }
            catch (Exception ex)
            {
                await Logging.WriteDebugInfoToErrorLog("Error processing queue", ex);
                log.WriteLine(Logging.GetExceptionMessageString(ex));
                lab.State = LabState.Error;
                await LabRepo.UpdateLab(lab, data.UserName);
                throw ex;
            }
        }
    }
}
