using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShiftCaptain.Models;
using ShiftCaptain.Helpers;
using ShiftCaptain.Filters;
using ShiftCaptain.Infrastructure;

namespace ShiftCaptain.Controllers
{
    public class UserController : BaseController
    {
        public UserController()
        {
            ClassName = "user";
        }
        private ShiftCaptainEntities db = new ShiftCaptainEntities();

        public JsonResult List(int VersionId = 0, int UserId = 0)
        {
            var result = db.UserViews.Where(uv => uv.VersionId == VersionId && (UserId == 0 || uv.UserId == UserId) && (currentUser.IsManager || currentUser.IsShiftManager || uv.UserId == currentUser.Id));
            return Json(EntitySelector.SelectUserInstance(result), JsonRequestBehavior.AllowGet);            
        }

        private UserView GetUserView(int id = 0)
        {
            int VersionId = GetVersionId();
            var userview = db.UserViews.Where(uv => uv.VersionId == VersionId && uv.UserId == id).FirstOrDefault();
            if (userview == null)
            {
                return db.UserViews.Where(uv => uv.UserId == id).FirstOrDefault();
            }
            return userview;
        }
        //
        // GET: /User/
        [ShiftManagerAccess]
        public ActionResult Index()
        {
            var userviews = db.UserViews.Where(uv => uv.VersionId == CurrentVersionId || uv.VersionId == null).OrderByDescending(o=>o.VersionId).ThenBy(o=>o.EmailAddress);
            AddVersionDropDown();
            return View(userviews);
        }

        //
        // GET: /User/Details/5
        [ShiftManagerAccess]
        public ActionResult Details(int id = 0)
        {
            UserView userview = GetUserView(id);
            if (userview == null)
            {
                return HttpNotFound();
            }
            return View(userview);
        }

        //
        // GET: /User/Create
        [ManagerAccess]
        [VersionNotApproved]
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /User/Create

        [HttpPost]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        [VersionNotApproved]
        public ActionResult Create(UserView userview)
        {
            if (ModelState.IsValid)
            {
                var user = ModelConverter.GetUser(userview);
                user.Locked = false;
                user.IsActive = true;
                user.Pass = "12345";

                if (userview.Line1 != null)
                {
                    var address = ModelConverter.GetAddress(userview, db);
                    db.Addresses.Add(address);
                    db.SaveChanges();
                    user.AddressId = address.Id;
                }
                db.Users.Add(user);
                db.SaveChanges();
                if (userview.MinHours.HasValue)
                {
                    var userI = new UserInstance
                    {
                        MinHours = userview.MinHours.Value,
                        MaxHours = userview.MaxHours.Value
                    };

                    db.UserInstances.Add(userI);
                    db.SaveChanges();
                }
                if(!SendNewUserEmail(user)){
                    return View(userview);
                }
                return RedirectToAction("Index");
            }

            return View(userview);
        }

        private bool SendNewUserEmail(User user)
        {
            var email = db.EmailTemplates.FirstOrDefault(t => t.Name == "NewUserEmail");
            if (!email.IsActive)
            {
                ModelState.AddModelError("CustomError", "USER CREATED.  Forgot Password Email Template is not active.");
                return false;
            }
            else
            {
                email.To = user.EmailAddress;
                email.From = currentUser.EmailAddress;
                String errorMessage = string.Empty;
                if (!EmailManager.SendMail(email, ref errorMessage))
                {
                    ModelState.AddModelError("CustomError", "USER CREATED.  " + errorMessage);
                    return false;
                }
            }
            return true;
        }
        //
        // GET: /User/Edit/5
        [ManagerAccess]
        [NoSelfAccess]
        public ActionResult Edit(int id = 0)
        {
            UserView userview = GetUserView(id);
            if (userview == null)
            {
                return HttpNotFound();
            }
            return View(userview);
        }

        //
        // POST: /User/Edit/5

        [HttpPost]
        [ManagerAccess]
        [NoSelfAccess]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserView userview)
        {
            if (ModelState.IsValid)
            {
                
                var user = db.Users.Find(userview.UserId);
                ModelConverter.UpdateUser(user, userview);
                
                if (user.Locked && !userview.Locked)
                {
                    user.NumTries = 0;
                }

                
                if (userview.Line1 != null)
                {
                    var address = ModelConverter.GetAddress(userview, db);

                    if (userview.AddressId == null)
                    {
                        db.Addresses.Add(address);
                        db.SaveChanges();
                        user.AddressId = address.Id;
                    }
                    else
                    {
                        db.Entry(address).State = EntityState.Modified;
                        db.SaveChanges();
                    }                    
                }
                else
                {
                    if (userview.AddressId != null)
                    {
                        db.Addresses.Remove(db.Addresses.Find(userview.AddressId));
                        user.AddressId = null;
                    }
                }
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                var versionId = GetVersionId();
                var version = db.Versions.FirstOrDefault(v => v.Id == versionId);
                if (!version.IsApproved)
                {
                    var userI = ModelConverter.GetUserInstance(userview, db);
                    if (userI != null)
                    {
                        if (userview.MinHours.HasValue)
                        {
                            db.Entry(userI).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            db.UserInstances.Remove(userI);
                            db.SaveChanges();
                        }
                    }
                    else if (userview.MinHours.HasValue)
                    {
                        db.UserInstances.Add(userI);
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("Index");
            }
            return View(userview);
        }


        //
        // GET: /Profile/
        [SelfAccess]
        public ActionResult EditProfile()
        {
            UserView userview = GetUserView(SessionManager.UserId);
            if (userview == null)
            {
                return HttpNotFound();
            }
            return View(userview);
        }

        //
        // POST: /Profile/5

        [HttpPost]
        [SelfAccess]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile([Bind(Exclude = "IsActive")]UserView userview)
        {
            if (ModelState.IsValid)
            {
                if (SessionManager.UserId == userview.UserId)
                {
                    var dbUser = db.Users.Find(userview.UserId);
                    var user = ModelConverter.GetUser(dbUser, userview);
                    user.Pass = dbUser.Pass;
                    if (!user.IsManager)
                    {
                        user.EmailAddress = dbUser.EmailAddress;
                        user.EmployeeId = dbUser.EmployeeId;
                    }

                    if (userview.Line1 != null)
                    {
                        var address = ModelConverter.GetAddress(userview, db);

                        if (userview.AddressId != null)
                        {
                            db.Addresses.Add(address);
                            db.SaveChanges();
                            user.AddressId = address.Id;
                        }
                        else
                        {
                            db.Entry(address).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        if (userview.AddressId != null)
                        {
                            db.Addresses.Remove(db.Addresses.Find(userview.AddressId));
                            user.AddressId = null;
                        }
                    }
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    if (user.IsManager)
                    {//only a manager can update the hours on the profile page
                        var versionId = GetVersionId();
                        var version = db.Versions.FirstOrDefault(v => v.Id == versionId);
                        if (!version.IsApproved)
                        {
                            var userI = ModelConverter.GetUserInstance(userview, db);
                            if (userview.MinHours.HasValue)
                            {
                                db.Entry(userI).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else if(userI != null)
                            {
                                db.UserInstances.Remove(userI);
                                db.SaveChanges();
                            }
                        }
                    }
                    return RedirectToAction("Index", "Shift");
                }
                else
                {
                    return RedirectToAction("Edit", userview.UserId);
                }
                
            }
            return View(userview);
        }

        //
        // GET: /User/Delete/5
        [ManagerAccess]
        public ActionResult Delete(int id = 0)
        {
            UserView userview = GetUserView(id);
            if (userview == null)
            {
                return HttpNotFound();
            }

            String ErrorMessage;
            ViewBag.CanDelete = CanDeleteUser(userview.UserId, userview.VersionId, out ErrorMessage);
            if (!ViewBag.CanDelete)
            {
                ViewBag.CantDeleteReason = ErrorMessage;
            }
            return View(userview);
        }
        private bool CanDeleteUser(int userId, int? versionId, out String ErrorMessage)
        {
            ErrorMessage = String.Empty;
            if (versionId.HasValue)
            {
                var version = db.Versions.FirstOrDefault(v => v.Id == versionId);
                if (version != null && version.IsApproved)
                {
                    ErrorMessage = "Cannot delete a user in an approved version.";
                    return false;
                }
            }
            var shifts = db.Shifts.Count(s => s.UserId == userId);
            if (shifts != 0)
            {
                ErrorMessage = "Cannot delete a user who has shifts";
                return false;
            }

            return true;
        }
        //
        // POST: /User/Delete/5

        [HttpPost, ActionName("Delete")]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserView userview = GetUserView(id);
            String ErrorMessage;
            if (userview == null)
            {
                return HttpNotFound();
            }
            else if (!CanDeleteUser(userview.UserId, userview.VersionId, out ErrorMessage))
            {
                return RedirectToAction("Delete", new { id = id });
            }
        
            var user = db.Users.Find(id);
            if (user.AddressId != null)
            {
                var address = db.Addresses.Find(user.AddressId);
                db.Addresses.Remove(address);
                db.SaveChanges();
            }
            foreach (var shiftPreference in db.ShiftPreferences.Where(sp => sp.UserId == user.Id))
            {
                db.ShiftPreferences.Remove(shiftPreference);
            }
            db.SaveChanges();
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}