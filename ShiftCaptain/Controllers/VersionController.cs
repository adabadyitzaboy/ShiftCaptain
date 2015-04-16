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
                SaveChange();
                return RedirectToAction("Index");
            }

            return View(version);
        }

        //
        // GET: /Version/Edit/5
        [ManagerAccess]
        [VersionNotApproved]
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
        [VersionNotApproved]
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
                SaveChange();
                return RedirectToAction("Index");
            }
            return View(version);
        }

        //
        // GET: /Version/Delete/5
        [ManagerAccess]
        [VersionNotApproved]
        public ActionResult Delete(int id = 0)
        {
            var version = db.Versions.Find(id);
            if (version == null)
            {
                return HttpNotFound();
            }

            String ErrorMessage;
            ViewBag.CanDelete = CanDeleteVersion(version.Id, out ErrorMessage);
            if (!ViewBag.CanDelete)
            {
                ViewBag.CantDeleteReason = ErrorMessage;
            }
            return View(version);
        }
        private bool CanDeleteVersion(int id, out String ErrorMessage)
        {
            ErrorMessage = String.Empty;
            if (db.Shifts.Count(s => s.VersionId == id) > 0)
            {
                ErrorMessage = "Cannot delete a version that contains shifts";
                return false;
            }
            else if (db.UserInstances.Count(ui => ui.VersionId == id) > 0)
            {
                ErrorMessage = "Cannot delete a version that contains users.";
                return false;
            }
            else if (db.RoomInstances.Count(ri => ri.VersionId == id) > 0)
            {
                ErrorMessage = "Cannot delete a version that contains rooms.";
                return false;

            }

            return true;
        }
        //
        // POST: /Version/Delete/5

        [HttpPost, ActionName("Delete")]
        [ManagerAccess]
        [VersionNotApproved]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            String ErrorMessage;
            if (!CanDeleteVersion(id, out ErrorMessage))
            {
                return RedirectToAction("Delete", new { id = id });
            }

            var version = db.Versions.Find(id);
            db.Versions.Remove(version);
            SaveChange();
            if (SessionManager.VersionId == id)
            {
                var currentVersion = db.Versions.OrderByDescending(v=>v.Id).OrderBy(v => v.IsActive).FirstOrDefault();
                if (currentVersion != null)
                {
                    SessionManager.VersionId = currentVersion.Id;
                }
                else
                {
                    SessionManager.VersionId = 0;
                }
            }
            return RedirectToAction("Index");
        }

        //
        // POST: /Version/Create

        [HttpPost]
        public ActionResult ChangeVersion(string VersionName)
        {
            int newId = 0;
            if(!String.IsNullOrEmpty(Request.Params["VersionId"]) && Int32.TryParse(Request.Params["VersionId"], out newId)){
                SessionManager.VersionId = newId;
            }else{
                return HttpNotFound();
            }
            var version = db.Versions.FirstOrDefault(v=> v.Id == newId);
            if(version == null){
                return HttpNotFound();
            }
            var baseUrl = String.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority);
            if (Request.UrlReferrer.AbsoluteUri == baseUrl || Request.UrlReferrer.AbsoluteUri == baseUrl + "/")
            {
                return Redirect(String.Format("{0}/{1}/Shift", baseUrl, version.EncodedName));
            }
            else if (VersionName != null)
            {
                var responseUrl = Request.UrlReferrer.AbsoluteUri.Replace(String.Format("/{0}/", version.EncodedName), String.Format("/{0}/", version.EncodedName));
                if (responseUrl != Request.UrlReferrer.AbsoluteUri)
                {
                    return Redirect(responseUrl);
                }
                responseUrl = Request.UrlReferrer.AbsoluteUri.Replace(String.Format("/{0}/", Uri.EscapeDataString(version.EncodedName)), String.Format("/{0}/", version.EncodedName));
                if (responseUrl != Request.UrlReferrer.AbsoluteUri)
                {
                    return Redirect(responseUrl);
                }
                return Redirect(String.Format("{0}/{1}/Shift", baseUrl, version.EncodedName));
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