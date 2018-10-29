using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab.Common;

namespace LabManageJob
{
    public class CustomNameResolver : INameResolver
    {
        public string Resolve(string name)
        {
            switch (name)
            {
                case "logqueue":
                    return Settings.LabQueueName;
                default:
                    return Settings.LabQueueName;
            }
        }
    }
}

/*
public static void WriteLog([QueueTrigger("%logqueue%")] string logMessage)
{
    Console.WriteLine(logMessage);
}
*/
