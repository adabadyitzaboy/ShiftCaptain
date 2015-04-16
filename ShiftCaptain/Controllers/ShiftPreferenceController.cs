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
        public ShiftPreferenceController()
        {
            ClassName = "shiftpreference";
        }

        private object IsValid(ShiftPreference shiftPreference)
        {
            var conflicts = db.ValidateShiftPreference(shiftPreference.Id, shiftPreference.UserId, shiftPreference.VersionId, shiftPreference.PreferenceId, shiftPreference.Day, shiftPreference.StartTime, shiftPreference.Duration);
            if (conflicts.Count() > 0)
            {

                return new { error = conflicts };
            }
            return null;
        }
        #region JsonResults
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
            SaveChange();

            var shifts = db.ShiftPreferences.Where(s => s.Id == shiftPreference.Id);
            return Json(EntitySelector.SelectShiftPreference(shifts), JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /ShiftPreference/Update

        [HttpPost]
        [ShiftManagerAccess]
        public JsonResult Update(int ShiftPreferenceId = 0, int Day = 0, string StartTime = "00:00", decimal Duration = 0, int PreferenceId = 0)
        {
            var shiftPreference = db.ShiftPreferences.Find(ShiftPreferenceId);
            shiftPreference.Day = Day;
            shiftPreference.StartTime = TimeSpan.Parse(StartTime);
            shiftPreference.Duration = Duration;

            db.Entry(shiftPreference).State = EntityState.Modified;
            SaveChange();

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
        public ActionResult DeleteConfirmed(int id)
        {
            var shiftPreference = db.ShiftPreferences.Find(id);
            db.ShiftPreferences.Remove(shiftPreference);
            SaveChange();

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        //
        // GET: /ShiftPreference/

        [VersionRequired(Order = 1)]
        [BuildingRequired(Order = 2)]
        [RoomRequired(Order = 3)]
        [UserRequired(Order = 4)]
        [PreferenceRequired(Order=5)]
        public ActionResult Index(String VersionName)
        {
            var version = GetVersion(VersionName);
            AddVersionDropDown(version);
            
            if (SessionManager.IsShiftManager || SessionManager.IsManager)
            {
                ViewData["UserId"] = new SelectList(db.UserInstances.Where(ui => ui.VersionId == version.Id), "UserId", "User.NickName");
            }
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}