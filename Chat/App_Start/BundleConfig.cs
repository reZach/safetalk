using System.Web;
using System.Web.Optimization;

namespace Chat
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/javascript").Include(
                        "~/Scripts/lib/jquery/jquery-{version}.js",
                        "~/Scripts/lib/knockout/knockout-3.4.1.min.js",
                        "~/Scripts/lib/modernizr/modernizr-*",
                        "~/Scripts/lib/facebook/facebooksdk.js",
                        "~/Scripts/lib/bootstrap/bootstrap.js",
                        "~/Scripts/lib/list/list.min.js",
                        "~/Scripts/lib/mdn/cookie.js",
                        "~/Scripts/lib/ion.sound/ion.sound.min.js"));

            bundles.Add(new ScriptBundle("~/chatscripts").Include(
                    "~/Scripts/chatViewModel.js"
                ));

            bundles.Add(new ScriptBundle("~/chatpage").Include(
                    "~/Scripts/lib/jquery/jquery.signalR-2.2.1.min.js",
                    "~/Scripts/server.js",
                    "~/Scripts/chatViewModel.js"
                ));

            bundles.Add(new ScriptBundle("~/chatpageafter").Include(
                    "~/Scripts/chatpage.js"
                ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/Bootstrap/bootstrap.css",
                      "~/Content/Site.css"));
            
        }
    }
}
