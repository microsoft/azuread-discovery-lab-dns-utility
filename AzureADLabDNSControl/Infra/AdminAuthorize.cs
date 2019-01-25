using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

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
            var currentUser = filterContext.HttpContext.User.Identity;
            var redir = new RouteValueDictionary
                {
                    { "controller", "account" }, { "action", "signinadmin" }, { "area", "" }
                };

            if (!currentUser.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(redir);
                return;
            }
        }
    }
}