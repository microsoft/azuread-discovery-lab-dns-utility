using Lab.Common;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin;
using Microsoft.Owin.Security.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;

namespace Infra.Auth
{
    public static class Utils
    {
        public static string ClientId { get; set; }
        public static string ClientSecret { get; set; }
        public static string Authority { get; set; }

        /// <summary>
        /// Retrieve a strongly-typed claim lookup result from a user's ClaimsIdentity
        /// </summary>
        /// <typeparam name="T">the type to return (coercing the claim string)</typeparam>
        /// <param name="claimsIdentity">the user's ClaimsIdentity object</param>
        /// <param name="claimName">string name of the claim to retrieve</param>
        /// <returns>the claim value</returns>
        public static T GetClaim<T>(ClaimsIdentity claimsIdentity, string claimName)
        {
            if (claimsIdentity == null)
                return default(T);

            try
            {
                var res = claimsIdentity.Claims.Where(cx => cx.Type == claimName).Select(c => c.Value).SingleOrDefault();
                return (res == null) ? default(T) : (T)Convert.ChangeType(res, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        public static T GetClaim<T>(string claimName)
        {
            ClaimsPrincipal claimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            if (claimsPrincipal == null)
                return default(T);

            try
            {
                var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;
                var res = claimsIdentity.Claims.Where(cx => cx.Type == claimName).Select(c => c.Value).SingleOrDefault();
                return (T)Convert.ChangeType(res, typeof(T));
            }
            catch
            {
                return default(T);
            }

        }
        public static string GetClaim(string claimName)
        {
            return GetClaim<string>(claimName);
        }
        /// <summary>
        /// Retrieve a strongly-typed claim lookup result from a user's ClaimsPrincipal
        /// </summary>
        /// <typeparam name="T">the type to return (coercing the claim string)</typeparam>
        /// <param name="claimsPrincipal">the user's ClaimsPrincipal object</param>
        /// <param name="claimName">string name of the claim to retrieve</param>
        /// <returns>the claim value</returns>
        public static T GetClaim<T>(ClaimsPrincipal claimsPrincipal, string claimName)
        {
            if (claimsPrincipal == null)
                return default(T);

            try
            {
                var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;
                return GetClaim<T>(claimsIdentity, claimName);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Retrieve a claim lookup result from a user's ClaimsPrincipal
        /// </summary>
        /// <param name="claimsPrincipal">the user's ClaimsPrincipal object</param>
        /// <param name="claimName">string name of the claim to retrieve</param>
        /// <returns>the claim value string</returns>
        public static string GetClaim(ClaimsPrincipal claimsPrincipal, string claimName)
        {
            return GetClaim<string>(claimsPrincipal, claimName);
        }

        /// <summary>
        /// Retrieve a claim lookup result from a user's ClaimsIdentity
        /// </summary>
        /// <param name="claimsIdentity">the user's ClaimsIdentity</param>
        /// <param name="claimName">string name of the claim to retrieve</param>
        /// <returns>the claim value string</returns>
        public static string GetClaim(ClaimsIdentity claimsIdentity, string claimName)
        {
            return GetClaim<string>(claimsIdentity, claimName);
        }

        /// <summary>
        /// Return the collection of the user's current system roles
        /// </summary>
        /// <param name="claimsPrincipal">the user's ClaimsPrincipal object</param>
        /// <returns>collection of strings</returns>
        public static IEnumerable<String> GetRoles(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
                throw new NullReferenceException("claimsPrinciple is null");

            return claimsPrincipal.Claims.Where(x => x.Type == System.Security.Claims.ClaimTypes.Role).Select(n => n.Value);
        }

        internal static string GetFQDN(HttpRequestContext request)
        {
            return GetFQDN(request.Url.Request.RequestUri.Scheme, request.Url.Request.RequestUri.Authority);
        }

        public static string GetFQDN(IOwinRequest request)
        {
            return GetFQDN(request.Scheme, request.Host.Value);
        }

        public static string GetFQDN(HttpRequestBase request)
        {
            return GetFQDN(request.Url.Scheme, request.Url.Authority);
        }

        public static string GetFQDN(string scheme, string host)
        {
            return scheme + "://" + host;
        }
        public static string GetFQDN(HttpControllerContext ctx)
        {
            return GetFQDN(ctx.Request.RequestUri.Scheme, ctx.Request.RequestUri.Authority);
        }

        public static EncryptedObj Encrypt(byte[] data)
        {
            AesManaged aes = new AesManaged();
            var keys = AESEncryption.GetKeys(aes.IV);
            var str = Convert.ToBase64String(data);
            var res = AESEncryption.EncryptStringToBytes_Aes(str, keys.Key, keys.IV);
            return new EncryptedObj
            {
                VectorData = aes.IV,
                EncryptedData = res
            };
        }

        public static byte[] Decrypt(EncryptedObj data)
        {
            var keys = AESEncryption.GetKeys(data.VectorData);
            var str = AESEncryption.DecryptStringFromBytes_Aes(data.EncryptedData, keys.Key, keys.IV);
            var res = Convert.FromBase64String(str);
            return res;
        }

        public static async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification context)
        {
            try
            {
                var code = context.Code;

                ClientCredential credential = new ClientCredential(ClientId, ClientSecret);
                string signedInUserID = context.AuthenticationTicket.Identity.FindFirst(TokenCacheClaimTypes.ObjectId).Value;
                AuthenticationContext authContext = new AuthenticationContext(Authority, new AdalCosmosTokenCache(signedInUserID, GetFQDN(context.Request)));
                var redirectUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path);
                AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(
                    code, new Uri(redirectUrl), credential, "https://login.microsoftonline.com/");
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error caching auth code", System.Diagnostics.EventLogEntryType.Error, ex);
                throw;
            }
        }

    }
}