using System;
using System.Configuration;
using System.Threading.Tasks;
using Lab.Common;
using Microsoft.Owin;
using Owin;

namespace AzureADLabDNSControl
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var dir = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;

            var task = Task.Run(async () => {
                await Settings.Init(ConfigurationManager.AppSettings, dir);
                ConfigureAuth(app);
            });
            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error during initialization", System.Diagnostics.EventLogEntryType.Error, ex);
            }
        }
    }
}
