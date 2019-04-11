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
using Infra.Auth;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Graph
{
    public static class AdalLib
    {
        public static string GraphApiVersion = "v1.0";
        const string SERVICE_UNAVAILABLE = "temporarily_unavailable";
        const string INTERACTION_REQUIRED = "interaction_required";

        public static async Task<string> GetAccessToken(ClaimsIdentity principal, HttpContextBase hctx, string resource)
        {
            var oid = principal.FindFirst(TokenCacheClaimTypes.ObjectId)
                        .Value;
            var fqdn = Utils.GetFQDN(hctx.Request);

            return await GetAccessToken(oid, resource, fqdn);
        }

        public static async Task<string> GetAccessToken(string oid, HttpControllerContext hctx, string resource)
        {
            var fqdn = Utils.GetFQDN(hctx.RequestContext);
            
            return await GetAccessToken(oid, resource, fqdn);
        }

        public static async Task<string> GetAccessToken(string oid, string resource, string fqdn)
        {
            AuthenticationContext authContext = null;
            AuthenticationResult result = null;
            ClientCredential credential = null;
            string nameId = "";

            try
            {

                // Get the access token from the cache
                nameId = oid;

                authContext = new AuthenticationContext(Settings.AdminAuthority,
                    new AdalCosmosTokenCache(nameId, fqdn));
                if (authContext.TokenCache.Count == 0)
                {
                    return "";
                }

                credential = new ClientCredential(Settings.LabAdminClientId, Settings.LabAdminSecret);
                result = await authContext.AcquireTokenSilentAsync(resource, credential, new UserIdentifier(nameId, UserIdentifierType.UniqueId));

                return result.AccessToken;
            }
            catch (AdalSilentTokenAcquisitionException)
            {
                return null;
            }
            catch (AdalServiceException ex)
            {
                if (ex.ErrorCode == "invalid_grant")
                {
                    return ex.Message;
                }

                if (ex.ErrorCode == INTERACTION_REQUIRED)
                {
                    HttpResponseMessage myMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, ReasonPhrase = INTERACTION_REQUIRED, Content = new StringContent(ex.Message) };
                    throw new HttpResponseException(myMessage);
                }
            }
            catch (AdalException ex)
            {
                if (ex.InnerException.GetType() == typeof(AdalClaimChallengeException))
                {
                    return null;
                }
                throw ex;
            }
            catch (Exception ex)
            {
                await Logging.WriteDebugInfoToErrorLog("Error retrieving user access token", ex);
                return null;
            }
            return null;
        }

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

        public static async Task<AdalResponse> GetResourceAsync(string request, string accessToken, HttpMethod verb = null, string body = null)
        {
            var res = new AdalResponse
            {
                Successful = true
            };

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    verb = verb ?? HttpMethod.Get;
                    HttpRequestMessage req = new HttpRequestMessage(verb, request);
                    HttpResponseMessage response = null;
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
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

        public static async Task<AdalResponse> GetResourceAsync(string request, string tenantId, HttpContextBase hctx, HttpMethod verb = null, string body=null)
        {
            string token = await GetAccessToken(hctx, tenantId);
            if (token == "")
            {
                return new AdalResponse
                {
                    Successful = false,
                    Message = "Reauthenticate"
                };
            }
            return await GetResourceAsync(request, token, verb, body);
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
                res = info.Error ?? new Uri(info.Issuer).Segments[1].TrimEnd('/');
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
            if (identity.HasClaim(TokenCacheClaimTypes.TenantId))
            {
                return identity.GetClaim(TokenCacheClaimTypes.TenantId);
            }

            var domain = GetUserUPNSuffix(identity);
            return GetAADTenantId(domain);
        }

        public static string GetUserUPNSuffix(IIdentity identity)
        {
            var str = identity.GetClaim(ClaimTypes.Upn);
            if (str == null)
                str = identity.GetClaim(ClaimTypes.Email);
            var domain = str.Split('@')[1];
            return domain;
        }
    }
}