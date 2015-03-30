using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftCaptainTest.Infrastructure;
using ShiftCaptain.Models;
using System.Linq;
using OpenQA.Selenium;

namespace ShiftCaptainTest.CRUDOps
{
    /// <summary>
    /// Tests User CRUD operations
    /// </summary>
    [TestClass]
    [DeploymentItem("DataSources/Versions.csv", "")]
    [DeploymentItem("DataSources/Buildings.csv", "")]
    [DeploymentItem("DataSources/Rooms.csv", "")]
    [DeploymentItem("DataSources/Shifts.csv", "")]
    public class Shift : CommonActions
    {

        private List<int> shiftIds = new List<int>();
        private String OutputShift(IDictionary<string, string> Shift)
        {
            return String.Format("Day - {0} Version - {1} User - {2} Room - {3} StartTime - {4} Duration - {5}",
                Clean<int>(Shift, "DAY"),
                Clean<string>(Shift, "VERSION_NAME"),
                Clean<string>(Shift, "NICK_NAME"),
                Clean<string>(Shift, "ROOM_NAME"),
                Clean<TimeSpan>(Shift, "START_TIME"),
                Clean<Decimal>(Shift, "DURATION"));

        }

        [TestMethod]
        public void CRUD()
        {
            Login();
            var createTable = new DataParser("Shifts.csv").Tables["Create"];
            var ticks = DateTime.Now.Ticks.ToString();

            var prevVersionName = String.Empty;
            foreach (var shiftObj in createTable)
            {
                var version = CreateDefaultVersion(Clean<string>(shiftObj, "VERSION_NAME"));
                var user = CreateDefaultUser(Clean<string>(shiftObj, "NICK_NAME"), version.Id, version.Name);
                var room = CreateDefaultRoom(Clean<string>(shiftObj, "ROOM_NAME"), version.Id);
                var shift = GetShift(shiftObj, version.Id, user.UserId, room.RoomId);
                GoToPage("Shift", version.Name != prevVersionName);
                prevVersionName = version.Name;

                var created = CreateShift(shift, room.BuildingId);
                if (created)
                {
                    shiftIds.Add(shift.Id);
                }
                if (Clean<bool>(shiftObj, "VALID"))
                {
                    Assert.IsTrue(created, String.Format("Failed to Create shift - {0}", OutputShift(shiftObj)));
                    var createdShift = db.Shifts.FirstOrDefault(s => s.Id == shift.Id);
                    Assert.IsTrue(CompareShift(shift, createdShift), String.Format("Shift does not match {0}", OutputShift(shiftObj)));

                    shift = createdShift;
                    shift.StartTime = shift.StartTime.Add(TimeSpan.FromHours(Clean<double>(shiftObj, "ADD_HOURS")));
                    shift.Day = (shift.Day + Clean<int>(shiftObj, "ADD_DAYS")) % 7;

                    var edited = EditShift(shift);
                    Assert.IsTrue(edited, String.Format("Failed editing shift {0}", OutputShift(shiftObj)));
                    createdShift = db.Shifts.FirstOrDefault(s => s.Id == shift.Id);
                    Assert.IsTrue(CompareShift(shift, createdShift), String.Format("Shift does not match {0}", OutputShift(shiftObj)));

                    RemoveShiftWithUI(shift);

                    createdShift = db.Shifts.FirstOrDefault(s => s.Id == shift.Id);
                    Assert.IsNull(createdShift, String.Format("Failed to remove shift {0}", OutputShift(shiftObj)));
                }
                else
                {
                    Assert.IsFalse(created, String.Format("Created shift - {0}", OutputShift(shiftObj)));
                }
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            db = new ShiftCaptainEntities();
            try
            {
                foreach (var shiftId in shiftIds)
                {
                    RemoveShift(shiftId);
                }
            }
            catch (Exception ex)
            {
            }
            RemoveDefaultRooms();
            RemoveDefaultUsers();
            RemoveDefaultBuildings();
            RemoveDefaultVersions();
        }
    }
}
