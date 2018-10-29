using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Linq;
using System.Net;
using System.IO;
using System.Security.Principal;
using Lab.Common;

namespace Graph
{
    public static class AdalLib
    {
        public static string GraphApiVersion = "v1.0";
        const String SERVICE_UNAVAILABLE = "temporarily_unavailable";
        const String INTERACTION_REQUIRED = "interaction_required";

        public static async Task<string> GetAccessToken(HttpContextBase hctx, string tenantId)
        {
            var credential = new ClientCredential(Settings.LabUserClientId, Settings.LabUserSecret);
            var authContext = new AuthenticationContext(string.Format(Settings.Authority, tenantId));
            var result = await authContext.AcquireTokenAsync(Settings.GraphResource, credential);

            return result.AccessToken;
        }

        public static async Task<AdalResponse<T>> GetResourceAsync<T>(string request, string tenantId, HttpContextBase hctx, HttpMethod verb = null, string body = null) where T : class
        {
            var data = await GetResourceAsync(request, tenantId, hctx, verb, body);
            return new AdalResponse<T>(data);
        }

        public static async Task<AdalResponse> GetResourceAsync(string request, string tenantId, HttpContextBase hctx, HttpMethod verb = null, string body=null)
        {
            var res = new AdalResponse
            {
                Successful = true
            };

            string token = await GetAccessToken(hctx, tenantId);
            if (token == "")
            {
                res.Successful = false;
                res.Message = "Reauthenticate";
                return res;
            }

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    verb = verb ?? HttpMethod.Get;
                    HttpRequestMessage req = new HttpRequestMessage(verb, request);
                    HttpResponseMessage response = null;
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    if (body != null)
                    {
                        req.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                    }
                    response = client.SendAsync(req).Result;
                    res.RawResponse = response;
                    res.ResponseContent = await response.Content.ReadAsStringAsync();
                    res.StatusCode = response.StatusCode;
                    res.Message = response.ReasonPhrase;
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        res.Successful = false;
                        var serverError = JsonConvert.DeserializeObject<GraphError>(res.ResponseContent);
                        var reason = (response == null ? "N/A" : response.ReasonPhrase);
                        var serverErrorMessage = (serverError.Error == null) ? "N/A" : serverError.Error.Message;
                        res.Message = string.Format("(Server response: {0}. Server detail: {1})", reason, serverErrorMessage);
                        return res;
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

        public static string GetAADTenantId(string domainName)
        {
            var uri = string.Format("https://login.windows.net/{0}/.well-known/openid-configuration", domainName);
            string res = "";
            using (var web = new WebClient())
            {
                try
                {
                    res = web.DownloadString(uri);
                }
                catch (WebException exception)
                {
                    using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                    {
                        res = reader.ReadToEnd();
                    }
                }
                var info = JsonConvert.DeserializeObject<OIDConfigResponse>(res);
                res = new Uri(info.Issuer).Segments[1].TrimEnd('/');
            }
            return res;
        }

        /// <summary>
        /// Look up AAD Tenant ID from UPN suffix
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string GetUserTenantId(IIdentity identity)
        {
            if (identity.HasClaim(CustomClaimTypes.TenantId))
            {
                return identity.GetClaim(CustomClaimTypes.TenantId);
            }

            var str = identity.GetClaim(ClaimTypes.Upn);
            if (str == null)
                str = identity.GetClaim(ClaimTypes.Email);
            var domain = str.Split('@')[1];
            return GetAADTenantId(domain);
        }
    }
}