using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Mvc;
using Lab.Common;

namespace Lab.Common.Infra
{
    public class HandleAndLogErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            var message = string.Format("Exception     : {0}\n" +
                                        "InnerException: {1}",
                filterContext.Exception,
                filterContext.Exception.InnerException);

            var request = new RequestDTO(filterContext.RequestContext.HttpContext);

            string eid = null;
            var task = Task.Run(async () => {
                eid = await Logging.WriteDebugInfoToErrorLog(message, filterContext.Exception, request, null);
            });
            task.Wait();

            filterContext.HttpContext.Items.Add("ErrorID", eid);
            if (HttpContext.Current.Session != null) HttpContext.Current.Session["ErrorID"] = eid;

            filterContext.ExceptionHandled = true;

            base.OnException(filterContext);

            // Verify if AJAX request
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                // Use partial view in case of AJAX request
                var result = new JsonResult
                {
                    Data = new ErrResponsePoco
                    {
                        DbErrorId = eid
                    }
                };
                filterContext.Result = result;
            }
            else
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Error.cshtml"
                };
            }
        }
    }

    public class HandleWebApiException : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.InternalServerError);
            var resex = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var res = new ErrResponsePoco();

            var req = new RequestDTO(actionContext.Request);

            var task = Task.Run(async () => {
                res.DbErrorId = await Logging.WriteDebugInfoToErrorLog("WebAPI Error", actionContext.Exception, req);
            });
            task.Wait();
            res.ErrorMessage = actionContext.Exception.Message;
            res.ErrorTitle = "API Error";
            
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items.Add("ErrorID", res.DbErrorId);
                HttpContext.Current.ClearError();
            }

            resex.Content = new ObjectContent(typeof(ErrResponsePoco), res, new JsonMediaTypeFormatter());

            throw new HttpResponseException(resex);
        }
    }

    public static class HttpRequestMessageExtensions
    {
        private const string HttpContext = "MS_HttpContext";
        private const string RemoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";

        public static string GetClientIpAddress(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey(HttpContext))
            {
                dynamic ctx = request.Properties[HttpContext];
                if (ctx != null)
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            if (request.Properties.ContainsKey(RemoteEndpointMessage))
            {
                dynamic remoteEndpoint = request.Properties[RemoteEndpointMessage];
                if (remoteEndpoint != null)
                {
                    return remoteEndpoint.Address;
                }
            }

            return null;
        }
    }
}