using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Claims;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
 
namespace AzureADLabDNSControl.Infra
{
    public static class SiteUtils
    {

        public static string GetDomain()
        {
            return ConfigurationManager.AppSettings["Domain"];
        }


        public static string GetHtmlMessageWrapper(string title, string body)
        {
            var res = new StringBuilder("<html><head>");
            res.AppendFormat("<title>{0}</title>", title);
            res.AppendLine("<style type='text/css'>");
            res.AppendLine("    p, blockquote, div, span {font:11pt calibri, arial}");
            res.AppendLine("</style></head>");
            res.AppendLine("<body><div>");
            res.Append(body);
            res.AppendLine("</div></body></html>");
            return res.ToString();
        }

        public static IEnumerable<SelectListItem> SetSelected(IEnumerable<SelectListItem> list, string selected)
        {
            if (selected == null) return list;

            var item = list.FirstOrDefault(l => l.Text == selected);
            if (item != null) item.Selected = true;
            return list;
        }

        public static IEnumerable<SelectListItem> GetListFromEnum<T>(T defaultSelected) where T : IConvertible
        {
            return GetListFromEnum(defaultSelected.ToString(CultureInfo.InvariantCulture));
        }

        public static IEnumerable<SelectListItem> GetListFromEnum<T>(string defaultSelected = null) where T : IConvertible
        {
            IList<SelectListItem> res = (from object o in Enum.GetValues(typeof(T))
                                         select new SelectListItem
                                         {
                                             Selected = (o.ToString() == defaultSelected),
                                             Text = SplitCamelCase(o.ToString()),
                                             Value = Enum.GetName(typeof(T), o)
                                         }).ToList();
            return res.ToList();
        }

        public static IEnumerable<SelectListItem> LoadListFromDictionary(Dictionary<string, string> list)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            foreach (var item in list)
            {
                selectListItems.Add(new SelectListItem
                {
                    Text = item.Value,
                    Value = item.Key
                });
            }
            return selectListItems;
        }


        public static IEnumerable<SelectListItem> LoadListFromArray(IEnumerable<string> list)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            foreach (var item in list)
            {
                selectListItems.Add(new SelectListItem
                {
                    Text = item,
                    Value = item
                });
            }
            return selectListItems;
        }

        /// <summary>
        /// Insert spaces before camel-cased words in a token, i.e., "ThisIsAString" to "This Is A String"
        /// </summary>
        /// <param name="s">the token</param>
        /// <returns>string</returns>
        public static string SplitCamelCase(String s)
        {
            return Regex.Replace(s, "([a-z](?=[A-Z0-9])|[A-Z](?=[A-Z][a-z]))", "$1 ");
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

        public static void UpsertCookie(HttpContextBase hctx, string name, string value)
        {
            UpsertCookie((HttpContextWrapper)hctx, name, value);
        }

        public static void UpsertCookie(HttpContextWrapper hctx, string name, string value)
        {
            var cookie = hctx.Request.Cookies.Get(name);
            if (cookie == null)
            {
                hctx.Response.Cookies.Add(new HttpCookie(name, value)
                {
                    HttpOnly = true,
                    Secure = true
                });
            }
            else
            {
                cookie.Value = value;
                hctx.Response.Cookies.Set(cookie);
            }
        }
    }
}