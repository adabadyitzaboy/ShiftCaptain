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
                var user = new User
                {
                    EmailAddress = userview.EmailAddress,
                    FName = userview.FName,
                    MName = userview.MName,
                    LName = userview.LName,
                    NickName = userview.NickName,
                    EmployeeId = userview.EmployeeId,
                    PhoneNumber = userview.PhoneNumber,
                    IsShiftManager = userview.IsShiftManager,
                    IsManager = userview.IsManager,
                    Locked = false,
                    IsActive = true,
                    IsMale = userview.IsMale,
                    Pass = "12345"
                };

                if (userview.Line1 != null)
                {
                    var address = new Address
                    {
                        Line1 = userview.Line1,
                        Line2 = userview.Line2,
                        City = userview.City,
                        State = userview.State,
                        ZipCode = userview.ZipCode,
                        Country = userview.Country
                    };
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
                user.EmailAddress = userview.EmailAddress;
                user.FName = userview.FName;
                user.MName = userview.MName;
                user.LName = userview.LName;
                user.NickName = userview.NickName;
                user.EmployeeId = userview.EmployeeId;
                user.PhoneNumber = userview.PhoneNumber;
                user.IsShiftManager = userview.IsShiftManager;
                user.IsManager = userview.IsManager;
                if (user.Locked && !userview.Locked)
                {
                    user.NumTries = 0;
                }
                user.Locked = userview.Locked;
                user.IsActive = userview.IsActive;
                user.IsMale = userview.IsMale;

                
                if (userview.Line1 != null)
                {
                    Address address = userview.AddressId != null ? db.Addresses.Find(userview.AddressId) : new Address();
                    address.Line1 = userview.Line1;
                    address.Line2 = userview.Line2;
                    address.City = userview.City;
                    address.State = userview.State;
                    address.Country = userview.Country;
                    address.ZipCode = userview.ZipCode;

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
                    var userI = db.UserInstances.FirstOrDefault(ui => ui.UserId == user.Id && ui.VersionId == versionId);
                    if (userI != null)
                    {
                        if (userview.MinHours.HasValue)
                        {
                            userI.MinHours = userview.MinHours.HasValue ? (decimal)userview.MinHours : 0;
                            userI.MaxHours = userview.MaxHours.HasValue ? (decimal)userview.MaxHours : 0;

                            db.Entry(userI).State = EntityState.Modified;
                        }
                        else
                        {
                            db.UserInstances.Remove(userI);
                        }
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
                    var user = db.Users.Find(userview.UserId);
                    
                    user.FName = userview.FName;
                    user.MName = userview.MName;
                    user.LName = userview.LName;
                    user.NickName = userview.NickName;
                    if (user.IsManager)
                    {
                        user.EmailAddress = userview.EmailAddress;
                        user.EmployeeId = userview.EmployeeId;
                    }
                    user.PhoneNumber = userview.PhoneNumber;
                    user.IsMale = userview.IsMale;


                    if (userview.Line1 != null)
                    {
                        Address address = userview.AddressId != null ? db.Addresses.Find(userview.AddressId) : new Address();
                        address.Line1 = userview.Line1;
                        address.Line2 = userview.Line2;
                        address.City = userview.City;
                        address.State = userview.State;
                        address.Country = userview.Country;

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
                            var userI = db.UserInstances.FirstOrDefault(ui => ui.UserId == user.Id && ui.VersionId == versionId);
                            if (userview.MinHours.HasValue)
                            {
                                userI.MinHours = userview.MinHours.HasValue ? (decimal)userview.MinHours : 0;
                                userI.MaxHours = userview.MaxHours.HasValue ? (decimal)userview.MaxHours : 0;

                                db.Entry(userI).State = EntityState.Modified;
                            }
                            else
                            {
                                db.UserInstances.Remove(userI);
                            }
                            db.SaveChanges();
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
            var canDelete = true;
            var version = db.Versions.FirstOrDefault(v => v.Id == userview.VersionId);
            if (userview.VersionId.HasValue && version != null && version.IsApproved)
            {
                
                canDelete = false;
                ViewBag.CantDeleteReason = "Cannot delete a user in an approved version.";
            }
            else
            {
                var shifts = db.Shifts.Count(s => s.UserId == userview.UserId);
                if (shifts != 0)
                {
                    canDelete = false;
                    ViewBag.CantDeleteReason = "Cannot delete a user who has shifts";
                }
            }
            ViewBag.CanDelete = canDelete;
            return View(userview);
        }

        //
        // POST: /User/Delete/5

        [HttpPost, ActionName("Delete")]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = db.Users.Find(id);
            if (user.AddressId != null)
            {
                var address = db.Addresses.Find(user.AddressId);
                db.Addresses.Remove(address);
                db.SaveChanges();
            }
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