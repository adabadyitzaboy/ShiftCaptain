using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShiftCaptain.Models;
using ShiftCaptain.Filters;
using System.Configuration;
using ShiftCaptain.Infrastructure;

namespace ShiftCaptain.Controllers
{
    public class EmailTemplateController : BaseController
    {

        //
        // GET: /EmailTemplate/

        [ManagerAccess]
        public ActionResult Index()
        {
            return View(db.EmailTemplates.ToList());
        }

        //
        // GET: /EmailTemplate/Details/5

        [ManagerAccess]
        public ActionResult Details(string id = null)
        {
            EmailTemplate emailtemplate = db.EmailTemplates.Find(id);
            if (emailtemplate == null)
            {
                return HttpNotFound();
            }
            return View(emailtemplate);
        }
        
        //
        // GET: /EmailTemplate/Edit/5

        [ManagerAccess]
        public ActionResult Edit(string id = null)
        {
            EmailTemplate emailtemplate = db.EmailTemplates.Find(id);
            if (emailtemplate == null)
            {
                return HttpNotFound();
            }
            return View(emailtemplate);
        }

        //
        // POST: /EmailTemplate/Edit/5

        [HttpPost]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EmailTemplate emailtemplate)
        {
            if (ModelState.IsValid)
            {
                db.Entry(emailtemplate).State = EntityState.Modified;
                SaveChange();
                return RedirectToAction("Index");
            }
            return View(emailtemplate);
        }

        //
        // GET: /ResetPassword/5

        [ShiftManagerAccess]
        [ManagerAccess]
        public ActionResult ResetPassword(int Id)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == Id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var emailObj = new EmailNPass { EmailAddress = user.EmailAddress };
            return View(emailObj);
        }

        //
        // POST: /ForgotPassword/

        [HttpPost]
        [ShiftManagerAccess]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword([Bind(Exclude = "Pass")]EmailNPass emailObj)
        {
            if (SendPasswordEmail(emailObj, "ResetPassword", "Reset Password Email Template", currentUser.EmailAddress))
            {
                return RedirectToAction("Index", "User");
            }
            else
            {
                return View();
            }
        }

        private bool SendPasswordEmail(EmailNPass emailObj, String emailTemplateName, String emailTemplateDisplayName, String fromAddress)
        {
            ModelState.Remove("Pass");
            if (ModelState.IsValid)
            {
                var validUser = db.Users.FirstOrDefault(u => u.EmailAddress == emailObj.EmailAddress);
                if (validUser != null)
                {
                    var email = db.EmailTemplates.FirstOrDefault(t => t.Name == emailTemplateName);
                    if (!email.IsActive)
                    {
                        ModelState.AddModelError("CustomError", emailTemplateDisplayName + " is not active.");
                    }
                    else
                    {
                        email.To = validUser.EmailAddress;
                        email.From = fromAddress;
                        String errorMessage = string.Empty;
                        if (!EmailManager.SendMail(email, ref errorMessage))
                        {
                            ModelState.AddModelError("CustomError", errorMessage);
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    //RedirectToRoute("Login");//dont' give the hacker the notion that the account exists.
                    ModelState.AddModelError("CustomError", "Email address does not exist");
                }
            }
            return false;
        }

        //
        // GET: /ForgotPassword/

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /ForgotPassword/

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]

        public ActionResult ForgotPassword([Bind(Exclude="Pass")]EmailNPass emailObj)
        {
            if(SendPasswordEmail(emailObj, "ForgotPassword", "Forgot Password Email Template", ConfigurationManager.AppSettings["DefaultFrom"]))
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                return View();
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}