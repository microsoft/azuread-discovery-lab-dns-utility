using Lab.Common.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AzureADLabDNSControl
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.Filters.Add(new HandleWebApiException());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
               name: "ActionAndIdApi",
               routeTemplate: "api/{controller}/{action}/{id}",
               defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
               name: "ActionOnlyApi",
               routeTemplate: "api/{controller}/{action}",
               defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
