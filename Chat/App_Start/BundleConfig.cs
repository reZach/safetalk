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
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/knockout-3.4.1.min.js",
                        "~/Scripts/modernizr-*",
                        "~/Scripts/facebooksdk.js",
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/cookie.js",
                        "~/Scripts/ion.sound.min.js"));

            bundles.Add(new ScriptBundle("~/chatscripts").Include(
                    "~/Scripts/chatViewModel.js"
                ));

            bundles.Add(new ScriptBundle("~/chatpage").Include(
                    "~/Scripts/jquery.signalR-2.2.1.min.js",
                    "~/Scripts/server.js",
                    "~/Scripts/chatViewModel.js"
                ));

            bundles.Add(new ScriptBundle("~/chatpageafter").Include(
                    "~/Scripts/chatpage.js"
                ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/Site.css"));
            
        }
    }
}
