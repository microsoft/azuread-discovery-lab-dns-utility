using Lab.Common;
using Microsoft.Owin.Security;
using System;
using System.Web;
using System.Web.Mvc;

namespace AzureADLabDNSControl.Infra
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Check authorization
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var redir = filterContext.HttpContext.Request.Url.OriginalString;
                filterContext.HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redir },
                    CustomAuthType.LabAdmin);
                return;
            }
        }
    }
}