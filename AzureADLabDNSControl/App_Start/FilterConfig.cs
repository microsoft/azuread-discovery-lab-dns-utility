using Lab.Common.Infra;
using System.Web;
using System.Web.Mvc;

namespace AzureADLabDNSControl
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleAndLogErrorAttribute());
        }
    }
}
