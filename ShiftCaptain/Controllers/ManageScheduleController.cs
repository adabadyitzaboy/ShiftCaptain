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

        private ShiftCaptainEntities db = new ShiftCaptainEntities();

        //
        // GET: /ValidateSchedule/5
        [ManagerAccess]
        [ShiftManagerAccess]
        public ActionResult ValidateSchedule(int Id)
        {
            var version = db.Versions.FirstOrDefault(v => v.Id == Id);
            if (version == null)
            {
                return HttpNotFound();
            }
            return View(GetErrors(version));
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
            foreach (var user in db.UserViews.Where(uv => uv.VersionId == version.Id))
            {
                if (user.CurrentHours < user.MinHours || user.CurrentHours > user.MaxHours)
                {
                    errors.NumErrors++;
                    errors.UserConstraintViolations.Add(user);
                }
            };
            errors.CantWorkViolations = db.CantWorkViolation(version.Id);
            errors.ConflictingShifts = db.ConflictingShifts(version.Id);
            errors.NoShiftCoverages = db.NoShiftCoverage(version.Id);

            errors.NumErrors += errors.CantWorkViolations.Count();
            errors.NumErrors += errors.ConflictingShifts.Count();
            errors.NumErrors += errors.NoShiftCoverages.Count();

            return errors;
        }

        //
        // POST: /ValidateSchedule/5

        [HttpPost]
        [ManagerAccess]
        [ShiftManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult ValidateSchedule(ShiftCaptain.Models.VersionErrors versionId)
        {
            var version = db.Versions.FirstOrDefault(v => v.Id == versionId.Id);
            if (version == null)
            {
                return HttpNotFound();
            }
            var errors = GetErrors(version);
            if (!version.IsReadyForApproval && !version.IsApproved && errors.NumErrors == 0)
            {
                version.IsReadyForApproval = true;
                db.Entry(version).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Shift");
            }

            return View(errors);
        }


        //
        // GET: /ApproveSchedule/5
        [ManagerAccess]
        public ActionResult ApproveSchedule(int Id)
        {
            var version = db.Versions.FirstOrDefault(v => v.Id == Id);
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
        public ActionResult ApproveSchedule(ShiftCaptain.Models.VersionErrors versionId)
        {
            var version = db.Versions.FirstOrDefault(v => v.Id == versionId.Id);
            if (version == null)
            {
                return HttpNotFound();
            }
            var errors = GetErrors(version);
            if (version.IsReadyForApproval && !version.IsApproved && errors.NumErrors == 0)
            {
                version.IsApproved = true;
                db.Entry(version).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Shift");
            }

            return View(errors);
        }

        //
        // GET: /DisapproveSchedule/5
        [ManagerAccess]
        public ActionResult DisapproveSchedule(int Id)
        {
            var version = db.Versions.FirstOrDefault(v => v.Id == Id);
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
        public ActionResult DisapproveSchedule(ShiftCaptain.Models.Version versionId)
        {
            var version = db.Versions.FirstOrDefault(v => v.Id == versionId.Id);
            if (version == null)
            {
                return HttpNotFound();
            }
            if (version.IsApproved)
            {
                version.IsApproved = false;
                db.Entry(version).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Version");
            }

            return View(version);
        }

        //
        // POST: /RejectSchedule/5

        [HttpPost]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult RejectSchedule(ShiftCaptain.Models.VersionErrors versionId)
        {
            var version = db.Versions.FirstOrDefault(v => v.Id == versionId.Id);
            if (version == null)
            {
                return HttpNotFound();
            }
            var errors = GetErrors(version);
            if (version.IsReadyForApproval && !version.IsApproved && errors.NumErrors == 0 )
            {
                version.IsReadyForApproval = false;
                db.Entry(version).State = EntityState.Modified;
                db.SaveChanges();
            }
            
            return RedirectToAction("Index", "Shift");
        }

        //
        // GET: /CloneSchedule/5
        [ManagerAccess]
        public ActionResult CloneSchedule(int Id)
        {
            ClassName = "CloneSchedule";

            var version = db.Versions.FirstOrDefault(v => v.Id == Id);
            if (version == null)
            {
                return HttpNotFound();
            }
            version.Name = version.Name + " - clone";
            
            return View(GetClone(version));
        }

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
                CloneRoom = new List<string>()
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

        //
        // POST: /CloneSchedule/5

        [HttpPost]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult CloneSchedule(ShiftCaptain.Models.Clone clone)
        {
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
            rtnClone.CloneRoom = clone.CloneRoom;
            rtnClone.CloneUser = clone.CloneUser;
            return View(rtnClone);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}