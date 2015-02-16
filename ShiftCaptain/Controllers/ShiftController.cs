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

namespace ShiftCaptain.Controllers
{
    public class ShiftController : Controller
    {
        private ShiftCaptainEntities db = new ShiftCaptainEntities();
        private TimeSpan GetCurrentHours(int VersionId, int UserId)
        {
            return TimeSpan.FromTicks(db.Shifts.Where(s => s.VersionId == VersionId && s.UserId == UserId).ToList().Sum(r => r.Duration.Ticks));
        }
        private object IsValid(Shift shift)
        {
            var userInstance = db.UserInstances.FirstOrDefault(uv => uv.Id == shift.UserId && uv.VersionId == shift.VersionId);
            if (userInstance == null)
            {
                return new { error = "User does not exist in this version given"};
            }
            if (shift.Id == null || shift.Id == 0)
            {
                var maxHours = userInstance.MaxHours;
                var currentHours = GetCurrentHours(shift.VersionId, shift.UserId);
                if (maxHours < currentHours.Add(shift.Duration))
                {
                    return new { error = "New shift would cause " + userInstance.User.NickName + " to have too many hours" };
                }
            }
            var endTime = shift.StartTime.Add(shift.Duration);
            //check for unable to work preference.
            //TODO
            var emptyTime = new DateTime();
            //check for shift during this time period
            var conflictingShifts = db.Shifts.Where(s => s.VersionId == shift.VersionId && s.UserId == shift.UserId && s.Id != shift.Id && s.Day == shift.Day &&
                (
                    s.StartTime == shift.StartTime ||
                    (s.StartTime > shift.StartTime && s.StartTime < endTime) ||//new shift ends before old shift starts
                    (s.StartTime < shift.StartTime && SqlFunctions.DateAdd("minute", SqlFunctions.DateDiff("minute", emptyTime, s.Duration), s.StartTime) > shift.StartTime)//new shift starts during old shift   
                )
                );
            
            if (conflictingShifts.Count() > 0)
            {
                return new { error = userInstance.User.NickName + " has a conflicting shift", shift = conflictingShifts.Select(s => new { s.VersionId, ShiftId = s.Id, s.User.NickName, s.RoomId, s.Day, s.StartTime, s.Duration, s.UserId }) };
            }
            return null;
        }
        //
        // GET: /Shift/Validate
        public JsonResult Validate(int VersionId = 0, int ShiftId = 0, int RoomId = 0, int UserId = 0, int Day = 0, string StartTime = "00:00", string Duration = "00:00")
        {
            var shift = new Shift
            {
                Id = ShiftId,
                VersionId = VersionId,
                RoomId = RoomId,
                UserId = UserId,
                StartTime = TimeSpan.Parse(StartTime),
                Duration = TimeSpan.Parse(Duration),
                Day = Day
            };
            object json = IsValid(shift);
            if (json != null)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /Shift/Create

        [HttpPost]
        public JsonResult Create(int VersionId = 0, int RoomId = 0, int UserId = 0, int Day = 0, string StartTime = "00:00", string Duration = "00:00")
        {
            var shift = new Shift
            {
                VersionId = VersionId,
                RoomId = RoomId,
                UserId = UserId,
                StartTime = TimeSpan.Parse(StartTime),
                Duration= TimeSpan.Parse(Duration),
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

            var user = db.UserInstances.First(uv => uv.Id == UserId && uv.VersionId == VersionId);
            user.CurrentHours = (decimal) GetCurrentHours(VersionId, UserId).TotalHours;
            db.SaveChanges();

            var shiftObj = db.Shifts.Where(s => s.Id == shift.Id).Select(s => new { s.VersionId, ShiftId = s.Id, s.User.NickName, s.RoomId, s.Day, s.StartTime, s.Duration, s.UserId });
            return Json(shiftObj, JsonRequestBehavior.AllowGet);
        }
        //
        // POST: /Shift/Update

        [HttpPost]
        public JsonResult Update(int ShiftId = 0, int RoomId = 0, int Day = 0, string StartTime = "00:00", string Duration = "00:00")
        {
            var shift = db.Shifts.Find(ShiftId);
            shift.RoomId = RoomId;
            shift.Day = Day;
            shift.StartTime = TimeSpan.Parse(StartTime);
            shift.Duration = TimeSpan.Parse(Duration);

            db.Entry(shift).State = EntityState.Modified;
            db.SaveChanges();

            var user = db.UserInstances.First(uv => uv.Id == shift.UserId && uv.VersionId == shift.VersionId);
            user.CurrentHours = (decimal)GetCurrentHours(shift.VersionId, shift.UserId).TotalHours;
            db.SaveChanges();

            var shiftObj = db.Shifts.Where(s => s.Id == shift.Id).Select(s => new { s.VersionId, ShiftId = s.Id, s.User.NickName, s.RoomId, s.Day, s.StartTime, s.Duration, s.UserId });
            return Json(shiftObj, JsonRequestBehavior.AllowGet);
        }
        public JsonResult List(int VersionId = 0, int RoomId = 0)
        {
            var result = db.Shifts.Where(s => s.VersionId == VersionId && s.RoomId == RoomId).Select(s => new { s.VersionId, ShiftId = s.Id, s.User.NickName, s.RoomId, s.Day, s.StartTime, s.Duration, s.UserId });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        //
        // GET: /Shift/

        public ActionResult Index()
        {
            ViewData["VersionId"] = 1;
            
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
        // GET: /Shift/Details/5

        public ActionResult Details(int id = 0)
        {
            Shift shift = db.Shifts.Find(id);
            if (shift == null)
            {
                return HttpNotFound();
            }
            return View(shift);
        }

        //
        // GET: /Shift/Create

        public ActionResult Create()
        {
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Name");
            ViewBag.UserId = new SelectList(db.Users, "Id", "EmailAddress");
            ViewBag.VersionId = new SelectList(db.Versions, "Id", "Name");
            return View();
        }

        //
        // GET: /Shift/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Shift shift = db.Shifts.Find(id);
            if (shift == null)
            {
                return HttpNotFound();
            }
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Name", shift.RoomId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "EmailAddress", shift.UserId);
            ViewBag.VersionId = new SelectList(db.Versions, "Id", "Name", shift.VersionId);
            return View(shift);
        }

        //
        // POST: /Shift/Edit/5

        [HttpPost]
        public ActionResult Edit(Shift shift)
        {
            if (ModelState.IsValid)
            {
                db.Entry(shift).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Name", shift.RoomId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "EmailAddress", shift.UserId);
            ViewBag.VersionId = new SelectList(db.Versions, "Id", "Name", shift.VersionId);
            return View(shift);
        }

        //
        // GET: /Shift/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Shift shift = db.Shifts.Find(id);
            if (shift == null)
            {
                return HttpNotFound();
            }
            return View(shift);
        }

        //
        // POST: /Shift/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Shift shift = db.Shifts.Find(id);
            db.Shifts.Remove(shift);
            var user = db.UserInstances.First(uv => uv.UserId == shift.UserId && uv.VersionId == shift.VersionId);
            user.CurrentHours += (decimal)shift.Duration.TotalMinutes / 60;
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