using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Lab.Common.Infra
{
    public class RequestDTO
    {
        public string UserAgent { get; set; }
        public string IPAddress { get; set; }
        public Uri UrlReferrer { get; set; }
        public NameValueCollection Form  { get; set; }
        public Uri Url { get; set; }
        public string UserName { get; set; }

        public RequestDTO()
        {
            Form = new NameValueCollection();
        }
        public RequestDTO(HttpContext ctx)
        {
            if (ctx == null) return;
            SetCtxProps(ctx);
        }

        public RequestDTO(HttpContextBase ctx)
        {
            SetCtxProps(ctx);
        }

        public RequestDTO(HttpRequestMessage request)
        {
            Form = request.Content.ReadAsFormDataAsync().Result;
            Url = request.RequestUri;
            IPAddress = request.GetClientIpAddress();
            UrlReferrer = request.Headers.Referrer;
            UserAgent = request.Headers.UserAgent.ToString();
            UserName = request.GetRequestContext().Principal.Identity.Name;
        }

        private void SetCtxProps(dynamic ctx)
        {
            Form = ctx.Request.Form;
            Url = ctx.Request.Url;
            IPAddress = ctx.Request.ServerVariables["REMOTE_HOST"];
            UrlReferrer = ctx.Request.UrlReferrer;
            UserAgent = ctx.Request.UserAgent;
            UserName = ctx.User.Identity.Name;
        }

        //item.UserAgent = _request.UserAgent;
        //item.IPAddress = _request.ServerVariables["REMOTE_HOST"];
        //item.Referrer = (_request.UrlReferrer==null) ? "N/A" : _request.UrlReferrer.ToString();
        //item.PostData = WebUtility.HtmlEncode(_request.Form.ToString());
        //item.QSData = WebUtility.HtmlEncode((_request.Url == null) ? "N/A" : _request.Url.Query);
    }
}