using System;
using System.Configuration;
using System.Threading.Tasks;
using Infra;
using Microsoft.Owin;
using Owin;

namespace AzureADLabDNSControl
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            var task = Task.Run(async () => {
                await Settings.Init(ConfigurationManager.AppSettings);
            });
            task.Wait();
        }
    }
}
