using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab.Common;
using Lab.Common.Repo;
using Lab.Data.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace LabManageJob
{
    public class Functions
    {
        /// <summary>
        /// Pickup Lab document ID from queue, along with operation
        /// Operations:
        ///     RemoveLab
        ///     AddLab
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="log"></param>
        public static async Task ProcessQueueMessage([QueueTrigger("%logqueue%")] string message, TextWriter log)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<LabQueueDTO>(message);
                var lab = await LabRepo.GetLab(data.LabId);
                switch (data.Operation)
                {
                    case "RemoveLab":
                        await LabRepo.RemoveLabAssignments(lab);
                        break;
                    case "AddLab":
                        await LabRepo.AddLabAssignments(lab);
                        break;
                    default:
                        return;
                }
                await Logging.WriteMessageToErrorLog(string.Format("{4} called \"{5}\" for {0} zones associated with lab \"{1} ({2})\", using RG \"{3}\"", lab.AttendeeCount, lab.City, lab.LabDate.ToShortDateString(), lab.DnsZoneRG, data.UserName, data.Operation));
                log.WriteLine(message);
            }
            catch (Exception ex)
            {
                await Logging.WriteDebugInfoToErrorLog("Error processing queue", ex);
                log.WriteLine(Logging.GetExceptionMessageString(ex));
                throw ex;
            }
        }
    }
}
