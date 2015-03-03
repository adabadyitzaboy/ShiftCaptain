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
    public class RoomController : BaseController
    {

        private ShiftCaptainEntities db = new ShiftCaptainEntities();

        public JsonResult List(int VersionId = 0, int BuildingId = 0)
        {
            var result = db.RoomViews.Where(rv => rv.VersionId == VersionId && rv.BuildingId == BuildingId);
            return Json(EntitySelector.SelectRoomInstance(result), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListHours(int VersionId = 0, int RoomInstanceId = 0)
        {
            var result = db.RoomHours.Where(rh => rh.RoomInstanceId == RoomInstanceId || (RoomInstanceId == 0 && rh.RoomInstance.VersionId == VersionId));
            return Json(EntitySelector.SelectRoomHours(result), JsonRequestBehavior.AllowGet);
        }

        private RoomView GetRoomView(int id = 0)
        {
            return db.RoomViews.Where(rv => rv.RoomInstanceId == id).FirstOrDefault();
        }

        //
        // GET: /Room/

        public ActionResult Index()
        {
            return View(db.RoomViews.ToList());
        }

        //
        // GET: /Room/Details/5

        public ActionResult Details(int id = 0)
        {
            RoomView roomview = GetRoomView(id);
            if (roomview == null)
            {
                return HttpNotFound();
            }
            return View(roomview);
        }

        //
        // GET: /Room/Create

        public ActionResult Create()
        {
            ViewBag.BuildingID = new SelectList(db.Buildings, "Id", "Name");
            return View();
        }

        private int CreateOrUpdateRoomHour(RoomHour roomhour)
        {
            if (roomhour.Id != 0)
            {
                var roomHours = db.RoomHours.Find(roomhour.Id);
                roomHours.StartTime = roomHours.StartTime;
                roomHours.Duration = roomHours.Duration;
                db.Entry(roomHours).State = EntityState.Modified;
                return roomHours.Id;
            }
            else
            {
                var roomHours = new RoomHour
                {
                    Day = roomhour.Day,
                    StartTime = roomhour.StartTime,
                    Duration = roomhour.Duration
                };
                db.RoomHours.Add(roomHours);
                return roomHours.Id;
            }
        }

        private void CreateOrUpdateRoomHours(RoomView roomview)
        {
            bool deleteRoomInstance = false;
            if (roomview.SundayStartTime.HasValue ||
            roomview.MondayStartTime.HasValue ||
            roomview.TuesdayStartTime.HasValue ||
            roomview.WednesdayStartTime.HasValue ||
            roomview.ThursdayStartTime.HasValue ||
            roomview.FridayStartTime.HasValue ||
            roomview.SaturdayStartTime.HasValue)
            {

                if (roomview.RoomInstanceId == null)
                {
                    var instance = new RoomInstance
                    {
                        RoomId = (int)roomview.RoomId,
                        VersionId = (int)ViewData["VersionId"]
                    };
                    db.RoomInstances.Add(instance);
                    roomview.RoomInstanceId = instance.Id;
                }
            }
            else if (roomview.RoomInstanceId != null && roomview.RoomInstanceId != 0)
            {
                deleteRoomInstance = true;
            }

            if (roomview.SundayStartTime.HasValue)
            {
                roomview.SundayInstanceId = CreateOrUpdateRoomHour(new RoomHour
                {
                    RoomInstanceId = (int)roomview.RoomInstanceId,
                    Id = roomview.SundayInstanceId.HasValue ? (int)roomview.SundayInstanceId : 0,
                    Day = 0,
                    StartTime = (TimeSpan)roomview.SundayStartTime,
                    Duration = (Decimal)roomview.SundayDuration
                });

            }
            else if (roomview.SundayInstanceId != null && roomview.SundayInstanceId != 0)
            {
                //need to delete sunday's hours
                var roomHours = db.RoomHours.Find(roomview.SundayInstanceId);
                db.RoomHours.Remove(roomHours);
            }

            if (roomview.MondayStartTime.HasValue)
            {
                roomview.MondayInstanceId = CreateOrUpdateRoomHour(new RoomHour
                {
                    RoomInstanceId = (int)roomview.RoomInstanceId,
                    Id = roomview.MondayInstanceId.HasValue ? (int)roomview.MondayInstanceId : 0,
                    Day = 1,
                    StartTime = (TimeSpan)roomview.MondayStartTime,
                    Duration = (Decimal)roomview.MondayDuration
                });

            }
            else if (roomview.MondayInstanceId != null && roomview.MondayInstanceId != 0)
            {
                //need to delete Monday's hours
                var roomHours = db.RoomHours.Find(roomview.MondayInstanceId);
                db.RoomHours.Remove(roomHours);
            }

            if (roomview.TuesdayStartTime.HasValue)
            {
                roomview.TuesdayInstanceId = CreateOrUpdateRoomHour(new RoomHour
                {
                    RoomInstanceId = (int)roomview.RoomInstanceId,
                    Id = roomview.TuesdayInstanceId.HasValue ? (int)roomview.TuesdayInstanceId : 0,
                    Day = 2,
                    StartTime = (TimeSpan)roomview.TuesdayStartTime,
                    Duration = (Decimal)roomview.TuesdayDuration
                });

            }
            else if (roomview.TuesdayInstanceId != null && roomview.TuesdayInstanceId != 0)
            {
                //need to delete Tuesday's hours
                var roomHours = db.RoomHours.Find(roomview.TuesdayInstanceId);
                db.RoomHours.Remove(roomHours);
            }

            if (roomview.WednesdayStartTime.HasValue)
            {
                roomview.WednesdayInstanceId = CreateOrUpdateRoomHour(new RoomHour
                {
                    RoomInstanceId = (int)roomview.RoomInstanceId,
                    Id = roomview.WednesdayInstanceId.HasValue ? (int)roomview.WednesdayInstanceId : 0,
                    Day = 3,
                    StartTime = (TimeSpan)roomview.WednesdayStartTime,
                    Duration = (Decimal)roomview.WednesdayDuration
                });

            }
            else if (roomview.WednesdayInstanceId != null && roomview.WednesdayInstanceId != 0)
            {
                //need to delete Wednesday's hours
                var roomHours = db.RoomHours.Find(roomview.WednesdayInstanceId);
                db.RoomHours.Remove(roomHours);
            }

            if (roomview.ThursdayStartTime.HasValue)
            {
                roomview.ThursdayInstanceId = CreateOrUpdateRoomHour(new RoomHour
                {
                    RoomInstanceId = (int)roomview.RoomInstanceId,
                    Id = roomview.ThursdayInstanceId.HasValue ? (int)roomview.ThursdayInstanceId : 0,
                    Day = 4,
                    StartTime = (TimeSpan)roomview.ThursdayStartTime,
                    Duration = (Decimal)roomview.ThursdayDuration
                });

            }
            else if (roomview.ThursdayInstanceId != null && roomview.ThursdayInstanceId != 0)
            {
                //need to delete Thursday's hours
                var roomHours = db.RoomHours.Find(roomview.ThursdayInstanceId);
                db.RoomHours.Remove(roomHours);
            }

            if (roomview.FridayStartTime.HasValue)
            {
                roomview.FridayInstanceId = CreateOrUpdateRoomHour(new RoomHour
                {
                    RoomInstanceId = (int)roomview.RoomInstanceId,
                    Id = roomview.FridayInstanceId.HasValue ? (int)roomview.FridayInstanceId : 0,
                    Day = 5,
                    StartTime = (TimeSpan)roomview.FridayStartTime,
                    Duration = (Decimal)roomview.FridayDuration
                });

            }
            else if (roomview.FridayInstanceId != null && roomview.FridayInstanceId != 0)
            {
                //need to delete Friday's hours
                var roomHours = db.RoomHours.Find(roomview.FridayInstanceId);
                db.RoomHours.Remove(roomHours);
            }

            if (roomview.SaturdayStartTime.HasValue)
            {
                roomview.SaturdayInstanceId = CreateOrUpdateRoomHour(new RoomHour
                {
                    RoomInstanceId = (int)roomview.RoomInstanceId,
                    Id = roomview.SaturdayInstanceId.HasValue ? (int)roomview.SaturdayInstanceId : 0,
                    Day = 6,
                    StartTime = (TimeSpan)roomview.SaturdayStartTime,
                    Duration = (Decimal)roomview.SaturdayDuration
                });

            }
            else if (roomview.SaturdayInstanceId != null && roomview.SaturdayInstanceId != 0)
            {
                //need to delete Saturday's hours
                var roomHours = db.RoomHours.Find(roomview.SaturdayInstanceId);
                db.RoomHours.Remove(roomHours);
            }
            if (deleteRoomInstance)
            {
                var roomInstance = db.RoomInstances.Find(roomview.RoomInstanceId);
                db.RoomInstances.Remove(roomInstance);
            }
            db.SaveChanges();
        }

        //
        // POST: /Room/Create

        [HttpPost]
        public ActionResult Create(RoomView roomview)
        {
            if (ModelState.IsValid)
            {
                var room = new Room { 
                    Name = roomview.Name,
                    RoomNumber = roomview.RoomNumber,
                    PhoneNumber = roomview.PhoneNumber,
                    BuildingId = roomview.BuildingId
                };
                db.Rooms.Add(room);
                db.SaveChanges();
                roomview.RoomId = room.Id;
                CreateOrUpdateRoomHours(roomview);
                
                return RedirectToAction("Index");
            }
            ViewBag.BuildingID = new SelectList(db.Buildings, "Id", "Name", roomview.BuildingId);

            return View(roomview);
        }

        //
        // GET: /Room/Edit/5

        public ActionResult Edit(int id = 0)
        {
            RoomView roomview = GetRoomView(id);
            if (roomview == null)
            {
                return HttpNotFound();
            }
            ViewBag.BuildingID = new SelectList(db.Buildings, "Id", "Name", roomview.BuildingId);
            return View(roomview);
        }

        //
        // POST: /Room/Edit/5

        [HttpPost]
        public ActionResult Edit(RoomView roomview)
        {
            if (ModelState.IsValid)
            {
                var room = db.Rooms.Find(roomview.RoomId);
                room.Name = roomview.Name;
                room.BuildingId = roomview.BuildingId;
                room.RoomNumber = roomview.RoomNumber;
                room.PhoneNumber = roomview.PhoneNumber;

                CreateOrUpdateRoomHours(roomview);
                db.Entry(room).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BuildingID = new SelectList(db.Buildings, "Id", "Name", roomview.BuildingId);
            return View(roomview);
        }

        //
        // GET: /Room/Delete/5

        public ActionResult Delete(int id = 0)
        {
            RoomView roomview = GetRoomView(id);
            if (roomview == null)
            {
                return HttpNotFound();
            }
            return View(roomview);
        }

        //
        // POST: /Room/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            RoomView roomview = GetRoomView(id);
            var roomHours = db.RoomHours.Where(rh => rh.RoomInstanceId == roomview.RoomInstanceId);
            foreach (var roomhour in roomHours)
            {
                db.RoomHours.Remove(roomhour);
            }
            var roominstance = db.RoomInstances.Find(roomview.RoomInstanceId);
            db.RoomInstances.Remove(roominstance);
            var room = db.Rooms.Find(roomview.RoomId);
            db.Rooms.Remove(room);
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