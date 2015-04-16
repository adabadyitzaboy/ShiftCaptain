using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShiftCaptain.Models;
using System.Data.Objects.SqlClient;
using System.Text;
using ShiftCaptain.Filters;
using ShiftCaptain.Infrastructure;

namespace ShiftCaptain.Controllers
{
    public class ManageScheduleController : BaseController
    {
        public ManageScheduleController()
        {
            ClassName = "ManageSchedule";
        }
        #region Helpers
        private Clone GetClone(ShiftCaptain.Models.Version version)
        {
            var userItems = new List<SelectListItem>();
            var roomItems = new List<SelectListItem>();
            var clone = new Clone
            {
                Version = version,
                Room = roomItems,
                User = userItems,
                CloneUser = new List<string>(),
                CloneRoom = new List<string>(),
                SelectedUsers = new List<SelectListItem>(),
                SelectedRooms = new List<SelectListItem>()
            };
            foreach (var user in db.UserViews.Where(uv => uv.VersionId == version.Id))
            {
                userItems.Add(new SelectListItem { Value = user.UserId.ToString(), Text = user.NickName });
            }

            foreach (var room in db.RoomViews.Where(rv => rv.VersionId == version.Id))
            {
                roomItems.Add(new SelectListItem { Value = room.RoomId.ToString(), Text = room.Name });
            }

            return clone;
        }

        private VersionErrors GetErrors(ShiftCaptain.Models.Version version)
        {
            var errors = new VersionErrors
            {
                Id = version.Id,
                Name = version.Name,
                IsActive = version.IsActive,
                IsApproved = version.IsApproved,
                IsReadyForApproval = version.IsReadyForApproval,
                IsVisible = version.IsVisible,
                UserConstraintViolations = new List<UserView>(),
                NumErrors = 0
            };

            //go through all of the users and ensure that the hours are within specs
            foreach (var user in db.UserViews.AsNoTracking().Where(uv => uv.VersionId == version.Id).AsNoTracking())
            {
                if (user.CurrentHours < user.MinHours || user.CurrentHours > user.MaxHours)
                {
                    errors.NumErrors++;
                    errors.UserConstraintViolations.Add(user);
                }
            };
            errors.CantWorkViolations = db.CantWorkViolation(version.Id).AsNoTracking();
            errors.ConflictingShifts = db.ConflictingShifts(version.Id).AsNoTracking();
            errors.NoShiftCoverages = db.NoShiftCoverage(version.Id).AsNoTracking();

            errors.NumErrors += errors.CantWorkViolations.Count();
            errors.NumErrors += errors.ConflictingShifts.Count();
            errors.NumErrors += errors.NoShiftCoverages.Count();

            return errors;
        }

        #endregion

        //
        // GET: /ValidateSchedule/5
        [ManagerAccess]
        [ShiftManagerAccess]
        public ActionResult ValidateSchedule(String VersionName)
        {
            var version = GetVersion(VersionName);
            if (version == null)
            {
                return HttpNotFound();
            }
            return View(GetErrors(version));
        }
        
       
        //
        // POST: /ValidateSchedule/5

        [HttpPost]
        [ManagerAccess]
        [ShiftManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult ValidateSchedule(String VersionName, ShiftCaptain.Models.VersionErrors versionId)
        {
            var version = GetVersion(VersionName);
            if (version == null || version.Id != versionId.Id)
            {
                return HttpNotFound();
            }
            var errors = GetErrors(version);
            if (!version.IsReadyForApproval && !version.IsApproved && errors.NumErrors == 0)
            {
                version.IsReadyForApproval = true;
                db.Entry(version).State = EntityState.Modified;
                SaveChange();
                return RedirectToAction("Index", "Shift");
            }

            return View(errors);
        }


        //
        // GET: /ApproveSchedule/5
        [ManagerAccess]
        public ActionResult ApproveSchedule(String VersionName)
        {
            var version = GetVersion(VersionName);
            if (version == null)
            {
                return HttpNotFound();
            }
            return View(GetErrors(version));
        }


        //
        // POST: /ApproveSchedule/5

        [HttpPost]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveSchedule(String VersionName, ShiftCaptain.Models.VersionErrors versionId)
        {
            var version = GetVersion(VersionName);
            if (version == null || version.Id != versionId.Id)
            {
                return HttpNotFound();
            }
            var errors = GetErrors(version);
            if (version.IsReadyForApproval && !version.IsApproved && errors.NumErrors == 0)
            {
                version.IsApproved = true;
                db.Entry(version).State = EntityState.Modified;
                SaveChange();
                return RedirectToAction("Index", "Shift");
            }

            return View(errors);
        }

        //
        // GET: /DisapproveSchedule/5
        [ManagerAccess]
        public ActionResult DisapproveSchedule(String VersionName)
        {
            var version = GetVersion(VersionName);
            if (version == null)
            {
                return HttpNotFound();
            }
            return View(version);
        }


        //
        // POST: /DisapproveSchedule/5

        [HttpPost]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult DisapproveSchedule(String VersionName, ShiftCaptain.Models.Version versionId)
        {
            var version = GetVersion(VersionName);
            if (version == null || version.Id != versionId.Id)
            {
                return HttpNotFound();
            }
            if (version.IsApproved)
            {
                var v = db.Versions.FirstOrDefault(o => o.Id == version.Id);
                v.IsApproved = false;
                v.IsReadyForApproval = false;
                db.Entry(v).State = EntityState.Modified;
                SaveChange();
                return RedirectToAction("Index", "Version");
            }

            return View(version);
        }

        //
        // POST: /RejectSchedule/5

        [HttpPost]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult RejectSchedule(String VersionName, ShiftCaptain.Models.VersionErrors versionId)
        {
            var version = GetVersion(VersionName);
            if (version == null || version.Id != versionId.Id)
            {
                return HttpNotFound();
            }
            var errors = GetErrors(version);
            if (version.IsReadyForApproval && !version.IsApproved && errors.NumErrors == 0 )
            {
                version.IsReadyForApproval = false;
                db.Entry(version).State = EntityState.Modified;
                SaveChange();
            }
            
            return RedirectToAction("Index", "Shift");
        }

        //
        // GET: /CloneSchedule/5
        [ManagerAccess]
        public ActionResult CloneSchedule(String VersionName)
        {
            ClassName = "CloneSchedule";

            var version = GetVersion(VersionName);
            if (version == null)
            {
                return HttpNotFound();
            }
            version.Name = version.Name + " clone";
            
            return View(GetClone(version));
        }

        
        //
        // POST: /CloneSchedule/5

        [HttpPost]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult CloneSchedule(String VersionName, ShiftCaptain.Models.Clone clone)
        {
            ClassName = "CloneSchedule";
            if (ModelState.IsValid)
            {
                var oldVersionId = clone.Version.Id;
                String users = string.Empty;
                String rooms = string.Empty;
                if (clone.CloneUser != null)
                {
                    users = String.Join(",", clone.CloneUser);
                }

                if (clone.CloneRoom != null)
                {
                    rooms = String.Join(",", clone.CloneRoom);
                }
                var result = db.sp_clone_schedule(clone.Version.Id, clone.Version.Name, users, rooms).FirstOrDefault();
                if (result != null && result.HasValue)
                {
                    SessionManager.VersionId = result.Value;
                }
                return RedirectToAction("Index", "Version");
            }
            var rtnClone = GetClone(clone.Version);
            clone.User = rtnClone.User;
            clone.Room = rtnClone.Room;

            clone.SelectedRooms = new List<SelectListItem>();
            clone.SelectedUsers = new List<SelectListItem>();

            var selectedUsers = (List<SelectListItem>)clone.SelectedUsers;
            var selectedRooms = (List<SelectListItem>)clone.SelectedRooms;
            foreach (var roomId in clone.CloneRoom)
            {
                var room = clone.Room.FirstOrDefault(r => r.Value == roomId);
                if (room != null)
                {
                    selectedRooms.Add(room);
                    ((List<SelectListItem>)clone.Room).Remove(room);
                }
            }
            foreach (var userId in clone.CloneUser)
            {
                var user = clone.User.FirstOrDefault(r => r.Value == userId);
                if (user != null)
                {
                    selectedUsers.Add(user);
                    ((List<SelectListItem>)clone.User).Remove(user);
                }
            }

            
            //rtnClone.CloneRoom = clone.CloneRoom;
            //rtnClone.CloneUser = clone.CloneUser;
            //return View(rtnClone);
            return View(clone);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}