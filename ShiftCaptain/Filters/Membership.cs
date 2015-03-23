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

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AuthorizedAttribute : FilterAttribute, IAuthorizationFilter
    {
        private void NotAuthorized(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                  new RouteValueDictionary 
                                   {
                                       { "action", "Index" },
                                       { "controller", "Login" }
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
            }
            NotAuthorized(filterContext);
        }
    }
}