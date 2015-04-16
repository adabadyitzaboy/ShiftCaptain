using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShiftCaptain.Models;
using System.Net;
using System.Data.Objects.SqlClient;
using System.Data.Objects;
using ShiftCaptain.Helpers;
using ShiftCaptain.Filters;
using ShiftCaptain.Infrastructure;

namespace ShiftCaptain.Controllers
{
    public class ShiftController : BaseController
    {
        public ShiftController()
        {
            ClassName = "shift";
        }
        #region helpers
        private Decimal GetCurrentHours(int VersionId, int UserId)
        {
            return db.Shifts.Where(s => s.VersionId == VersionId && s.UserId == UserId).ToList().Sum(r => r.Duration);
        }

        private object IsValid(Shift shift)
        {
            var conflicts = db.ValidateShift(shift.Id, shift.UserId, shift.VersionId, shift.RoomId, shift.Day, shift.StartTime, shift.Duration);
            if (conflicts.Count() > 0)
            {

                return new { error = conflicts };
            }
            return null;
        }
        #endregion

        #region JsonResults
        //
        // GET: /Shift/Validate
        [ShiftManagerAccess]
        public JsonResult Validate(int VersionId = 0, int ShiftId = 0, int RoomId = 0, int UserId = 0, int Day = 0, string StartTime = "00:00", decimal Duration = 0)
        {
            var shift = new Shift
            {
                Id = ShiftId,
                VersionId = VersionId,
                RoomId = RoomId,
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
        // POST: /Shift/Create

        [HttpPost]
        [ShiftManagerAccess]
        public JsonResult Create(int VersionId = 0, int RoomId = 0, int UserId = 0, int Day = 0, string StartTime = "00:00", decimal Duration = 0)
        {
            var shift = new Shift
            {
                VersionId = VersionId,
                RoomId = RoomId,
                UserId = UserId,
                StartTime = TimeSpan.Parse(StartTime),
                Duration = Duration,
                Day = Day
            };
            var errors = db.GetValidationErrors();
            if (errors.Count() > 0)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(errors, JsonRequestBehavior.AllowGet);
            }
            db.Shifts.Add(shift);
            SaveChange();

            var shifts = db.Shifts.Where(s => s.Id == shift.Id);
            return Json(EntitySelector.SelectShift(shifts), JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /Shift/Update

        [HttpPost]
        [ShiftManagerAccess]
        public JsonResult Update(int ShiftId = 0, int RoomId = 0, int Day = 0, string StartTime = "00:00", decimal Duration = 0)
        {
            var shift = db.Shifts.Find(ShiftId);
            shift.RoomId = RoomId;
            shift.Day = Day;
            shift.StartTime = TimeSpan.Parse(StartTime);
            shift.Duration = Duration;

            db.Entry(shift).State = EntityState.Modified;
            SaveChange();

            var shifts = db.Shifts.Where(s => s.Id == shift.Id);
            return Json(EntitySelector.SelectShift(shifts), JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /List/

        public JsonResult List(int VersionId = 0, int RoomId = 0)
        {
            var result = db.Shifts.Where(s => s.VersionId == VersionId && s.RoomId == RoomId);
            return Json(EntitySelector.SelectShift(result), JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /List/

        public JsonResult ListPreferences(int VersionId = 0, int UserId= 0)
        {
            var result = db.ShiftPreferences.Where(s => s.VersionId == VersionId && s.UserId == UserId).Select(s => new { s.VersionId, ShiftId = s.Id, s.User.NickName, s.Day, s.StartTime, s.Duration, s.UserId, s.Preference.CanWork, s.Preference.Color });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //
        // GET: /Shift/
        [VersionRequired(Order=1)]
        [BuildingRequired(Order = 2)]
        [RoomRequired(Order = 3)]
        [UserRequired(Order = 4)]
        public ActionResult Index(String VersionName)
        {
            var version = GetVersion(VersionName);
            AddVersionDropDown(version);
            var buildings = db.Buildings.Where(b => b.Rooms.Count(r=>r.RoomInstances.Count(ri=>ri.VersionId == version.Id) > 0) > 0);
            if (buildings.Count() > 0)
            {
                var first = buildings.First();
                ViewBag.BuildingId = new SelectList(buildings, "Id", "Name", first.Id);
                var rooms = db.Rooms.Where(r => r.BuildingId == first.Id && r.RoomInstances.Count(ri => ri.VersionId == version.Id) > 0);
                ViewBag.RoomID = new SelectList(rooms, "Id", "Name", rooms.First().Id);
            }
            else
            {
                ViewBag.BuildingId = new SelectList(buildings, "Id", "Name");
                ViewBag.RoomID = new SelectList(new List<Room>(), "Id", "Name");
            }
            
            return View();
        }

        //
        // POST: /Shift/Delete/5

        [HttpPost, ActionName("Delete")]
        [ShiftManagerAccess]
        public ActionResult DeleteConfirmed(String VersionName, int id)
        {
            Shift shift = db.Shifts.Find(id);
            db.Shifts.Remove(shift);
            SaveChange();

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}