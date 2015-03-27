using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShiftCaptain.Models;

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
        public ActionResult InternalError(string returnUrl)
        {
            return View();
        }

        //
        // GET: /NotAuthorized/
        [AllowAnonymous]
        public ActionResult NotAuthorized(string returnUrl)
        {
            return View();
        }
        
        //
        // GET: /PageNotFound/
        [AllowAnonymous]
        public ActionResult PageNotFound(string returnUrl)
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}