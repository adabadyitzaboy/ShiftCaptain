using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShiftCaptain.Models;
using WebMatrix.WebData;
using ShiftCaptain.Filters;
using ShiftCaptain.Infrastructure;

namespace ShiftCaptain.Controllers
{
//    [Authorized]
// not necessary since all methods allow anonymous
    public class LoginController : Controller
    {
        private ShiftCaptainEntities db = new ShiftCaptainEntities();

        //
        // GET: /Login/LogOff
        public ActionResult LogOff()
        {
            Authentication.LogOff();
            return RedirectToAction("Index", "Login");
        }

        //
        // GET: /Login/
        [AllowAnonymous]
        public ActionResult Index(string returnUrl)
        {
            if (SessionManager.IsLoggedIn)
            {
                Authentication.LogOff();
                return RedirectToAction("Index", "Login");//Redirect back to ourself
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        //
        // POST: /Login/

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Index(EmailNPass user, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //var auth = HttpContext.Current.User.Identity.IsAuthenticated;
                String errorMessage = "";
                if (ModelState.IsValid && Authentication.Login(user, ref errorMessage))
                {
                    return RedirectToLocal(returnUrl);
                }

                // If we got this far, something failed, redisplay form
                ModelState.AddModelError("", errorMessage);
            }
            return View();
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Shift");
            }
        }
        #endregion
    }
}