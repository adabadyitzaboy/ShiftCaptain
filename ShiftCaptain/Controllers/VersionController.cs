using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShiftCaptain.Models;
using ShiftCaptain.Filters;
using ShiftCaptain.Infrastructure;

namespace ShiftCaptain.Controllers
{
    public class VersionController : BaseController
    {
        public VersionController()
        {
            ClassName = "version";
        }
        private ShiftCaptainEntities db = new ShiftCaptainEntities();

        //
        // GET: /Version/
        [ShiftManagerAccess]
        public ActionResult Index()
        {
            return View(db.Versions.ToList());
        }

        //
        // GET: /Version/Details/5
        [ShiftManagerAccess]
        public ActionResult Details(int id = 0)
        {
            var version = db.Versions.Find(id);
            if (version == null)
            {
                return HttpNotFound();
            }
            return View(version);
        }

        //
        // GET: /Version/Create
        [ManagerAccess]
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Version/Create

        [HttpPost]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ShiftCaptain.Models.Version version)
        {
            if (ModelState.IsValid)
            {
                if (version.IsActive)
                {
                    var activeVersion = db.Versions.FirstOrDefault(v => v.IsActive);
                    if (activeVersion != null)
                    {
                        activeVersion.IsActive = false;
                        db.Entry(activeVersion).State = EntityState.Modified;
                    }
                }
                db.Versions.Add(version); 
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(version);
        }

        //
        // GET: /Version/Edit/5
        [ManagerAccess]
        public ActionResult Edit(int id = 0)
        {
            var version = db.Versions.Find(id);
            if (version == null)
            {
                return HttpNotFound();
            }
            return View(version);
        }

        //
        // POST: /Version/Edit/5

        [HttpPost]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ShiftCaptain.Models.Version version)
        {
            if (ModelState.IsValid)
            {
                db.Entry(version).State = EntityState.Modified;
                if (version.IsActive)
                {
                    var activeVersion = db.Versions.FirstOrDefault(v => v.IsActive);
                    if (activeVersion != null && activeVersion != version)
                    {
                        activeVersion.IsActive = false;
                        db.Entry(activeVersion).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(version);
        }

        //
        // GET: /Version/Delete/5
        [ManagerAccess]
        public ActionResult Delete(int id = 0)
        {
            var version = db.Versions.Find(id);
            if (version == null)
            {
                return HttpNotFound();
            }
            return View(version);
        }

        //
        // POST: /Version/Delete/5

        [HttpPost, ActionName("Delete")]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var version = db.Versions.Find(id);
            db.Versions.Remove(version);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //
        // POST: /Version/Create

        [HttpPost]
        public ActionResult ChangeVersion(string selected)
        {
            int newId = 0;
            if(!String.IsNullOrEmpty(Request.Params["VersionId"]) && Int32.TryParse(Request.Params["VersionId"], out newId)){
                SessionManager.VersionId = newId;
            }
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}