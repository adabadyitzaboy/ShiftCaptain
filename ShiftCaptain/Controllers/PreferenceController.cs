using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShiftCaptain.Models;
using ShiftCaptain.Helpers;

namespace ShiftCaptain.Controllers
{
    public class PreferenceController : BaseController
    {
        public PreferenceController()
        {
            ClassName = "preference";
        }
        private ShiftCaptainEntities db = new ShiftCaptainEntities();

        //
        // GET: /Preference/List

        public JsonResult List(int PreferenceId = 0)
        {
            return Json(EntitySelector.SelectPreference(db.Preferences.Where(p=> PreferenceId == 0 || PreferenceId == p.Id)), JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Preference/

        public ActionResult Index()
        {
            return View(db.Preferences.ToList());
        }

        //
        // GET: /Preference/Details/5

        public ActionResult Details(int id = 0)
        {
            Preference preference = db.Preferences.Find(id);
            if (preference == null)
            {
                return HttpNotFound();
            }
            return View(preference);
        }

        //
        // GET: /Preference/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Preference/Create

        [HttpPost]
        public ActionResult Create(Preference preference)
        {
            if (ModelState.IsValid)
            {
                db.Preferences.Add(preference);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(preference);
        }

        //
        // GET: /Preference/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Preference preference = db.Preferences.Find(id);
            if (preference == null)
            {
                return HttpNotFound();
            }
            return View(preference);
        }

        //
        // POST: /Preference/Edit/5

        [HttpPost]
        public ActionResult Edit(Preference preference)
        {
            if (ModelState.IsValid)
            {
                db.Entry(preference).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(preference);
        }

        //
        // GET: /Preference/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Preference preference = db.Preferences.Find(id);
            if (preference == null)
            {
                return HttpNotFound();
            }
            return View(preference);
        }

        //
        // POST: /Preference/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Preference preference = db.Preferences.Find(id);
            db.Preferences.Remove(preference);
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