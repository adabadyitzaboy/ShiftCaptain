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
    [DeploymentItem("DataSources/ShiftPreferences.csv", "")]
    [DeploymentItem("DataSources/Preferences.csv", "")]
    public class ShiftPreference : CommonActions
    {
        List<String> VersionNames = new List<string>();

        private List<int> shiftPreferenceIds = new List<int>();
        private String OutputShiftPreference(IDictionary<string, string> ShiftPreference)
        {
            return String.Format("Day - {0} Version - {1} User - {2} Preference - {3} StartTime - {4} Duration - {5}",
                Clean<int>(ShiftPreference, "DAY"),
                Clean<string>(ShiftPreference, "VERSION_NAME"),
                Clean<string>(ShiftPreference, "NICK_NAME"),
                Clean<string>(ShiftPreference, "PREFERENCE_NAME"),
                Clean<TimeSpan>(ShiftPreference, "START_TIME"),
                Clean<Decimal>(ShiftPreference, "DURATION"));

        }
        [TestMethod]
        public void CRUD()
        {
            Login();
            var createTable = new DataParser("ShiftPreferences.csv").Tables["Create"];
            var ticks = DateTime.Now.Ticks.ToString();

            foreach (var shiftPreferenceObj in createTable)
            {
                var version = CreateDefaultVersion(Clean<string>(shiftPreferenceObj, "VERSION_NAME"));
                if (!VersionNames.Contains(version.Name))
                {
                    VersionNames.Add(version.Name);
                }
                var user = CreateDefaultUser(Clean<string>(shiftPreferenceObj, "NICK_NAME"), version.Id, version.Name);
                var preference = CreateDefaultPreference(Clean<string>(shiftPreferenceObj, "PREFERENCE_NAME"));
                var shiftPreference = GetShiftPreference(shiftPreferenceObj, version.Id, user.UserId, preference.Id);
                CreateDefaultRoom(version.Id, version.Name);
                GoToPage(version.Name, "ShiftPreference");
                
    
                var created = CreateShiftPreference(version.Name, shiftPreference);
                if (created)
                {
                    shiftPreferenceIds.Add(shiftPreference.Id);
                }
                if (Clean<bool>(shiftPreferenceObj, "VALID"))
                {
                    Assert.IsTrue(created, String.Format("Failed to Create shiftPreference - {0}", OutputShiftPreference(shiftPreferenceObj)));
                    var createdShiftPreference = db.ShiftPreferences.FirstOrDefault(s => s.Id == shiftPreference.Id);
                    Assert.IsTrue(CompareShiftPreference(shiftPreference, createdShiftPreference), String.Format("ShiftPreference does not match {0}", OutputShiftPreference(shiftPreferenceObj)));
                    
                    shiftPreference = createdShiftPreference;
                    shiftPreference.StartTime = shiftPreference.StartTime.Add(TimeSpan.FromHours(Clean<double>(shiftPreferenceObj, "ADD_HOURS")));
                    shiftPreference.Day = (shiftPreference.Day + Clean<int>(shiftPreferenceObj, "ADD_DAYS")) % 7;
                    
                    var edited = EditShiftPreference(version.Name, shiftPreference);
                    Assert.IsTrue(edited, String.Format("Failed editing shiftPreference {0}", OutputShiftPreference(shiftPreferenceObj)));
                    createdShiftPreference = db.ShiftPreferences.FirstOrDefault(s => s.Id == shiftPreference.Id);
                    Assert.IsTrue(CompareShiftPreference(shiftPreference, createdShiftPreference), String.Format("ShiftPreference does not match {0}", OutputShiftPreference(shiftPreferenceObj)));

                    RemoveShiftPreferenceWithUI(version.Name, shiftPreference);
                    
                    createdShiftPreference = db.ShiftPreferences.FirstOrDefault(s => s.Id == shiftPreference.Id);
                    Assert.IsNull(createdShiftPreference, String.Format("Failed to remove shiftPreference {0}", OutputShiftPreference(shiftPreferenceObj)));
                }
                else
                {
                    Assert.IsFalse(created, String.Format("Created shiftPreference - {0}", OutputShiftPreference(shiftPreferenceObj)));
                }
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            RefreshDB();
            try
            {
                foreach (var shiftPreferenceId in shiftPreferenceIds)
                {
                    RemoveShiftPreference(shiftPreferenceId);
                }
            }
            catch (Exception ex)
            {
            }

            
            foreach (var VersionName in VersionNames)
            {
                RemoveDefaultRooms(VersionName);
                RemoveDefaultUsers(VersionName);
                RemoveDefaultBuildings(VersionName);
                RemoveDefaultVersions(VersionName);
            }
        }
    }
}
