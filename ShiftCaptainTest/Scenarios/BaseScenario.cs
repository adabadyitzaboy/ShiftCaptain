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
    [DeploymentItem("DataSources/Versions.csv", "")]
    [DeploymentItem("DataSources/Buildings.csv", "")]
    [DeploymentItem("DataSources/Rooms.csv", "")]
    [DeploymentItem("DataSources/Preferences.csv", "")]
    [DeploymentItem("DataSources/Shifts.csv", "")]
    [DeploymentItem("DataSources/ShiftPreferences.csv", "")]
    public class BaseScenario : CommonActions
    {
        public String VersionName = String.Empty;

        public ShiftCaptain.Models.Version CreateAndVerifyDefaultVersion()
        {
            var version = GetDefaultVersion(VersionName);
            if(version.Id == 0){
                Assert.IsTrue(CreateVersion(version), String.Format("Failed to create version {0}", version.Name));
            }
            return GetDefaultVersion(VersionName);            
        }

        public List<RoomView> CreateAndVerifyDefaultBuildings(int versionId)
        {
            List<RoomView> rooms = new List<RoomView>();
            foreach (var roomObj in new DataParser("Rooms.csv").Tables["DefaultRoom"])
            {
                if (VersionName == Clean<string>(roomObj, "VERSION_NAME"))
                {
                    var building = GetDefaultBuildingView(Clean<string>(roomObj, "BUILDING_NAME"));
                    if (building.BuildingId == 0)
                    {
                        Assert.IsTrue(CreateBuilding(building), String.Format("Failed to create building {0}", building.Name));
                    }
                    rooms.Add(GetRoomView(roomObj, GetDefaultBuilding(building.Name), versionId));
                }
            }
            Assert.IsTrue(rooms.Count > 0, "No Default Rooms added");
            return rooms;
        }
        
        public void CreateAndVerifyDefaultRooms(List<RoomView> rooms)
        {
            foreach (var room in rooms)
            {
                if (!RoomExists(room))
                {
                    Assert.IsTrue(CreateRoom(VersionName, room), String.Format("Failed to create room {0}", room.Name));
                }
            }
        }

        private bool RoomExists(RoomView roomview)
        {
            return db.RoomViews.Count(rv => rv.Name == roomview.Name && rv.VersionId == roomview.VersionId) > 0;
        }
        public void CreateAndVerifyDefaultUsers(int versionId)
        {
            var ticks = DateTime.Now.Ticks.ToString();
            var counter = 0;
            
            bool createdUser = false;
            foreach (var userObj in UserTable)
            {
                if (VersionName == Clean<string>(userObj, "VERSION_NAME"))
                {
                    if (!Clean<bool>(userObj, "CREATED"))
                    {
                        var userview = GetUserView(userObj, versionId, ticks, counter);
                        if (!UserExists(userview))
                        {
                            Assert.IsTrue(CreateUser(VersionName, userview), String.Format("Failed to create user {0}", userview.NickName));
                        }
                    }
                    createdUser = true;
                }
            }
            Assert.IsTrue(createdUser, "No Default Users added");
        }
        private bool UserExists(UserView userview)
        {
            return db.UserViews.Count(uv => uv.NickName == userview.NickName && uv.VersionId == userview.VersionId) > 0;
        }
        public void CreateAndVerifyDefaultPreferences()
        {
            bool createdPreference = false;
            //var preferences = GetPreferencesForDefaultShiftPreferences();
            foreach (var preferenceObj in new DataParser("Preferences.csv").Tables["DefaultPreference"])
            {
                var preference = GetPreference(preferenceObj);
                if (Clean<string>(preferenceObj, "VERSION_NAME") == VersionName)
                {
                    if (!PreferenceExists(preference))
                    {
                        Assert.IsTrue(CreatePreference(preference), String.Format("Failed to create preference {0}", preference.Name));
                    }
                    createdPreference = true;
                }
            }
            Assert.IsTrue(createdPreference, "No Default Preferences added");
        }
        private bool PreferenceExists(Preference preference)
        {
            return db.Preferences.Count(p => p.Name == preference.Name) > 0;
        }

        //public List<String> GetPreferencesForDefaultShiftPreferences()
        //{
        //    List<String> preferences = new List<string>();
        //    foreach (var shiftPreferenceObj in new DataParser("ShiftPreferences.csv").Tables["DefaultShiftPreference"])
        //    {
        //        preferences.Add(Clean<string>(shiftPreferenceObj, "PREFERENCE_NAME"));
        //    }
        //    return preferences;
        //}

        public void CreateAndVerifyDefaultShiftPreferences(int versionId)
        {
            bool createdShiftPreference = false;
            foreach (var shiftPreferenceObj in new DataParser("ShiftPreferences.csv").Tables["DefaultShiftPreference"])
            {
                if (VersionName == Clean<string>(shiftPreferenceObj, "VERSION_NAME"))
                {
                    var shiftPreferencUser = GetDefaultUser(Clean<string>(shiftPreferenceObj, "NICK_NAME"), versionId, VersionName);
                    var preference = GetDefaultPreference(Clean<string>(shiftPreferenceObj, "PREFERENCE_NAME"));
                    var shiftPreference = GetShiftPreference(shiftPreferenceObj, versionId, shiftPreferencUser.UserId, preference.Id);
                    var entity = db.ShiftPreferences.FirstOrDefault(sp => sp.VersionId == shiftPreference.VersionId && sp.UserId == shiftPreference.UserId && sp.Day == shiftPreference.Day && sp.PreferenceId == shiftPreference.PreferenceId && sp.StartTime == shiftPreference.StartTime && sp.Duration == shiftPreference.Duration);
                    if (entity == null)
                    {
                        Assert.IsTrue(CreateShiftPreference(VersionName, shiftPreference), String.Format("Failed to create shift preference"));
                    }
                    createdShiftPreference = true;
                }
            }
            Assert.IsTrue(createdShiftPreference, "No Default Shift Preferences added");

        }

        public void CreateAndVerifyDefaultShifts(int versionId)
        {
            bool createdShift = false;
            foreach (var shiftObj in new DataParser("Shifts.csv").Tables["DefaultShift"])
            {
                if (Clean<string>(shiftObj, "VERSION_NAME") == VersionName)
                {
                    var shiftUser = GetDefaultUser(Clean<string>(shiftObj, "NICK_NAME"), versionId, VersionName);
                    var shiftRoom = GetDefaultRoom(Clean<string>(shiftObj, "ROOM_NAME"), versionId);
                    var shift = GetShift(shiftObj, versionId, shiftUser.UserId, shiftRoom.Id);
                    var entity = db.Shifts.FirstOrDefault(s => s.VersionId == shift.VersionId && s.UserId == shift.UserId && s.Day == shift.Day && s.RoomId == shift.RoomId && s.StartTime == shift.StartTime && s.Duration == shift.Duration);
                    if (entity == null)
                    {
                        Assert.IsTrue(CreateShift(VersionName, shift, shiftRoom.BuildingId), String.Format("Failed to create shift"));
                    }
                    createdShift = true;
                }
            }
            Assert.IsTrue(createdShift, "No Default Shift added");

        }

        [TestCleanup]
        public void CleanUp()
        {
            RefreshDB();
            //RemoveDefaultShifts(VersionName);
            //RemoveDefaultShiftPreferences(VersionName);
            //RemoveDefaultPreferences();
            //RemoveDefaultRooms(VersionName);
            //RemoveDefaultUsers(VersionName);
            //RemoveDefaultBuildings(VersionName);
            //RemoveDefaultVersions(VersionName);
        }
    }
}
