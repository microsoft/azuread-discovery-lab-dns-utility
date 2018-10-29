using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Threading.Tasks;
using Lab.Common.Infra;
using Lab.Data;

namespace Lab.Common
{
    public static class Logging
    {
        /// <summary>
        /// For troubleshooting: write a "Debugging Message" to the SQL Local Error Logs. Will not include exception details nor the caller's environment.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="entities"></param>
        /// <returns>ErrorID (string)</returns>
        public async static Task<string> WriteMessageToErrorLog(string message)
        {
            return await WriteDebugInfoToErrorLog(message, new Exception(message));
        }

        /// <summary>
        /// For message logging with context: write a "Debugging Message" to the SQL Local Error Logs. Will not include exception details but includes the caller's environment.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="entities"></param>
        /// <returns>ErrorID (string)</returns>
        public async static Task<string> WriteMessageToErrorLog(string message, RequestDTO request)
        {
            return await WriteDebugInfoToErrorLog(message, new Exception(message), request);
        }

        public async static Task<string> WriteDebugInfoToErrorLog(string message, Exception ex, AuthorizationContext filterContext)
        {
            return await WriteDebugInfoToErrorLog(message, ex, new RequestDTO(filterContext.HttpContext));
        }

        /// <summary>
        /// Log an exception in the Error table. Will include exception details only.
        /// Optionally include user comments.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="entities"></param>
        /// <param name="userComment"></param>
        /// <returns>ErrorID (string)</returns>
        public async static Task<string> WriteDebugInfoToErrorLog(string message, Exception ex, string userComment = "")
        {
            return await WriteDebugInfoToErrorLog(message, ex, null, userComment);
        }

        /// <summary>
        /// Log an exception in the Error table. Includes all details from the MVC ExceptionContext filter.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="filterContext"></param>
        /// <returns>ErrorID (string)</returns>
        public async static Task<string> WriteDebugInfoToErrorLog(string message, ExceptionContext filterContext)
        {
            return await WriteDebugInfoToErrorLog(message, filterContext.Exception, new RequestDTO(filterContext.RequestContext.HttpContext), null);
        }

        ///// <summary>
        ///// Log an exception in the Error table. Includes all details from the WebAPI ActionContext filter.
        ///// </summary>
        ///// <param name="message"></param>
        ///// <param name="actionContext"></param>
        ///// <param name="ctx"></param>
        ///// <returns>ErrorID (string)</returns>
        public async static Task<string> WriteDebugInfoToErrorLog(string message, HttpActionExecutedContext actionContext)
        {
            return await WriteDebugInfoToErrorLog(message, actionContext.Exception, new RequestDTO(actionContext.Request));
        }

        /// <summary>
        /// Log an exception in the Error table. Includes exception details and user environment.
        /// Optionally include user comments.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="ctx"></param>
        /// <param name="entities"></param>
        /// <param name="userComment"></param>
        /// <returns>ErrorID (string)</returns>
        public async static Task<string> WriteDebugInfoToErrorLog(string message, Exception ex, RequestDTO request, string userComment = "")
        {
            IErrorMgr emgr = new ErrorMgr(request);
            var res = await emgr.InsertError(ex, message, userComment);
            return res.DbErrorId;
        }

        /// <summary>
        /// Writes an error entry to the Application log, Application Source. This is a fallback error writing mechanism.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="errorType">Type of error.</param>
        /// <param name="ex">Original exception (optional)</param>
        public static void WriteToAppLog(string message, EventLogEntryType errorType, Exception ex = null)
        {
            if (ex != null)
            {
                message += message + " (original error: " + ex.Source + "/" + ex.Message + "\r\nStack Trace: " +
                                ex.StackTrace + ")";
                if (ex.InnerException != null)
                {
                    message += "\r\nInner Exception: " + ex.GetBaseException();
                }
            }
            EventLog.WriteEntry("Application", message, errorType, 0);
        }
        public static string GetExceptionMessageString(Exception ex)
        {
            var message = string.Format("Exception     : {0}\n" +
                            "InnerException: {1}",
                ex,
                ex.InnerException);
            return message;
        }

        #region helpers
        private static string FormatAlertMessage(string message, string source)
        {
            var res = new StringBuilder();
            res.AppendFormat("<p>The following error occured at {0} (server time):</p>", DateTime.Now);
            res.AppendLine("    <blockquote>");
            res.AppendFormat("      Message: {0}<br>", message);
            res.AppendFormat("      Source: {0}<br>", source);
            res.AppendLine("    </blockquote>");
            res.Append("<p>View error details at the Kudu System Error Log");
            return Utils.GetHtmlMessageWrapper("Application Alert", res.ToString());
        }
        #endregion
    }
}
