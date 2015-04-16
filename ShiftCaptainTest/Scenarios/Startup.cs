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
    public class Startup : BaseScenario
    {
        public Startup()
        {
            VersionName = "StartupScenario";
        }

        private void NavigateAndVerifyErrorPage(String Url, String Page, bool Reroute)
        {
            GoToPage(Url, false, false);
            if (Reroute)
            {
                Assert.AreEqual(BaseUrl + Page, Driver.Url, String.Format("Failed to route to the \"{0}\" page", Page));
            }
            else
            {
                Assert.AreNotEqual(BaseUrl + Page, Driver.Url, String.Format("Routed to the \"{0}\" page", Page));
            }
        }

        private void VerifySetup(String defaultUserEmailAddress)
        {
            if (db.Versions.Count() != 0)
            {
                Assert.Inconclusive("Versions are already in the system.");
            }
            if (db.Buildings.Count() != 0)
            {
                Assert.Inconclusive("Buildings are already in the system.");
            }
            if (db.Rooms.Count() != 0)
            {
                Assert.Inconclusive("Rooms are already in the system.");
            }
            if (db.Preferences.Count() != 0)
            {
                Assert.Inconclusive("Preferences are already in the system.");
            }
            if (db.ShiftPreferences.Count() != 0)
            {
                Assert.Inconclusive("Shift Preferences are already in the system.");
            }
            if (db.Shifts.Count() != 0)
            {
                Assert.Inconclusive("Shifts are already in the system.");
            }
            if (db.Users.Count(u => u.EmailAddress.ToLower() != defaultUserEmailAddress.ToLower()) != 0)
            {
                Assert.Inconclusive("Users are already in the system.");
            }
            
        }
        [TestMethod]
        public void StartUp()
        {
            var defaultUser = UserTable.FirstOrDefault(u => u["NICK_NAME"].ToLower() == "default_manager");
            var defaultEmail = defaultUser["EMAIL_ADDRESS"];
            VerifySetup(defaultEmail);
            var ticks = DateTime.Now.Ticks.ToString();
            var counter = 0;
            
            var user = db.Users.FirstOrDefault(u => u.EmailAddress == defaultEmail);
            if (user == null)
            {
                var newUser = GetUser(defaultUser, ticks, ++counter);
                db.Users.Add(newUser);
                db.SaveChanges();
            }
            Login();
            NavigateAndVerifyErrorPage("", "NoVersions", true);
            NavigateAndVerifyErrorPage("Shift", "NoVersions", true);
            NavigateAndVerifyErrorPage("ShiftPreference", "NoVersions", true);
            NavigateAndVerifyErrorPage("Preference", "NoVersions", false);
            NavigateAndVerifyErrorPage("Building", "NoVersions", false);
            NavigateAndVerifyErrorPage("Room", "NoVersions", true);
            NavigateAndVerifyErrorPage("User", "NoVersions", false);
            NavigateAndVerifyErrorPage("Version", "NoVersions", false);

            var version = CreateAndVerifyDefaultVersion();
            var encodedName = version.Name.Replace(" ", "_");

            NavigateAndVerifyErrorPage("", "NoVersions", false);
            NavigateAndVerifyErrorPage("", encodedName  + "/NoBuildings", true);
            NavigateAndVerifyErrorPage("Shift", "NoVersions", false);
            NavigateAndVerifyErrorPage("Shift", encodedName + "/NoBuildings", true);
            NavigateAndVerifyErrorPage("ShiftPreference", "NoVersions", false);
            NavigateAndVerifyErrorPage("ShiftPreference", encodedName + "/NoBuildings", true);
            NavigateAndVerifyErrorPage("Preference", "NoVersions", false);
            NavigateAndVerifyErrorPage("Building", "NoVersions", false);
            NavigateAndVerifyErrorPage("Room", "NoVersions", false);
            NavigateAndVerifyErrorPage("Room", encodedName + "/NoBuildings", true);
            NavigateAndVerifyErrorPage("User", "NoVersions", false);
            NavigateAndVerifyErrorPage("Version", "NoVersions", false);

            var rooms = CreateAndVerifyDefaultBuildings(version.Id);

            NavigateAndVerifyErrorPage("", encodedName + "/NoBuildings", false);
            NavigateAndVerifyErrorPage("", encodedName + "/NoRooms", true);
            NavigateAndVerifyErrorPage("Shift", encodedName + "/NoBuildings", false);
            NavigateAndVerifyErrorPage("Shift", encodedName + "/NoRooms", true);
            NavigateAndVerifyErrorPage("ShiftPreference", encodedName + "/NoBuildings", false);
            NavigateAndVerifyErrorPage("ShiftPreference", encodedName + "/NoRooms", true);
            NavigateAndVerifyErrorPage("Room", encodedName + "/NoBuildings", false);

            CreateAndVerifyDefaultRooms(rooms);

            NavigateAndVerifyErrorPage("", encodedName + "/NoRooms", false);
            NavigateAndVerifyErrorPage("", encodedName + "/NoUsers", true);
            NavigateAndVerifyErrorPage("Shift", encodedName + "/NoRooms", false);
            NavigateAndVerifyErrorPage("Shift", encodedName + "/NoUsers", true);
            NavigateAndVerifyErrorPage("ShiftPreference", encodedName + "/NoRooms", false);
            NavigateAndVerifyErrorPage("ShiftPreference", encodedName + "/NoUsers", true);

            CreateAndVerifyDefaultUsers(version.Id);

            CreateAndVerifyDefaultPreferences();

            CreateAndVerifyDefaultShiftPreferences(version.Id);


            CreateAndVerifyDefaultShifts(version.Id);

            Assert.IsTrue(SubmitForApproval(version), "Failed to submit schedule for approval");

            Assert.IsTrue(ApproveSchedule(version), "Failed to approve schedule");
        }
    }
}
