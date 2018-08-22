using AzureADLabDNSControl;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

namespace Infra
{
    public static class AdalLib
    {
        public static async Task<string> GetAccessToken(string graphResource=null)
        {
            AuthenticationResult authResult = null;
            AuthenticationContext authContext = null;
            try
            {
                string resource = (graphResource != null) ? graphResource : "https://graph.microsoft.com/";
                var clientCred = new ClientCredential(Startup.clientId, Startup.clientSecret);

                authContext = new AuthenticationContext(string.Format(Startup.authority, Startup.tenantId));
                authResult = await authContext.AcquireTokenAsync(resource, clientCred);

                return authResult.AccessToken;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}