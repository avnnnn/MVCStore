using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MVCStore
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               name: "CategoryMenuPartial",
               url: "Pages/CategoryMenuPartial",
               defaults: new { controller = "Shop", action = "CategoryMenuPartial" },
               namespaces: new[] { "MVCStore.Controllers" }
           );
            routes.MapRoute(
               name: "SidebarPartial",
               url: "Pages/SidebarPartial",
               defaults: new { controller = "Pages", action = "SidebarPartial" },
               namespaces: new[] { "MVCStore.Controllers" }
           );
            
            routes.MapRoute(
               name: "PagesMenuPartial",
               url: "Pages/PagesMenuPartial",
               defaults: new { controller = "Pages", action = "PagesMenuPartial" },
               namespaces: new[] { "MVCStore.Controllers" }
           );
            routes.MapRoute(
                name: "Pages",
                url: "{page}",
                defaults: new { controller = "Pages", action = "Index" },
                namespaces: new[] { "MVCStore.Controllers" }
            );
            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "Pages", action = "Index" },
                namespaces: new[] { "MVCStore.Controllers" }
            );

        }
    }
}
