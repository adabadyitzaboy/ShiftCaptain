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
    public class UserController : Controller
    {
        private ShiftCaptainEntities db = new ShiftCaptainEntities();

        public JsonResult List(int VersionId = 0)
        {
            var result = db.UserViews.Where(uv => uv.VersionId == VersionId).Select(uv => new { uv.NickName, uv.UserId, uv.MinHours, uv.MaxHours, uv.EmployeeId, uv.CurrentHours});
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private UserView GetUserView(int id = 0)
        {
            return db.UserViews.Where(uv => uv.UserId == id).FirstOrDefault();
        }
        //
        // GET: /User/

        public ActionResult Index()
        {
            return View(db.UserViews.ToList());
        }

        //
        // GET: /User/Details/5

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

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /User/Create

        [HttpPost]
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
                        Country = userview.Country
                    };
                    db.Addresses.Add(address);
                    db.SaveChanges();
                    user.AddressId = address.Id;
                }
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(userview);
        }

        //
        // GET: /User/Edit/5

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
                user.Locked = false;
                user.IsActive = true;
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
                return RedirectToAction("Index");
            }
            return View(userview);
        }

        //
        // GET: /User/Delete/5

        public ActionResult Delete(int id = 0)
        {
            UserView userview = GetUserView(id);
            if (userview == null)
            {
                return HttpNotFound();
            }
            return View(userview);
        }

        //
        // POST: /User/Delete/5

        [HttpPost, ActionName("Delete")]
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