using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Chat
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Root",
                url: "",
                defaults: new { controller = "Chat", action = "Index", id = UrlParameter.Optional}
            );
            routes.MapRoute(
                name: "Landing",
                url: "{controller}",
                defaults: new { action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Chat", action = "Index", id = UrlParameter.Optional }
            );
            
        }
    }
}
