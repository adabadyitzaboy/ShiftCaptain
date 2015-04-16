using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftCaptainTest.Infrastructure;
using ShiftCaptain.Models;
using System.Linq;
using OpenQA.Selenium;

namespace ShiftCaptainTest.Scenarios
{
    /// <summary>
    /// Tests User CRUD operations
    /// </summary>
    [TestClass]
    public class ManageSchedule : BaseScenario
    {
        public ManageSchedule()
        {
            VersionName = "Test_Scenario";
        }

        [TestMethod]
        public void ScheduleOperations()
        {
            Login();

            var version = CreateAndVerifyDefaultVersion();

            var rooms = CreateAndVerifyDefaultBuildings(version.Id);

            CreateAndVerifyDefaultRooms(rooms);

            CreateAndVerifyDefaultUsers(version.Id);

            CreateAndVerifyDefaultPreferences();

            CreateAndVerifyDefaultShiftPreferences(version.Id);

            CreateAndVerifyDefaultShifts(version.Id);

            Assert.IsTrue(SubmitForApproval(version), "Failed to submit schedule for approval");

            Assert.IsTrue(RejectSchedule(version), "Failed to reject schedule");

            Assert.IsTrue(SubmitForApproval(version), "Failed to submit schedule for approval");

            Assert.IsTrue(ApproveSchedule(version), "Failed to approve schedule");

            Assert.IsTrue(DisapproveSchedule(version), "Failed to approve schedule");
        }

        [TestMethod]
        public void CloneSchedule()
        {
            Login();

            var version = CreateAndVerifyDefaultVersion();

            var rooms = CreateAndVerifyDefaultBuildings(version.Id);

            CreateAndVerifyDefaultRooms(rooms);

            CreateAndVerifyDefaultUsers(version.Id);

            CreateAndVerifyDefaultPreferences();

            CreateAndVerifyDefaultShiftPreferences(version.Id);

            CreateAndVerifyDefaultShifts(version.Id);

            var cloneUsers = db.UserViews.Where(uv=>uv.VersionId == version.Id);
            var cloneRooms = db.RoomViews.Where(rv=>rv.VersionId == version.Id);
            //var clone = GetClone(version, clone_rooms, clone_users);
            var cloneName = "Testing clone_" + version.Name;
            ShiftCaptain.Models.Version cloneVersion;
            Assert.IsTrue(CloneSchedule(version.Name, cloneName, cloneUsers, cloneRooms, out cloneVersion), "Failed to clone version.");

            Assert.IsTrue(SubmitForApproval(version), "Failed to submit schedule for approval");

            Assert.IsTrue(RejectSchedule(version), "Failed to reject schedule");

            Assert.IsTrue(SubmitForApproval(version), "Failed to submit schedule for approval");

            Assert.IsTrue(ApproveSchedule(version), "Failed to approve schedule");

            Assert.IsTrue(DisapproveSchedule(version), "Failed to approve schedule");
        }
    }
}
