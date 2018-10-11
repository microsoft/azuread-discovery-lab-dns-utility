using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security;
using Infra;

namespace AzureADLabDNSControl.Controllers
{
    public class AccountController : Controller
    {
        public void SignInAdmin()
        {
            var redir = (Request.QueryString["redir"] ?? "/");

            // Send an OpenID Connect sign-in request.
            if (!Request.IsAuthenticated || Request.QueryString["force"] == "true")
            {
                HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redir },
                    CustomAuthType.LabAdmin);
            }
        }

        public void SignIn()
        {
            var redir = (Request.QueryString["redir"] ?? "/");

            // Send an OpenID Connect sign-in request.
            if (!Request.IsAuthenticated || Request.QueryString["force"] == "true")
            {
                HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redir },
                    CustomAuthType.LabUser);
            }
        }

        public void SignOut()
        {
            string callbackUrl = Url.Action("SignOutCallback", "Account", routeValues: null, protocol: Request.Url.Scheme);

            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                OpenIdConnectAuthenticationDefaults.AuthenticationType, 
                CookieAuthenticationDefaults.AuthenticationType,
                CustomAuthType.LabAdmin,
                CustomAuthType.LabUser);
        }

        public ActionResult SignOutCallback()
        {
            if (Request.IsAuthenticated)
            {
                // Redirect to home page if the user is authenticated.
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
