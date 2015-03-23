using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShiftCaptain.Models;
using System.Net;
using ShiftCaptain.Helpers;
using System.Data.Objects.SqlClient;
using ShiftCaptain.Filters;
using ShiftCaptain.Infrastructure;

namespace ShiftCaptain.Controllers
{
    public class ShiftPreferenceController : BaseController
    {
        private ShiftCaptainEntities db = new ShiftCaptainEntities();
        public ShiftPreferenceController()
        {
            ClassName = "shiftpreference";
            if (SessionManager.IsShiftManager || SessionManager.IsManager)
            {
                var versionId = GetVersionId();
                ViewData["UserId"] = new SelectList(db.UserInstances.Where(ui => ui.VersionId == CurrentVersionId), "UserId", "User.NickName");
            }
            AddVersionDropDown();
        }

        private object IsValid(ShiftPreference shiftPreference)
        {
            var endTime = shiftPreference.StartTime.Add(TimeSpan.FromHours((double)shiftPreference.Duration));
            //check for unable to work preference.
            var cantWorkShifts = db.ShiftPreferences.Where(s => s.VersionId == shiftPreference.VersionId && s.UserId == shiftPreference.UserId && s.Id != shiftPreference.Id && s.Day == shiftPreference.Day && 
                (
                    s.StartTime == shiftPreference.StartTime ||
                    (s.StartTime > shiftPreference.StartTime && s.StartTime < endTime) ||//new shiftPreference ends before old shiftPreference starts
                    (s.StartTime < shiftPreference.StartTime && SqlFunctions.DateAdd("minute", (double)s.Duration, s.StartTime) > shiftPreference.StartTime)//new shiftPreference starts during old shiftPreference   
                )
                );

            if (cantWorkShifts.Count() > 0)
            {
                return new { error = "A shift preference already exists during this time.", shiftPreference = EntitySelector.SelectShiftPreference(cantWorkShifts) };
            }

            return null;
        }

        //
        // GET: /ShiftPreference/Validate
        [ShiftManagerAccess]
        public JsonResult Validate(int VersionId = 0, int ShiftPreferenceId = 0, int PreferenceId = 0, int UserId = 0, int Day = 0, string StartTime = "00:00", decimal Duration = 0)
        {
            var shift = new ShiftPreference
            {
                Id = ShiftPreferenceId,
                PreferenceId = PreferenceId,
                VersionId = VersionId,
                UserId = UserId,
                StartTime = TimeSpan.Parse(StartTime),
                Duration = Duration,
                Day = Day
            };
            object json = IsValid(shift);
            if (json != null)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        //
        // POST: /ShiftPreference/Create
        [HttpPost]
        [ShiftManagerAccess]
        [ValidateAntiForgeryToken]
        public JsonResult Create(int VersionId = 0, int UserId = 0, int Day = 0, string StartTime = "00:00", decimal Duration = 0, int PreferenceId = 0)
        {
            var shiftPreference = new ShiftPreference
            {
                VersionId = VersionId,
                UserId = UserId,
                StartTime = TimeSpan.Parse(StartTime),
                Duration = Duration,
                Day = Day,
                PreferenceId = PreferenceId
            };
            var errors = db.GetValidationErrors();
            if (errors.Count() > 0)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(errors, JsonRequestBehavior.AllowGet);
            }
            db.ShiftPreferences.Add(shiftPreference);
            db.SaveChanges();

            var shifts = db.ShiftPreferences.Where(s => s.Id == shiftPreference.Id);
            return Json(EntitySelector.SelectShiftPreference(shifts), JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /ShiftPreference/Update

        [HttpPost]
        [ShiftManagerAccess]
        [ValidateAntiForgeryToken]
        public JsonResult Update(int ShiftPreferenceId = 0, int Day = 0, string StartTime = "00:00", decimal Duration = 0, int PreferenceId = 0)
        {
            var shiftPreference = db.ShiftPreferences.Find(ShiftPreferenceId);
            shiftPreference.Day = Day;
            shiftPreference.StartTime = TimeSpan.Parse(StartTime);
            shiftPreference.Duration = Duration;
            shiftPreference.PreferenceId = PreferenceId;

            db.Entry(shiftPreference).State = EntityState.Modified;
            db.SaveChanges();

            var shifts = db.ShiftPreferences.Where(s => s.Id == shiftPreference.Id);
            return Json(EntitySelector.SelectShiftPreference(shifts), JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /List/
        public JsonResult List(int VersionId = 0, int UserId = 0)
        {
            var shifts = db.ShiftPreferences.Where(s => s.VersionId == VersionId && (UserId == 0 || s.UserId == UserId));
            return Json(EntitySelector.SelectShiftPreference(shifts), JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /ShiftPreference/Delete/5

        [HttpPost, ActionName("Delete")]
        [ShiftManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var shiftPreference = db.ShiftPreferences.Find(id);
            db.ShiftPreferences.Remove(shiftPreference);
            db.SaveChanges();

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /ShiftPreference/

        public ActionResult Index()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}