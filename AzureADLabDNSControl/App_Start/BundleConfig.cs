using System.Web;
using System.Web.Optimization;

namespace AzureADLabDNSControl
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.gritter.js",
                        "~/Scripts/moment.min.js",
                        "~/Scripts/moment-timezone-with-data.min.js",
                        "~/Scripts/global.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                       "~/Scripts/bootstrap-datetimepicker.js",
                       "~/Scripts/respond.js"
                       ));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                    "~/Scripts/lib/jquery.dataTables.js",
                    "~/Scripts/lib/dataTables.bootstrap.js"));

            bundles.Add(new StyleBundle("~/content/datatablescss").Include(
                    "~/Content/datatables/css/dataTables.bootstrap.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-theme.css",
                      "~/Content/bootstrap-datetimepicker.css",
                      "~/Content/jquery.gritter.css",
                      "~/Content/site.css"));
        }
    }
}
