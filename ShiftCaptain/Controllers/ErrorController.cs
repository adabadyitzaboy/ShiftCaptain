using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShiftCaptain.Models;
using ShiftCaptain.Filters;

namespace ShiftCaptain.Controllers
{
    public class ErrorController : BaseController
    {
        public ErrorController()
        {
            ClassName = "Error";
        }

        //
        // GET: /InternalError/
        [AllowAnonymous]
        public ActionResult InternalError(string page)
        {
            return View();
        }

        //
        // GET: /NotAuthorized/
        [AllowAnonymous]
        public ActionResult NotAuthorized(string page)
        {
            return View();
        }
        
        //
        // GET: /PageNotFound/
        [AllowAnonymous]
        public ActionResult PageNotFound(string page)
        {
            return View();
        }

        //
        // GET: /ScheduleNotReady/
        [AllowAnonymous]
        public ActionResult ScheduleNotReady()
        {
            return View();
        }

        //
        // GET: /NoVersions/
        [ManagerAccess]
        [ShiftManagerAccess]
        public ActionResult NoVersions()
        {
            return View();
        }
        
        //
        // GET: /NoBuildings/
        [ManagerAccess]
        [ShiftManagerAccess]
        public ActionResult NoBuildings(String VersionName)
        {
            return View();
        }

        //
        // GET: /NoRooms/
        [ManagerAccess]
        [ShiftManagerAccess]
        public ActionResult NoRooms(String VersionName)
        {
            return View();
        }

        //
        // GET: /NoUsers/
        [ManagerAccess]
        [ShiftManagerAccess]
        public ActionResult NoUsers(String VersionName)
        {
            return View();
        }

        //
        // GET: /NoPreferences/
        [ManagerAccess]
        [ShiftManagerAccess]
        public ActionResult NoPreferences()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}