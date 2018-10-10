using System;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Notifications;
using Microsoft.IdentityModel.Protocols;
using System.Web.Mvc;
using System.Configuration;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens;
using Infra;
using System.Web;
using System.Linq;
using System.Security.Claims;
using AzureADLabDNSControl.Infra;
using Graph;

namespace AzureADLabDNSControl
{
    public partial class Startup
    {
        private static string aadInstance = EnsureTrailingSlash(ConfigurationManager.AppSettings["ida:AADInstance"]);

        public static string LabAdminClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        public static string LabAdminSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        public static string LabAdminTenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        public static string Authority = "https://login.microsoftonline.com/{0}";
        public static string adminAuthority = String.Format(Authority, LabAdminTenantId);

        public static string LabUserClientId = ConfigurationManager.AppSettings["LinkAdminClientId"];
        public static string LabUserSecret = ConfigurationManager.AppSettings["LinkAdminSecret"];
        public static string userAuthority = String.Format(Authority, "common");

        public static string GraphResource = "https://graph.microsoft.com";

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            var authProvider = new CookieAuthenticationProvider
            {
                OnResponseSignIn = ctx =>
                {
                    var task = Task.Run(async () => {
                        await AuthInit(ctx);
                    });
                    task.Wait();
                },
                OnValidateIdentity = ctx =>
                {
                    //good spot to troubleshoot nonces, etc...
                    return Task.FromResult(0);
                }
            };

            var cookieOptions = new CookieAuthenticationOptions
            {
                Provider = authProvider,
                CookieManager = new Microsoft.Owin.Host.SystemWeb.SystemWebChunkingCookieManager()
            };

            app.UseCookieAuthentication(cookieOptions);

            OpenIdConnectAuthenticationOptions LabAdminOptions = new OpenIdConnectAuthenticationOptions
            {
                ClientId = LabAdminClientId,
                Authority = adminAuthority,
                PostLogoutRedirectUri = "/",
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    RedirectToIdentityProvider = (context) =>
                    {
                        string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                        context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
                        context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;
                        return Task.FromResult(0);
                    },
                },
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                },
                AuthenticationType = CustomAuthType.LabAdmin
            };
            app.UseOpenIdConnectAuthentication(LabAdminOptions);

            OpenIdConnectAuthenticationOptions LabUserOptions = new OpenIdConnectAuthenticationOptions
            {
                ClientId = LabUserClientId,
                Authority = userAuthority,
                PostLogoutRedirectUri = "/",
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    RedirectToIdentityProvider = (context) =>
                    {
                        string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                        context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
                        context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;
                        context.ProtocolMessage.Prompt = "login";
                        return Task.FromResult(0);
                    },
                },
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                },
                AuthenticationType = CustomAuthType.LabUser
            };
            app.UseOpenIdConnectAuthentication(LabUserOptions);

        }

        private static string EnsureTrailingSlash(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }

            return value;
        }
    }
}
