using System;
using System.Configuration;
using System.Diagnostics;
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

            try
            {
                Settings.Init(ConfigurationManager.AppSettings, dir);
                ConfigureAuth(app);
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error during initialization", EventLogEntryType.Error, ex);
            }
        }
    }
}
