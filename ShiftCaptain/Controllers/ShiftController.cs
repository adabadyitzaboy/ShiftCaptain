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

namespace ShiftCaptain.Controllers
{
    public class ShiftController : BaseController
    {
        private ShiftCaptainEntities db = new ShiftCaptainEntities();
        private Decimal GetCurrentHours(int VersionId, int UserId)
        {
            return db.Shifts.Where(s => s.VersionId == VersionId && s.UserId == UserId).ToList().Sum(r => r.Duration);
        }
        private object IsValid(Shift shift)
        {
            var userInstance = db.UserInstances.FirstOrDefault(uv => uv.UserId == shift.UserId && uv.VersionId == shift.VersionId);
            if (userInstance == null)
            {
                return new { error = "User does not exist in this version given"};
            }
            if (shift.Id == 0)
            {
                var maxHours = userInstance.MaxHours;
                var currentHours = GetCurrentHours(shift.VersionId, shift.UserId);
                if (maxHours < currentHours + shift.Duration)
                {
                    return new { error = "New shift would cause " + userInstance.User.NickName + " to have too many hours" };
                }
            }
            var endTime = shift.StartTime.Add(TimeSpan.FromHours((double)shift.Duration));
            //check for unable to work preference.
            var cantWorkShifts = db.ShiftPreferences.Where(s => s.VersionId == shift.VersionId && s.UserId == shift.UserId && s.Day == shift.Day && !s.Preference.CanWork && 
                (
                    s.StartTime == shift.StartTime ||
                    (s.StartTime > shift.StartTime && s.StartTime < endTime) ||//new shift ends before old shift starts
                    (s.StartTime < shift.StartTime && SqlFunctions.DateAdd("minute", (double) s.Duration, s.StartTime) > shift.StartTime)//new shift starts during old shift   
                )
                );

            if (cantWorkShifts.Count() > 0)
            {
                return new { error = userInstance.User.NickName + " cant work this shift.", shift = EntitySelector.SelectShiftPreference(cantWorkShifts) };
            }
            

            //check for shift during this time period
            var conflictingShifts = db.Shifts.Where(s => s.VersionId == shift.VersionId && s.UserId == shift.UserId && s.Id != shift.Id && s.Day == shift.Day &&
                (
                    s.StartTime == shift.StartTime ||
                    (s.StartTime > shift.StartTime && s.StartTime < endTime) ||//new shift ends before old shift starts
                    (s.StartTime < shift.StartTime && SqlFunctions.DateAdd("minute", (double) s.Duration, s.StartTime) > shift.StartTime)//new shift starts during old shift   
                )
                );
            
            if (conflictingShifts.Count() > 0)
            {
                return new { error = userInstance.User.NickName + " has a conflicting shift", shift = EntitySelector.SelectShift(conflictingShifts) };
            }
            return null;
        }
        //
        // GET: /Shift/Validate
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
            db.SaveChanges();

            var user = db.UserInstances.First(uv => uv.UserId == UserId && uv.VersionId == VersionId);
            user.CurrentHours = GetCurrentHours(VersionId, UserId);
            db.SaveChanges();

            var shifts = db.Shifts.Where(s => s.Id == shift.Id);
            return Json(EntitySelector.SelectShift(shifts), JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /Shift/Update

        [HttpPost]
        public JsonResult Update(int ShiftId = 0, int RoomId = 0, int Day = 0, string StartTime = "00:00", decimal Duration = 0)
        {
            var shift = db.Shifts.Find(ShiftId);
            shift.RoomId = RoomId;
            shift.Day = Day;
            shift.StartTime = TimeSpan.Parse(StartTime);
            shift.Duration = Duration;

            db.Entry(shift).State = EntityState.Modified;
            db.SaveChanges();

            var user = db.UserInstances.First(uv => uv.UserId == shift.UserId && uv.VersionId == shift.VersionId);
            user.CurrentHours = GetCurrentHours(shift.VersionId, shift.UserId);
            db.SaveChanges();

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

        //
        // GET: /Shift/

        public ActionResult Index()
        {
            var buildings = db.Buildings.Where(b => b.Rooms.Count() > 0);
            if (buildings.Count() > 0)
            {
                var first = buildings.First();
                ViewBag.BuildingID = new SelectList(buildings, "Id", "Name", first.Id);
                var rooms = db.Rooms.Where(r => r.BuildingId == first.Id);
                ViewBag.RoomID = new SelectList(rooms, "Id", "Name", rooms.First().Id);

            }
            
            return View();
        }

        //
        // POST: /Shift/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Shift shift = db.Shifts.Find(id);
            db.Shifts.Remove(shift);
            var user = db.UserInstances.First(uv => uv.UserId == shift.UserId && uv.VersionId == shift.VersionId);
            user.CurrentHours = GetCurrentHours(shift.VersionId, shift.UserId);
            db.Entry(user).State = EntityState.Modified;           
            db.SaveChanges();

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}