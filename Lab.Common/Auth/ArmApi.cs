using Graph;
using Graph.Models;
using Lab.Common;
using Lab.Common.Graph.Models;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;

namespace Infra.Auth
{
    public class ArmApi
    {
        private bool _isInit;
        //private string _token;
        private ServiceClientCredentials _serviceCreds;

        public ArmApi(string oid, HttpControllerContext ctx)
        {
            var task = Task.Run(async () => {
                //_token was used for delegated user auth
                //_token = await AdalLib.GetAccessToken(oid, ctx, "https://management.azure.com");
                // Build the service credentials and DNS management client
                _serviceCreds = await ApplicationTokenProvider.LoginSilentAsync(Settings.LabAdminTenantId, Settings.LabAdminClientId, Settings.LabAdminSecret);
                _isInit = true;
            });
            task.Wait();
        }

        private void CheckInit()
        {
            if (!_isInit)
            {
                throw new Exception("The DNSAdmin utility has to be initialized asynchronously with a DomainResourceGroup object.");
            }
        }

        private dynamic GetResult(AdalResponse response)
        {
            if (!response.Successful)
            {
                return new { };
            }
            return (dynamic)JsonConvert.DeserializeObject(response.ResponseContent);
        }

        public async Task<dynamic> GetSubscriptions()
        {
            var url = "https://management.azure.com/subscriptions?api-version=2016-06-01";
            var res = await CallApi(url, HttpMethod.Get);
            return GetResult(res);
        }

        public async Task<dynamic> GetResourceGroups(string subscriptionId)
        {
            var url = string.Format("https://management.azure.com/subscriptions/{0}/resourcegroups?api-version=2018-05-01", subscriptionId);
            var res = await CallApi(url, HttpMethod.Get);
            return GetResult(res);
        }

        public async Task<IEnumerable<DnsZone>> GetDNSZones(string subscriptionId, string resourceGroupName)
        {
            var url = string.Format("https://management.azure.com/subscriptions/{0}/resourcegroups/{1}/providers/Microsoft.Network/dnsZones?api-version=2018-03-01-preview", subscriptionId, resourceGroupName);
            var res = await CallApi(url, HttpMethod.Get);
            if (!res.Successful)
            {
                var err = JsonConvert.DeserializeObject<DomainError>(res.ResponseContent);
                throw new Exception(string.Format("{0}: {1}", err.Error.Code, err.Error.Message));
            }
            var zones = JsonConvert.DeserializeObject<DnsZones>(res.ResponseContent);
            DnsZones newZones = null;
            while (zones.NextLink != null)
            {
                res = await CallApi(zones.NextLink, HttpMethod.Get);
                newZones = JsonConvert.DeserializeObject<DnsZones>(res.ResponseContent);
                zones.Value.AddRange(newZones.Value);
            }
            return zones.Value;
        }

        private async Task<AdalResponse> CallApi(string url, HttpMethod verb = null, string body = null)
        {
            CheckInit();
            HttpResponseMessage response = null;
            var res = new AdalResponse
            {
                Successful = true
            };

            try
            {
                verb = verb ?? HttpMethod.Get;
                var msg = new HttpRequestMessage(verb, url);
                //msg.Headers.Add("Authorization", "bearer " + _token);
                CancellationToken _cancel = new CancellationToken();
                await _serviceCreds.ProcessHttpRequestAsync(msg, _cancel);
                if (body != null)
                {
                    msg.Content = new StringContent(body, Encoding.UTF8, "application/json");
                }

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(300);
                    client.DefaultRequestHeaders.Add("client-request-id", Guid.NewGuid().ToString());
                    response = await client.SendAsync(msg);

                    res.ResponseContent = await response.Content.ReadAsStringAsync();
                    res.StatusCode = response.StatusCode;
                    res.Message = response.ReasonPhrase;
                    if (!response.IsSuccessStatusCode)
                    {
                        res.Successful = false;
                        dynamic serverError = JsonConvert.DeserializeObject(res.ResponseContent);
                        var reason = (response == null ? "N/A" : response.ReasonPhrase);
                        var serverErrorMessage = (serverError.Error == null) ? "N/A" : serverError.Error.Message;
                        res.Message = string.Format("(Server response: {0}. Server detail: {1})", reason, serverErrorMessage);
                        return res;
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                res.Successful = false;
                res.Message = ex.Message;
                return res;
            }
        }
    }
}
