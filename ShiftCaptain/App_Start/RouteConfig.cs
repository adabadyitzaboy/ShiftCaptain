using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ShiftCaptain
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            var actions = new List<String> { "Create", "Edit", "Details", "Delete"};
            foreach (var route in new List<String> { "Shift", "ShiftPreference", "User", "Room" })
            {
                routes.MapRoute(
                  route,                                           // Route name
                  "{VersionName}/" + route,                            // URL with parameters
                  new { controller = route, action = "Index" }  // Parameter defaults
                  );
                if (route != "Shift" && route != "ShiftPreference")
                {
                    foreach (var action in actions)
                    {
                        var test = String.Format("{{VersionName}}/{0}/{1}{2}", route, action, action == "Create" ? "" : "/{id}");
                        routes.MapRoute(
                          route + action,                                           // Route name
                          String.Format("{{VersionName}}/{0}/{1}{2}", route, action, action == "Create" ? "" : "/{id}"),                            // URL with parameters
                          new { controller = route, action = action }  // Parameter defaults
                        );
                    }
                }
            }
            routes.MapRoute(
                "Version",                                           // Route name
                "{VersionName}/ChangeVersion",                            // URL with parameters
                new { controller = "Version", action = "ChangeVersion" }  // Parameter defaults
            );

            foreach (var route in new List<String> { "ScheduleNotReady", "NoVersions", "InternalError", "NotAuthorized", "PageNotFound" })
            {
                routes.MapRoute(
                  route,                                           // Route name
                  route,                            // URL with parameters
                  new { controller = "Error", action = route }  // Parameter defaults
                  );
            }
            foreach (var route in new List<String> { "NoBuildings", "NoRooms", "NoPreferences", "NoUsers"})
            {
                routes.MapRoute(
                  route,                                           // Route name
                  "{VersionName}/" + route,                            // URL with parameters
                  new { controller = "Error", action = route }  // Parameter defaults
                  );
            }
            foreach (var route in new List<String> { "ValidateSchedule", "ApproveSchedule", "DisapproveSchedule", "RejectSchedule", "CloneSchedule" })
            {
                routes.MapRoute(
                  route,                                           // Route name
                  "{VersionName}/" + route,                            // URL with parameters
                  new { controller = "ManageSchedule", action = route }  // Parameter defaults
                  );
            }
            
            routes.MapRoute(
                "ForgotPassword",                                           // Route name
                "ForgotPassword/",                            // URL with parameters
                new { controller = "EmailTemplate", action = "ForgotPassword" }  // Parameter defaults
            );

            routes.MapRoute(
                "ResetPassword",                                           // Route name
                "ResetPassword/{id}",                            // URL with parameters
                new { controller = "EmailTemplate", action = "ResetPassword" }  // Parameter defaults
            );

            routes.MapRoute(
                "Profile",                                           // Route name
                "{VersionName}/Profile/",                            // URL with parameters
                new { controller = "User", action = "EditProfile" }  // Parameter defaults
            );
            routes.MapRoute(
                "Output",                                           // Route name
                "{VersionName}/Download/",                            // URL with parameters
                new { controller = "Output", action = "Download" }  // Parameter defaults
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Shift", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}