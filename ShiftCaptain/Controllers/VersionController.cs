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
    public class VersionController : BaseController
    {
        private ShiftCaptainEntities db = new ShiftCaptainEntities();

        //
        // GET: /Version/

        public ActionResult Index()
        {
            return View(db.Versions.ToList());
        }

        //
        // GET: /Version/Details/5

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

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Version/Create

        [HttpPost]
        public ActionResult Create(ShiftCaptain.Models.Version version)
        {
            if (ModelState.IsValid)
            {
                db.Versions.Add(version);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(version);
        }

        //
        // GET: /Version/Edit/5

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
        public ActionResult Edit(ShiftCaptain.Models.Version version)
        {
            if (ModelState.IsValid)
            {
                db.Entry(version).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(version);
        }

        //
        // GET: /Version/Delete/5

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
        public ActionResult DeleteConfirmed(int id)
        {
            var version = db.Versions.Find(id);
            db.Versions.Remove(version);
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