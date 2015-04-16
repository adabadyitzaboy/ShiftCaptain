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

    [ShiftCaptain.Filters.BaseActionFilterAttribute.HandleExceptions]
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
                    if (returnUrl == "/")
                    {
                        if (db.Versions.Count() == 0)
                        {
                            return RedirectToLocal("/Version");
                        }
                        var version = db.Versions.FirstOrDefault(v => v.IsActive);
                        if (version == null)
                        {
                            if (SessionManager.IsManager || SessionManager.IsShiftManager)
                            {
                                version = db.Versions.OrderByDescending(v => v.Id).FirstOrDefault();
                            }
                            else
                            {
                                version = db.Versions.OrderByDescending(v => v.Id).FirstOrDefault(v=>v.IsVisible);
                            }
                            if (version == null)
                            {
                                return RedirectToLocal("/Version");
                            }
                        }
                        SessionManager.VersionId = version.Id;
                        if (db.Buildings.Count() == 0)
                        {
                            return RedirectToLocal("/Building");
                        }
                        else if (db.RoomViews.Count(rv => rv.VersionId == version.Id) == 0)
                        {
                            return RedirectToAction("Index", "Room", new { VersionName = version.EncodedName });
                        }
                        else if (db.UserViews.Count(uv => uv.VersionId == version.Id) == 0)
                        {
                            return RedirectToAction("Index", "User", new { VersionName = version.EncodedName });
                        }
                        else if (db.Preferences.Count() == 0)
                        {
                            return RedirectToLocal("/Preference");
                        }
                    }
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