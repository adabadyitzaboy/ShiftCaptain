﻿using System;
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

            routes.MapRoute(
                "ValidateSchedule",                                           // Route name
                "ValidateSchedule/{id}",                            // URL with parameters
                new { controller = "ManageSchedule", action = "ValidateSchedule" }  // Parameter defaults
            );

            routes.MapRoute(
                "ApproveSchedule",                                           // Route name
                "ApproveSchedule/{id}",                            // URL with parameters
                new { controller = "ManageSchedule", action = "ApproveSchedule" }  // Parameter defaults
            );

            routes.MapRoute(
                "RejectSchedule",                                           // Route name
                "RejectSchedule/{id}",                            // URL with parameters
                new { controller = "ManageSchedule", action = "RejectSchedule" }  // Parameter defaults
            );

            routes.MapRoute(
                "CloneSchedule",                                           // Route name
                "CloneSchedule/{id}",                            // URL with parameters
                new { controller = "ManageSchedule", action = "CloneSchedule" }  // Parameter defaults
            );

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
                "Profile/",                            // URL with parameters
                new { controller = "User", action = "EditProfile" }  // Parameter defaults
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Shift", action = "Index", id = UrlParameter.Optional }
            );
            
        }
    }
}