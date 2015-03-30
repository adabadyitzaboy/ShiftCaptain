﻿using ShiftCaptain.Infrastructure;
using ShiftCaptain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ShiftCaptain.Filters
{
    public class ManagerAccessAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        { 

        }
    }
    public class ShiftManagerAccessAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

        }
    }
    public class SelfAccessAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

        }
    }
    public class NoSelfAccessAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

        }
    }

    public class VersionRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            
            var db = new ShiftCaptainEntities();
            if (db.Versions.Count() > 0)
            {
                return;
            }
            filterContext.Result = new RedirectToRouteResult(
                  new RouteValueDictionary 
                                   {
                                       { "action", "NoVersions" },
                                       { "controller", "Error" }
                                   });
        }
    }
    public class VersionNotApprovedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var name = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var action = filterContext.ActionDescriptor.ActionName;
            var id = SessionManager.VersionId;
            if(name == "Version" && (action == "Edit" || action == "Delete")  && filterContext.ActionParameters.Count() > 0){
                if (filterContext.ActionParameters.Keys.Contains("version"))
                {
                    var version = (ShiftCaptain.Models.Version)filterContext.ActionParameters["version"];
                    if(version != null && version.Id != 0){
                        id = version.Id;
                    }                    
                }
                else if (filterContext.ActionParameters.Keys.Contains("id"))
                {
                    id = (int)filterContext.ActionParameters["id"];
                }
                
            }
            if (id != 0)
            {
                var db = new ShiftCaptainEntities();
                var version = db.Versions.FirstOrDefault(v => v.Id == id);
                if (version != null && !version.IsApproved)
                {
                    return;
                }
            }
            filterContext.Result = new RedirectToRouteResult(
                  new RouteValueDictionary 
                                   {
                                       { "action", "NotAuthorized" },
                                       { "controller", "Error" },
                                       { "page", filterContext.RequestContext.HttpContext.Request.Url.PathAndQuery}
                                   });
        }        
    }

    public class VersionVisibleAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (SessionManager.IsManager || SessionManager.IsShiftManager)
            {
                return;
            }
            
            var db = new ShiftCaptainEntities();
            var versionCount = db.Versions.Count(v => v.IsVisible);
            if (versionCount > 0)
            {
                return;
            }
            
            filterContext.Result = new RedirectToRouteResult(
                  new RouteValueDictionary 
                                   {
                                       { "action", "NotAuthorized" },
                                       { "controller", "Error" },
                                       { "page", filterContext.RequestContext.HttpContext.Request.Url.PathAndQuery}
                                   });
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AuthorizedAttribute : FilterAttribute, IAuthorizationFilter
    {
        private void NotAuthorized(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                  new RouteValueDictionary 
                                   {
                                       { "action", "NotAuthorized" },
                                       { "controller", "Error" },
                                       { "page", filterContext.RequestContext.HttpContext.Request.Url.PathAndQuery}
                                   });
        }
        private void NotLoggedIn(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                  new RouteValueDictionary 
                                   {
                                       { "action", "Index" },
                                       { "controller", "Login" },
                                       { "returnUrl",filterContext.RequestContext.HttpContext.Request.Url.PathAndQuery}
                                   });
            
        }
        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            /*
             * 
             */
            bool allowAnonymousReq = filterContext.ActionDescriptor.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Count() > 0;
            if (allowAnonymousReq)
            {
                return;
            }
            var authenticated = filterContext.HttpContext.User.Identity.IsAuthenticated;
            if ((authenticated || Authentication.Authenticate(filterContext.HttpContext)) && SessionManager.UserId != 0)
            {
                var db = new ShiftCaptainEntities();
                var validUser = new ShiftCaptainEntities().Users.FirstOrDefault(u => u.Id == SessionManager.UserId);
                if (validUser != null && validUser.IsActive)
                {
                    ActionDescriptor action = filterContext.ActionDescriptor;
                    bool selfAccessReq = action.GetCustomAttributes(typeof(SelfAccessAttribute), true).Count() > 0; 
                    bool noSelfAccessReq = action.GetCustomAttributes(typeof(NoSelfAccessAttribute), true).Count() > 0;                    
                    bool managerAccessReq= action.GetCustomAttributes(typeof(ManagerAccessAttribute), true).Count() > 0;
                    bool shiftManagerAccessReq = action.GetCustomAttributes(typeof(ShiftManagerAccessAttribute), true).Count() > 0;
                    //routeData values
                    if ((!managerAccessReq || validUser.IsManager) && (!shiftManagerAccessReq || validUser.IsManager || validUser.IsShiftManager) && !noSelfAccessReq && !selfAccessReq)
                    {//user has access to content
                        return;//valid
                    }
                    else if (noSelfAccessReq || selfAccessReq)
                    {
                        if (filterContext.RouteData.Values["id"] != null)
                        {
                            var id = filterContext.RouteData.Values["id"].ToString();
                            int uid = 0;
                            if (Int32.TryParse(id, out uid))
                            {
                                if ((uid == SessionManager.UserId && selfAccessReq) || (uid != SessionManager.UserId && noSelfAccessReq))
                                {
                                    return;
                                }
                            }
                        }
                        else if (selfAccessReq)
                        {
                            return;
                        }
                    }
                }
                NotAuthorized(filterContext);
            }
            NotLoggedIn(filterContext);
        }
    }
}