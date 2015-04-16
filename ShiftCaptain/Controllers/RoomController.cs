using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShiftCaptain.Models;
using ShiftCaptain.Helpers;
using ShiftCaptain.Filters;

namespace ShiftCaptain.Controllers
{
    public class RoomController : BaseController
    {
        public RoomController()
        {
            ClassName = "room";
        }
        #region JsonResults
        public JsonResult List(int VersionId = 0, int BuildingId = 0)
        {
            var result = db.Rooms.Where(r => r.BuildingId == BuildingId && r.RoomInstances.Count(ri=>ri.VersionId == VersionId) > 0);
            return Json(EntitySelector.SelectRoom(result), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListHours(int VersionId = 0, int RoomId = 0)
        {
            var result = db.RoomHours.Where(rh => (rh.RoomInstance.RoomId == RoomId  || RoomId == 0) && rh.RoomInstance.VersionId == VersionId);
            return Json(EntitySelector.SelectRoomHours(result), JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region helpers
        private RoomView GetRoomView(int VersionId, int id = 0)
        {
            var view = db.RoomViews.Where(rv => rv.RoomId == id && rv.VersionId == VersionId).FirstOrDefault();
            if (view == null)
            {
                var room = db.Rooms.Where(r => r.Id == id).FirstOrDefault();
                if (room != null)
                {
                    return new RoomView
                    {
                        BuildingId = room.BuildingId,
                        BuildingName = room.Building.Name,
                        Name = room.Name,
                        PhoneNumber = room.PhoneNumber,
                        RoomId = id,
                        RoomNumber = room.RoomNumber
                    };
                }
            }
            return view;
        }

        private int CreateOrUpdateRoomHour(RoomHour roomhour)
        {
            if (roomhour.Id != 0)
            {
                var roomHours = db.RoomHours.Find(roomhour.Id);
                roomHours.StartTime = roomhour.StartTime;
                roomHours.Duration = roomhour.Duration;
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
                        VersionId = (int)roomview.VersionId
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
            SaveChange();
        }

        #endregion

        //
        // GET: /Room/
        [ShiftManagerAccess(Order = 0)]
        [VersionRequired(Order = 1)]
        [BuildingRequired(Order = 2)]
        public ActionResult Index(String VersionName)
        {
            var version = GetVersion(VersionName);
            AddVersionDropDown(version);
            var rooms = db.RoomViews.Where(rv => rv.VersionId == version.Id).OrderBy(o => o.Name);
            if (currentUser.IsManager)
            {
                return View(rooms.Union(db.RoomViews.Where(rv => rv.VersionId == null).OrderBy(o => o.Name)));
            }
            else
            {
                return View(rooms);
            }
        }

        //
        // GET: /Room/Details/5
        [ShiftManagerAccess]
        [VersionRequired]
        public ActionResult Details(String VersionName, int id = 0)
        {
            var version = GetVersion(VersionName);
            RoomView roomview = GetRoomView(version.Id, id);
            if (roomview == null)
            {
                return HttpNotFound();
            }
            AddVersionDropDown(version);
            return View(roomview);
        }

        //
        // GET: /Room/Create
        [ManagerAccess(Order = 0)]
        [VersionRequired(Order=1)]
        [VersionNotApproved(Order = 2)]
        [BuildingRequired(Order = 3)]
        public ActionResult Create(String VersionName)
        {
            var version = GetVersion(VersionName);
            AddVersionDropDown(version);
            ViewBag.BuildingId = new SelectList(db.Buildings, "Id", "Name");
            return View();
        }

        //
        // POST: /Room/Create

        [HttpPost]
        [ManagerAccess]
        [VersionRequired]
        [VersionNotApproved]
        [ValidateAntiForgeryToken]
        public ActionResult Create(String VersionName, RoomView roomview)
        {
            var version = GetVersion(VersionName);
            if (ModelState.IsValid)
            {
                var room = new Room { 
                    Name = roomview.Name,
                    RoomNumber = roomview.RoomNumber,
                    PhoneNumber = roomview.PhoneNumber,
                    BuildingId = roomview.BuildingId
                };
                db.Rooms.Add(room);
                SaveChange();
                roomview.RoomId = room.Id;
                roomview.VersionId = version.Id;
                CreateOrUpdateRoomHours(roomview);
                
                return RedirectToAction("Index");
            }
            ViewBag.BuildingId = new SelectList(db.Buildings, "Id", "Name", roomview.BuildingId);

            AddVersionDropDown(version);
            return View(roomview);
        }

        //
        // GET: /Room/Edit/5
        [ManagerAccess]
        [VersionRequired]
        [VersionNotApproved]
        public ActionResult Edit(String VersionName, int id = 0)
        {

            var version = GetVersion(VersionName);
            RoomView roomview = GetRoomView(version.Id, id);
            if (roomview == null)
            {
                return HttpNotFound();
            }
            ViewBag.BuildingId = new SelectList(db.Buildings, "Id", "Name", roomview.BuildingId);
            AddVersionDropDown(version);
            return View(roomview);
        }

        //
        // POST: /Room/Edit/5

        [HttpPost]
        [ManagerAccess]
        [VersionRequired]
        [ValidateAntiForgeryToken]
        [VersionNotApproved]
        public ActionResult Edit(String VersionName, RoomView roomview)
        {
            var version = GetVersion(VersionName);
            AddVersionDropDown(version);
            if (ModelState.IsValid)
            {
                var room = db.Rooms.Find(roomview.RoomId);
                room.Name = roomview.Name;
                room.BuildingId = roomview.BuildingId;
                room.RoomNumber = roomview.RoomNumber;
                room.PhoneNumber = roomview.PhoneNumber;
                if (!roomview.VersionId.HasValue)
                {
                    roomview.VersionId = version.Id;
                }
                CreateOrUpdateRoomHours(roomview);
                db.Entry(room).State = EntityState.Modified;
                SaveChange();
                return RedirectToAction("Index");
            }
            ViewBag.BuildingId = new SelectList(db.Buildings, "Id", "Name", roomview.BuildingId);
            AddVersionDropDown(version);
            return View(roomview);
        }

        //
        // GET: /Room/Delete/5
        [ManagerAccess]
        [VersionRequired]
        public ActionResult Delete(String VersionName, int id = 0)
        {
            var version = GetVersion(VersionName);
            var roomview = GetRoomView(version.Id, id);
            if (roomview == null)
            {
                return HttpNotFound();
            }
            String ErrorMessage;
            ViewBag.CanDelete = CanDeleteRoom(roomview.VersionId, roomview.RoomId, out ErrorMessage);
            if (!ViewBag.CanDelete)
            {
                ViewBag.CantDeleteReason = ErrorMessage;
            }
            AddVersionDropDown(version);
            return View(roomview);
        }
        private bool CanDeleteRoom(int? versionId, int roomId, out String ErrorMessage)
        {
            ErrorMessage = String.Empty;
            var version = db.Versions.FirstOrDefault(v => v.Id == versionId);
            if (versionId.HasValue && version != null && version.IsApproved)
            {
                ErrorMessage = "Cannot delete a room in an approved version.";
                return false;
            }
            var shifts = db.Shifts.Count(s => s.RoomId == roomId && (!versionId.HasValue || s.VersionId == versionId));
            if (shifts != 0)
            {
                ErrorMessage = "Cannot delete a room that contains shifts";
                return false;
            }
            return true;
        }
        //
        // POST: /Room/Delete/5

        [HttpPost, ActionName("Delete")]
        [ManagerAccess]
        [VersionRequired]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(String VersionName, int id)
        {
            String ErrorMessage;
            var version = GetVersion(VersionName);
            var roomview = GetRoomView(version.Id, id);
            if (roomview == null)
            {
                return HttpNotFound();
            } else if (!CanDeleteRoom(roomview.VersionId, roomview.RoomId, out ErrorMessage))
            {
                return RedirectToAction("Delete", new { id = id, VersionName = VersionName });
            }
        
            if (roomview.RoomInstanceId.HasValue)
            {
                foreach (var roomhour in db.RoomHours.Where(rh => rh.RoomInstanceId == roomview.RoomInstanceId))
                {
                    db.RoomHours.Remove(roomhour);
                }
                var roominstance = db.RoomInstances.Find(roomview.RoomInstanceId);
                db.RoomInstances.Remove(roominstance);
                SaveChange();
            }
            if (db.RoomInstances.Count(ri => ri.RoomId == roomview.RoomId) == 0)
            {
                var room = db.Rooms.Find(roomview.RoomId);
                db.Rooms.Remove(room);
                SaveChange();
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}