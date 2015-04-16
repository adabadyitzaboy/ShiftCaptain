using log4net;
using ShiftCaptain.Infrastructure;
using ShiftCaptain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ShiftCaptain.Filters
{
    public class BaseActionFilterAttribute : ActionFilterAttribute
    {
        public class HandleExceptionsAttribute : HandleErrorAttribute
        {
            public ILog Logger = LogManager.GetLogger(typeof(HandleErrorAttribute));
            public override void OnException(ExceptionContext filterContext)
            {
                var exception = filterContext.Exception;
                Logger.Error(exception.Message, exception);

                // custom code here..
            }
        }
        public String GetVersionName(ActionExecutingContext filterContext)
        {
            return GetVersion(filterContext).Name;
        }
        public String _getVersionName(ActionExecutingContext filterContext)
        {
            String versionName = String.Empty;
            if (filterContext.ActionParameters.Keys.Contains("VersionName") && filterContext.ActionParameters["VersionName"] != null)
            {
                versionName = filterContext.ActionParameters["VersionName"].ToString();
            }
            else
            {
                var name = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                var action = filterContext.ActionDescriptor.ActionName;
                var id = SessionManager.VersionId;
                if (name == "Version" && (action == "Edit" || action == "Delete" || action == "Details") && filterContext.ActionParameters.Count() > 0)
                {
                    if (filterContext.ActionParameters.Keys.Contains("version"))
                    {
                        var version = (ShiftCaptain.Models.Version)filterContext.ActionParameters["version"];
                        if (version != null && version.Id != 0)
                        {
                            id = version.Id;
                        }
                    }
                    else if (filterContext.ActionParameters.Keys.Contains("id"))
                    {
                        id = (int)filterContext.ActionParameters["id"];
                    }

                }
            }
            return versionName;
        }

        public ShiftCaptain.Models.Version GetVersion(ActionExecutingContext filterContext)
        {
            var versionName = _getVersionName(filterContext);
            return SessionManager.GetVersion(versionName);
        }
    }

    public class ManagerAccessAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        { 

        }
    }
    public class ShiftManagerAccessAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

        }
    }
    public class SelfAccessAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

        }
    }
    public class NoSelfAccessAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

        }
    }

    public class VersionRequiredAttribute : BaseActionFilterAttribute
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

    public class BuildingRequiredAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var db = new ShiftCaptainEntities();
            if (db.Buildings.Count() > 0)
            {
                return;
            }
            filterContext.Result = new RedirectToRouteResult(
                  new RouteValueDictionary 
                                   {
                                       { "action", "NoBuildings" },
                                       { "controller", "Error" },
                                       { "VersionName", GetVersionName(filterContext) }
                                   });
        }
    }

    public class RoomRequiredAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var db = new ShiftCaptainEntities();
            ShiftCaptain.Models.Version version = GetVersion(filterContext);
            if (version != null && db.RoomViews.Count(rv=>rv.VersionId == version.Id) > 0)
            {
                return;
            }
            filterContext.Result = new RedirectToRouteResult(
                  new RouteValueDictionary 
                                   {
                                       { "action", "NoRooms" },
                                       { "controller", "Error" },
                                       { "VersionName", version != null? version.Name : String.Empty }
                                   });
        }
    }

    public class PreferenceRequiredAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var db = new ShiftCaptainEntities();
            if (db.Preferences.Count() > 0)
            {
                return;
            }
            filterContext.Result = new RedirectToRouteResult(
                  new RouteValueDictionary 
                                   {
                                       { "action", "NoPreferences" },
                                       { "controller", "Error" }
                                   });
        }
    }

    public class UserRequiredAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var db = new ShiftCaptainEntities();
            ShiftCaptain.Models.Version version = GetVersion(filterContext);
            if (version != null && db.UserViews.Count(uv => uv.VersionId == version.Id) > 0)
            {
                return;
            }
            filterContext.Result = new RedirectToRouteResult(
                  new RouteValueDictionary 
                                   {
                                       { "action", "NoUsers" },
                                       { "controller", "Error" },
                                       { "VersionName", version != null? version.Name : String.Empty}
                                   });
        }
    }
    public class VersionNotApprovedAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var version = GetVersion(filterContext);
            if (version != null && !version.IsApproved)
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

    public class VersionVisibleAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (SessionManager.IsManager || SessionManager.IsShiftManager)
            {
                return;
            }
            
            var db = new ShiftCaptainEntities();
            var version = GetVersion(filterContext);
            if (version.IsVisible)
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