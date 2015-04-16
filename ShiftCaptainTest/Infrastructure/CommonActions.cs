using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using OpenQA.Selenium;
using System.Linq;
using ShiftCaptain.Models;
using System.Collections.Generic;

namespace ShiftCaptainTest.Infrastructure
{
    public class CommonActions : ModelConverter
    {
        
        public void Logout()
        {
            try
            {
                GoToPage("Logout.ashx");
            }
            catch (Exception)
            {
            }

        }

        public bool GoToPage(string page, bool force = false, bool verifyUrl = true)
        {
            if (!force && (BaseUrl + page == Driver.Url || BaseUrl + page + "/" == Driver.Url))
            {
                return false;
            }
            switch (page)
            {
                default:
                    _goToPage(page, verifyUrl);
                    break;
            }
            return true;
        }
        
        public bool GoToPage(string versionName, string page, bool force = false)
        {
            return GoToPage(String.Format("{0}/{1}", EncodeVersionName(versionName), page), force);
        }

        private void _goToPage(string page, bool verifyUrl = true)
        {
            string currentUrl = Driver.Url;
            var start = DateTime.Now;
            Driver.Navigate().GoToUrl(BaseUrl + page);
            var ellapsed = (DateTime.Now - start).TotalSeconds;
            Assert.IsTrue(ellapsed < ElapsedTimeThreshold, String.Format("{0} exceeded Timeout threshold - {1}", page, ellapsed));
            if (LogLoadTimes)
            {
                Logger.InfoFormat("{0}\t{1}{2}\t{3}\t{4}\t{5}", DateTime.Now.ToString("MM/dd/yy H:mm:ss"), BaseUrl, page, ellapsed, DriverType, IsLoggedIn());
            }
            if (verifyUrl)
            {
                if (currentUrl != BaseUrl + page)
                {
                    WaitForNextPage(currentUrl);
                }
                Assert.AreEqual(BaseUrl + page, Driver.Url);
            }
        }

        public bool Login(String email = null, String password = null)
        {
            Logout();
            GoToPage("Login");
            if (email == null || password == null)
            {
                var defaultUser = UserTable.FirstOrDefault(u => u["NICK_NAME"].ToLower() == "default_manager");
                var defaultEmail = defaultUser["EMAIL_ADDRESS"];
                var user = db.Users.FirstOrDefault(u => u.EmailAddress == defaultEmail);
                if (user == null)
                {
                    throw new Exception(String.Format("User {0} not found", defaultEmail));
                }
                email = user.EmailAddress;
                password = user.Pass;
            }
            Driver.FindElement(By.Id("EmailAddress")).SendKeys(email);
            Driver.FindElement(By.Id("Pass")).SendKeys(password);
            ClickAndWaitForElements(Driver.FindElement(GetSubmitButton()), new List<ByExists> { new ByExists { by = By.Id("EmailAddress"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true } });
            return !ElementExists(By.Id("EmailAddress"));
        }
        #region User 
        
        public bool CreateUser(String VersionName, UserView user)
        {
            GoToPage(VersionName, "User/Create");
            SetTextBoxValues(user);
            ClickAndWaitForElements(Driver.FindElement(GetSubmitButton()), new List<ByExists> { new ByExists { by = By.Id("EmailAddress"), exists = false}, new ByExists{ by = By.CssSelector(".validation-summary-errors"), exists = true}, new ByExists{ by = By.CssSelector(".field-validation-error"), exists = true}});
            return !Driver.Url.Contains("User/Create");
        }

        public ShiftCaptain.Models.UserView CreateDefaultUser(String NickName, int? VersionId, String VersionName)
        {
            RefreshDB();
            var ticks = DateTime.Now.Ticks.ToString();
            
            if (db.UserViews.Count(uv => uv.NickName == NickName && uv.VersionId == VersionId) == 0)
            {
                var createTable = new DataParser("Users.csv").Tables["DefaultUser"];
                foreach (var userObj in createTable)
                {

                    var user = GetUser(userObj, ticks, 1);
                    if (user.NickName == NickName && Clean<string>(userObj, "VERSION_NAME") == VersionName)
                    {
                        if (db.UserViews.Count(uv => uv.NickName == NickName) == 0)
                        {
                            var address = CreateAddress(user.Address);
                            
                            if (address != null)
                            {
                                user.AddressId = address.Id;
                            }

                            db.Users.Add(user);                            
                            db.SaveChanges();
                            if (VersionId.HasValue)
                            {
                                db.UserInstances.Add(new UserInstance
                                {
                                    UserId = user.Id,
                                    VersionId = VersionId.Value,
                                    MinHours = Clean<decimal>(userObj, "MIN_HOURS"),
                                    MaxHours = Clean<decimal>(userObj, "MAX_HOURS")
                                });
                                db.SaveChanges();
                            }
                        }                        
                        
                        break;
                    }
                }
            }
            return db.UserViews.FirstOrDefault(uv => uv.NickName == NickName && uv.VersionId == VersionId);
        }
        
        public ShiftCaptain.Models.UserView GetDefaultUser(String NickName, int VersionId, String VersionName)
        {
            RefreshDB();
            var ticks = DateTime.Now.Ticks.ToString();

            if (db.UserViews.Count(uv => uv.NickName == NickName && uv.VersionId == VersionId) == 0)
            {
                var createTable = new DataParser("Users.csv").Tables["DefaultUser"];
                foreach (var userObj in createTable)
                {
                    var user = GetUserView(userObj, VersionId, ticks, 1);
                    if (user.NickName == NickName && Clean<string>(userObj, "VERSION_NAME") == VersionName)
                    {
                        return user;
                    }
                }
            }
            return db.UserViews.FirstOrDefault(uv => uv.NickName == NickName && uv.VersionId == VersionId);
        }

        public void RemoveDefaultUsers(String VersionName)
        {
            try
            {
                var createTable = new DataParser("Users.csv").Tables["DefaultUser"];
                foreach (var userObj in createTable)
                {
                    if (!Clean<bool>(userObj, "CREATED") && Clean<string>(userObj, "VERSION_NAME") == VersionName)
                    {
                        var nickName = Clean<string>(userObj, "NICK_NAME");
                        var users = db.Users.Where(u => u.NickName == nickName).ToList();
                        foreach (var user in users)
                        {
                            RemoveUser(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
        
        public bool EditUser(String VersionName, UserView user)
        {
            GoToPage(VersionName, "User/Edit/" + user.UserId);
            SetTextBoxValues(user);
            ClickAndWaitForElements(Driver.FindElement(GetSubmitButton()), new List<ByExists> { new ByExists { by = By.Id("EmailAddress"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });
            return !Driver.Url.Contains("User/Edit/");
        }

        public void RemoveUser(String EmailAddress)
        {
            RemoveUser(db.Users.FirstOrDefault(u => u.EmailAddress == EmailAddress));
        }
        public void RemoveUser(ShiftCaptain.Models.User user)
        {
            if (user != null)
            {
                var userview = db.UserViews.FirstOrDefault(uv => uv.UserId == user.Id);
                if (userview != null)
                {
                    if (userview.VersionId != null)
                    {
                        var save = false;
                        foreach (var shiftPreference in db.ShiftPreferences.Where(sp => sp.UserId == userview.UserId).ToList())
                        {
                            db.ShiftPreferences.Remove(shiftPreference);
                            save = true;
                        }
                        foreach (var shift in db.Shifts.Where(s => s.UserId == userview.UserId).ToList())
                        {
                            db.Shifts.Remove(shift);
                            save = true;
                        }
                        if (save)
                        {
                            db.SaveChanges();
                        }
                        foreach (var userInstance in db.UserInstances.Where(ui => ui.UserId == userview.UserId).ToList())
                        {
                            db.UserInstances.Remove(userInstance);
                            db.SaveChanges();
                        }
                    }
                }
                db.Users.Remove(user);
                db.SaveChanges();
            }
        }

        public bool CompareUser(UserView user1, UserView user2)
        {
            var invalidNames = new string[] { "UserId", "VersionId", "LastLogin", "AddressId", "Pass"};
            var type = user1.GetType();
            foreach (System.Reflection.PropertyInfo propertyInfo in type.GetProperties())
            {
                if (propertyInfo.CanRead  && !invalidNames.Contains(propertyInfo.Name))
                {
                    var value1 = propertyInfo.GetValue(user1);
                    var value2 = propertyInfo.GetValue(user2);
                    if (value1 != value2 && (value1 != null && value2 != null && value1.ToString() != value2.ToString()))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        
        
        #endregion

        #region Version
        public ShiftCaptain.Models.Version CreateDefaultVersion(String VersionName)
        {
            var version = GetDefaultVersion(VersionName);
            if (version != null && version.Id == 0)
            {
                db.Versions.Add(version);
                db.SaveChanges();        
            }
            return version;
        }

        public ShiftCaptain.Models.Version GetDefaultVersion(String VersionName)
        {
            RefreshDB();
            if (db.Versions.Count(v => v.Name == VersionName) == 0)
            {
                var createTable = new DataParser("Versions.csv").Tables["DefaultVersion"];
                foreach (var versionObj in createTable)
                {
                    var version = GetVersion(versionObj);
                    if (version.Name == VersionName)
                    {
                        return version;
                    }
                }
            }
            return db.Versions.FirstOrDefault(v => v.Name == VersionName);

        }

        public void RemoveDefaultVersions(String VersionName)
        {
            try
            {
                var createTable = new DataParser("Versions.csv").Tables["DefaultVersion"];
                foreach (var versionObj in createTable)
                {
                    var name = Clean<string>(versionObj, "NAME");
                    if (Clean<string>(versionObj, "VERSION_NAME") == VersionName)
                    {
                        var version = db.Versions.FirstOrDefault(v => v.Name == name);
                        if (version != null)
                        {
                            //var preferences = db.Preferences.Where(e => e.ShiftPreferences.Count(sp => sp.PreferenceId == e.Id && sp.VersionId == version.Id) > 0).ToList();
                            //var userInstances = db.UserInstances.Where(e => e.VersionId == version.Id).ToList();
                            //var address = db.Addresses.Where(e => e.Users.Count(u => e.Id == u.AddressId) > 0 || e.Users.Count(u => e.Id == u.AddressId) > 0).ToList();
                            //var users = db.Users.Where(e => e.UserInstances.Count(sp => sp.UserId == e.Id && sp.VersionId == version.Id) > 0).ToList();
                            //foreach (var entity in db.Shifts.Where(e => e.VersionId == version.Id).ToList())
                            //{
                            //    db.Shifts.Remove(entity);
                            //    db.SaveChanges();
                            //}
                            //foreach (var entity in db.ShiftPreferences.Where(e => e.VersionId == version.Id).ToList())
                            //{
                            //    db.ShiftPreferences.Remove(entity);
                            //    db.SaveChanges();
                            //}
                            //foreach (var entity in preferences)
                            //{
                            //    db.Preferences.Remove(entity);
                            //    db.SaveChanges();
                            //}
                            //foreach (var entity in address)
                            //{
                            //    db.Users.Remove(entity);
                            //    db.SaveChanges();
                            //}
                            //foreach (var entity in users)
                            //{
                            //    db.Users.Remove(entity);
                            //    db.SaveChanges();
                            //}
                            db.Versions.Remove(version);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
        public bool CreateVersion(ShiftCaptain.Models.Version version)
        {
            GoToPage("Version/Create");
            SetTextBoxValues(version);
            ClickAndWaitForElements(Driver.FindElement(GetSubmitButton()), new List<ByExists> { new ByExists { by = By.Id("Name"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });
            return !Driver.Url.Contains("Version/Create");
        }

        public bool EditVersion(ShiftCaptain.Models.Version version)
        {
            GoToPage("Version/Edit/" + version.Id);
            SetTextBoxValues(version);
            ClickAndWaitForElements(Driver.FindElement(GetSubmitButton()), new List<ByExists> { new ByExists { by = By.Id("Name"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });
            return !Driver.Url.Contains("Version/Edit/");
        }

        public void RemoveVersion(String Name)
        {
            RemoveVersion(db.Versions.FirstOrDefault(v => v.Name == Name));
        }
        
        public void RemoveVersion(int id)
        {
            RemoveVersion(db.Versions.FirstOrDefault(v => v.Id == id));

        }
        public void RemoveVersion(ShiftCaptain.Models.Version Version)
        {
            if (Version != null)
            {
                foreach (var shiftPreference in db.ShiftPreferences.Where(sp => sp.VersionId == Version.Id))
                {
                    db.ShiftPreferences.Remove(shiftPreference);
                }
                foreach (var shift in db.Shifts.Where(s => s.VersionId == Version.Id))
                {
                    db.Shifts.Remove(shift);
                }
                db.SaveChanges();

                foreach (var userInstance in db.UserInstances.Where(uv => uv.VersionId == Version.Id))
                {
                    db.UserInstances.Remove(userInstance);
                }
                foreach (var roomInstance in db.RoomInstances.Where(ri => ri.VersionId == Version.Id))
                {
                    db.RoomInstances.Remove(roomInstance);
                }
                db.SaveChanges();
                db.Versions.Remove(Version);
                db.SaveChanges();
            }
        }

        public bool CompareVersion(ShiftCaptain.Models.Version v1, ShiftCaptain.Models.Version v2)
        {
            var invalidNames = new string[] { "Id"};
            var type = v1.GetType();
            foreach (System.Reflection.PropertyInfo propertyInfo in type.GetProperties())
            {
                if (propertyInfo.CanRead && !invalidNames.Contains(propertyInfo.Name))
                {
                    var value1 = propertyInfo.GetValue(v1);
                    var value2 = propertyInfo.GetValue(v2);
                    if (value1 != value2 && (value1 != null && value2 != null && value1.ToString() != value2.ToString()))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        #endregion
        
        #region Address
        
        public ShiftCaptain.Models.Address CreateAddress(Address address)
        {
            if (address != null)
            {
                db.Addresses.Add(address);
                db.SaveChanges();
                return address;
            }
            return null;
        }

        #endregion
        
        #region Building

        public bool CreateBuilding(BuildingView Building)
        {
            GoToPage("Building/Create");
            SetTextBoxValues(Building);
            ClickAndWaitForElements(Driver.FindElement(GetSubmitButton()), new List<ByExists> { new ByExists { by = By.Id("Name"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });
            return !Driver.Url.Contains("Building/Create");
        }
        
        public ShiftCaptain.Models.Building CreateDefaultBuilding(String BuildingName)
        {
            var building = GetDefaultBuilding(BuildingName);
            if (building.Id == 0)
            {
                db.Buildings.Add(building);
                db.SaveChanges();
                if (building.Address != null)
                {
                    building.Address = CreateAddress(building.Address);
                    building.AddressId = building.Address.Id; 
                }
            }
            return building;
        }

        public ShiftCaptain.Models.Building GetDefaultBuilding(String BuildingName)
        {
            RefreshDB();
            if (db.Buildings.Count(b => b.Name == BuildingName) == 0)
            {
                var createTable = new DataParser("Buildings.csv").Tables["DefaultBuilding"];
                foreach (var buildingObj in createTable)
                {
                    var building = GetBuilding(buildingObj);
                    if (building.Name == BuildingName)
                    {                       
                        return building;
                    }
                }
            }
            return db.Buildings.FirstOrDefault(v => v.Name == BuildingName);
        }

        public ShiftCaptain.Models.BuildingView GetDefaultBuildingView(String BuildingName)
        {
            RefreshDB();
            if (db.BuildingViews.Count(b => b.Name == BuildingName) == 0)
            {
                var createTable = new DataParser("Buildings.csv").Tables["DefaultBuilding"];
                foreach (var buildingObj in createTable)
                {
                    var building = GetBuildingView(buildingObj);
                    if (building.Name == BuildingName)
                    {
                        return building;
                    }
                }
            }
            return db.BuildingViews.FirstOrDefault(v => v.Name == BuildingName);
        }

        public void RemoveDefaultBuildings(String VersionName)
        {
            try
            {
                var createTable = new DataParser("Buildings.csv").Tables["DefaultBuilding"];
                foreach (var buildingObj in createTable)
                {
                    var name = Clean<string>(buildingObj, "NAME");
                    if (Clean<string>(buildingObj, "VERSION_NAME") == VersionName)
                    {
                        var building = db.Buildings.FirstOrDefault(v => v.Name == name);
                        if (building != null)
                        {
                            if (building.AddressId.HasValue)
                            {
                                db.Addresses.Remove(building.Address);
                                db.SaveChanges();
                            }
                            db.Buildings.Remove(building);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public bool EditBuilding(BuildingView Building)
        {
            GoToPage("Building/Edit/" + Building.BuildingId);
            SetTextBoxValues(Building);
            ClickAndWaitForElements(Driver.FindElement(GetSubmitButton()), new List<ByExists> { new ByExists { by = By.Id("Name"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });
            return !Driver.Url.Contains("Building/Edit/");
        }

        public void RemoveBuilding(String Name)
        {
            var building = db.Buildings.FirstOrDefault(u => u.Name == Name);
            RemoveBuilding(building);
        }
        public void RemoveBuilding(int id)
        {
            var building = db.Buildings.FirstOrDefault(u => u.Id == id);
            RemoveBuilding(building);
        }
        private void RemoveBuilding(ShiftCaptain.Models.Building Building)
        {
            if (Building != null)
            {
                if (Building.AddressId.HasValue) {
                    db.Addresses.Remove(Building.Address);
                    db.SaveChanges();
                }
                db.Buildings.Remove(Building);
                db.SaveChanges();
            }
        }
        public bool CompareBuilding(BuildingView Building1, BuildingView Building2)
        {
            var invalidNames = new string[] { "BuildingId", "AddressId"};
            var type = Building1.GetType();
            foreach (System.Reflection.PropertyInfo propertyInfo in type.GetProperties())
            {
                if (propertyInfo.CanRead && !invalidNames.Contains(propertyInfo.Name))
                {
                    var value1 = propertyInfo.GetValue(Building1);
                    var value2 = propertyInfo.GetValue(Building2);
                    if (value1 != value2 && (value1 != null && value2 != null && value1.ToString() != value2.ToString()))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        

        #endregion

        #region Room
        public bool CreateRoom(String VersionName, RoomView Room)
        {
            GoToPage(VersionName, "Room/Create");
            RemoveFixedHeader();
            SetDropDownValues(Room);
            SetTextBoxValues(Room);
            ClickAndWaitForElements(Driver.FindElement(GetSubmitButton()), new List<ByExists> { new ByExists { by = By.Id("Name"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });
            return !Driver.Url.Contains("Room/Create");
        }

        public ShiftCaptain.Models.Room CreateDefaultRoom(String RoomName, int VersionId)
        {
            var room = GetDefaultRoom(RoomName, VersionId);
            if (room != null && room.Id == 0)
            {
                db.Rooms.Add(room);
                db.SaveChanges();
            }
            if (room.RoomInstances != null && room.RoomInstances.Count() > 0)
            {
                var roomInstance = room.RoomInstances.FirstOrDefault();
                if (roomInstance.Id == 0)
                {
                    var updatedRoomInstance = db.RoomInstances.Add(roomInstance);
                    db.SaveChanges();
                    foreach (var roomHour in roomInstance.RoomHours)
                    {
                        roomHour.RoomInstanceId = updatedRoomInstance.Id;
                        db.RoomHours.Add(roomHour);
                        db.SaveChanges();
                    }
                }
            }
            return room;
        }
        public ShiftCaptain.Models.Room GetDefaultRoom(String RoomName, int VersionId)
        {
            RefreshDB();
            if (db.RoomViews.Count(r => r.Name == RoomName && r.VersionId == VersionId) == 0)
            {
                var createTable = new DataParser("Rooms.csv").Tables["DefaultRoom"];
                foreach (var roomObj in createTable)
                {
                    if (Clean<string>(roomObj, "NAME") == RoomName)
                    {
                        var building = CreateDefaultBuilding(Clean<string>(roomObj, "BUILDING_NAME"));
                        var room = db.Rooms.FirstOrDefault(rv => rv.Name == RoomName);
                        if (room == null)
                        {
                            room = GetRoom(roomObj, building.Id);
                        }
                        if (HasRoomHours(roomObj))
                        {
                            var roomInstance = new RoomInstance
                            {
                                RoomId = room.Id,
                                VersionId = VersionId,
                                RoomHours = new List<RoomHour>()
                            };
                            room.RoomInstances = new List<RoomInstance> { roomInstance };
                            
                            foreach (var roomHour in GetRoomHours(roomObj, 0))
                            {
                                roomInstance.RoomHours.Add(roomHour);
                            }
                        }
                        return room;
                    }
                }
            }
            return db.Rooms.FirstOrDefault(rv => rv.Name == RoomName && rv.RoomInstances.Count(ri=>ri.RoomId == rv.Id && ri.VersionId == VersionId) > 0);
        }

        public ShiftCaptain.Models.RoomView CreateDefaultRoom(int VersionId, String VersionName)
        {
            RefreshDB();
            if (db.RoomViews.Count(r => r.VersionId == VersionId) == 0)
            {
                var createTable = new DataParser("Rooms.csv").Tables["DefaultRoom"];
                foreach (var roomObj in createTable)
                {
                    if (Clean<string>(roomObj, "VERSION_NAME") == VersionName)
                    {
                        var building = CreateDefaultBuilding(Clean<string>(roomObj, "BUILDING_NAME"));
                        var room = GetRoom(roomObj, building.Id);

                        db.Rooms.Add(room);
                        db.SaveChanges();
                        if (HasRoomHours(roomObj))
                        {
                            var roomInstance = new RoomInstance
                            {
                                RoomId = room.Id,
                                VersionId = VersionId
                            };
                            roomInstance = db.RoomInstances.Add(roomInstance);
                            db.SaveChanges();
                            foreach (var roomHour in GetRoomHours(roomObj, roomInstance.Id))
                            {
                                if (roomHour != null)
                                {
                                    db.RoomHours.Add(roomHour);
                                    db.SaveChanges();
                                }
                            }
                        }
                        break;
                    }
                }
            }
            return db.RoomViews.FirstOrDefault(rv => rv.VersionId == VersionId);
        }
                
        public void RemoveDefaultRooms(String VersionName)
        {
            try
            {
                var createTable = new DataParser("Rooms.csv").Tables["DefaultRoom"];
                foreach (var roomObj in createTable)
                {
                    var name = Clean<string>(roomObj, "NAME");
                    if (Clean<string>(roomObj, "VERSION_NAME") == VersionName)
                    {
                        foreach (var room in db.Rooms.Where(r => r.Name == name).ToList())
                        {
                            RemoveRoom(room.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public bool EditRoom(String VersionName, RoomView Room)
        {
            GoToPage(VersionName, "Room/Edit/" + Room.RoomId);
            RemoveFixedHeader();
            SetDropDownValues(Room);
            SetTextBoxValues(Room);
            ClickAndWaitForElements(Driver.FindElement(GetSubmitButton()), new List<ByExists> { new ByExists { by = By.Id("Name"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });
            return !Driver.Url.Contains("Room/Edit/");
        }

        public void RemoveRoom(String Name)
        {
            var room = db.Rooms.FirstOrDefault(u => u.Name == Name);
            RemoveRoom(room);
        }
        public void RemoveRoom(int id)
        {
            var room = db.Rooms.FirstOrDefault(u => u.Id == id);
            RemoveRoom(room);
        }
        private void RemoveRoom(ShiftCaptain.Models.Room Room)
        {
            if (Room != null)
            {
                foreach (var roomInstance in Room.RoomInstances.ToList())
                {
                    foreach (var roomHour in roomInstance.RoomHours.ToList())
                    {
                        db.RoomHours.Remove(roomHour);   
                    }
                    db.SaveChanges();
                    db.RoomInstances.Remove(roomInstance);
                    db.SaveChanges();
                }
                db.Rooms.Remove(Room);
                db.SaveChanges();
            }
        }
        public bool CompareRoom(RoomView Room1, RoomView Room2)
        {
            var invalidNames = new string[] { "RoomId", "AddressId" };
            var type = Room1.GetType();
            foreach (System.Reflection.PropertyInfo propertyInfo in type.GetProperties())
            {
                if (propertyInfo.CanRead && !invalidNames.Contains(propertyInfo.Name))
                {
                    var value1 = propertyInfo.GetValue(Room1);
                    var value2 = propertyInfo.GetValue(Room2);
                    if (value1 != value2 && (value1 != null && value2 != null && value1.ToString() != value2.ToString()))
                    {
                        return false;
                    }
                }
            }
            return true;
        }



        #endregion

        #region Preference

        public ShiftCaptain.Models.Preference CreateDefaultPreference(String PreferenceName)
        {
            var defaultPreference = GetDefaultPreference(PreferenceName);
            if (defaultPreference != null && defaultPreference.Id == 0)
            {
                defaultPreference= db.Preferences.Add(defaultPreference);
                db.SaveChanges();
            }
            return defaultPreference;
        }
        public ShiftCaptain.Models.Preference GetDefaultPreference(String PreferenceName)
        {
            RefreshDB();
            if (db.Preferences.Count(p => p.Name == PreferenceName) == 0)
            {
                var createTable = new DataParser("Preferences.csv").Tables["DefaultPreference"];
                foreach (var preferenceObj in createTable)
                {
                    var preference = GetPreference(preferenceObj);
                    if (preference.Name == PreferenceName)
                    {
                        return preference;
                    }
                }
            }
            return db.Preferences.FirstOrDefault(p => p.Name == PreferenceName);
        }
        public void RemoveDefaultPreferences()
        {
            try
            {
                var createTable = new DataParser("Preferences.csv").Tables["DefaultPreference"];
                foreach (var preferenceObj in createTable)
                {
                    var name = Clean<string>(preferenceObj, "NAME");
                    var preference = db.Preferences.FirstOrDefault(v => v.Name == name);
                    if (preference != null)
                    {
                        db.Preferences.Remove(preference);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }
        public bool CreatePreference(ShiftCaptain.Models.Preference preference)
        {
            GoToPage("Preference/Create");
            SetTextBoxValues(preference);
            ClickAndWaitForElements(Driver.FindElement(GetSubmitButton()), new List<ByExists> { new ByExists { by = By.Id("Name"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });
            return !Driver.Url.Contains("Preference/Create");
        }

        public bool EditPreference(ShiftCaptain.Models.Preference preference)
        {
            GoToPage("Preference/Edit/" + preference.Id);
            SetTextBoxValues(preference);
            ClickAndWaitForElements(Driver.FindElement(GetSubmitButton()), new List<ByExists> { new ByExists { by = By.Id("Name"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });
            return !Driver.Url.Contains("Preference/Edit/");
        }

        public void RemovePreference(String Name)
        {
            RemovePreference(db.Preferences.FirstOrDefault(v => v.Name == Name));
        }

        public void RemovePreference(int id)
        {
            RemovePreference(db.Preferences.FirstOrDefault(v => v.Id == id));

        }
        public void RemovePreference(ShiftCaptain.Models.Preference Preference)
        {
            if (Preference != null)
            {
                db.Preferences.Remove(Preference);
                db.SaveChanges();
            }
        }

        public bool ComparePreference(ShiftCaptain.Models.Preference v1, ShiftCaptain.Models.Preference v2)
        {
            var invalidNames = new string[] { "Id" };
            var type = v1.GetType();
            foreach (System.Reflection.PropertyInfo propertyInfo in type.GetProperties())
            {
                if (propertyInfo.CanRead && !invalidNames.Contains(propertyInfo.Name))
                {
                    var value1 = propertyInfo.GetValue(v1);
                    var value2 = propertyInfo.GetValue(v2);
                    if (value1 != value2 && (value1 != null && value2 != null && value1.ToString() != value2.ToString()))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        #endregion

        #region ShiftPreference

        //public ShiftCaptain.Models.ShiftPreference CreateDefaultShiftPreference(String ShiftPreferenceName)
        //{
        //    if (db.ShiftPreferences.Count(p => p.Name == ShiftPreferenceName) == 0)
        //    {
        //        var createTable = new DataParser("ShiftPreferences.csv").Tables["DefaultShiftPreference"];
        //        foreach (var shiftPreferenceObj in createTable)
        //        {
        //            var shiftPreference = new ShiftCaptain.Models.ShiftPreference
        //            {
        //                Name = Clean<string>(shiftPreferenceObj, "NAME"),
        //                Description = Clean<string>(shiftPreferenceObj, "DESCRIPTION"),
        //                CanWork = Clean<bool>(shiftPreferenceObj, "CAN_WORK"),
        //                Color = Clean<string>(shiftPreferenceObj, "COLOR")
        //            };
        //            if (shiftPreference.Name == ShiftPreferenceName)
        //            {
        //                db.ShiftPreferences.Add(shiftPreference);
        //                db.SaveChanges();
        //                break;
        //            }
        //        }
        //    }
        //    return db.ShiftPreferences.FirstOrDefault(p => p.Name == ShiftPreferenceName);
        //}
        public void RemoveDefaultShiftPreferences(String VersionName)
        {
            try
            {
                foreach (var shiftPreference in db.ShiftPreferences.Where(sp => sp.Version.Name == VersionName).ToList())
                {
                    db.ShiftPreferences.Remove(shiftPreference);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
        public bool CreateShiftPreference(String VersionName, ShiftCaptain.Models.ShiftPreference shiftPreference)
        {
            try
            {
                var navigated = GoToPage(VersionName, "ShiftPreference");
                bool changedVersion = SetDropDownValue(Driver.FindElement(By.Id("VersionId")), shiftPreference.VersionId.ToString());
                if (changedVersion)
                {
                    WaitTillAvailable(By.Id("Preferences"));
                }
                bool changedUser = SetDropDownValue(Driver.FindElement(By.Id("UserID")), shiftPreference.UserId.ToString());
                if (navigated || changedUser)
                {
                    WaitTillAvailable(By.CssSelector(String.Format(".{0}", Enum.GetName(typeof(DayOfWeek), shiftPreference.Day))), TimeSpan.FromSeconds(2));
                }
                var changedDuration = SetDropDownValue(Driver.FindElement(By.Id("ShiftDuration")), ((double)shiftPreference.Duration).ToString());
                if (!changedDuration && GetDropDownValue(Driver.FindElement(By.Id("ShiftDuration"))) != ((double)shiftPreference.Duration).ToString())
                {//if changedDuration, then save loop up value operation.
                    SetDurationDropDownValue(((double)shiftPreference.Duration).ToString());
                }
                var dropBy = By.CssSelector(String.Format(".{0} .open[s='{1}']", Enum.GetName(typeof(DayOfWeek), shiftPreference.Day), shiftPreference.StartTime.TotalHours.ToString()));
                if (!ElementExists(dropBy))
                {
                    return false;
                }
                WaitTillAvailable(By.CssSelector("#Preferences preference"));
                var preferenceElement = Driver.FindElement(By.CssSelector("preference[preferenceid='" + shiftPreference.PreferenceId + "']"));
                var dropElement = Driver.FindElement(dropBy);
                var success = DragElement(preferenceElement, dropElement);
                //var action = new OpenQA.Selenium.Interactions.Actions(Driver);
                //action.DragAndDrop(preferenceElement, dropElement);
                //action.Perform();
                var droppedBy = By.CssSelector(String.Format(".{0} .taken[s='{1}']", Enum.GetName(typeof(DayOfWeek), shiftPreference.Day), shiftPreference.StartTime.TotalHours));

                WaitForElement(new List<ByExists> { new ByExists { by = droppedBy, exists = true }, new ByExists { by = By.CssSelector("#Errors ul li"), exists = true } }, TimeSpan.FromSeconds(2));
                if (ElementExists(droppedBy))
                {
                    var droppedElement = Driver.FindElement(droppedBy);
                    shiftPreference.Id = Int32.Parse(droppedElement.GetAttribute("shiftpreferenceid"));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return false;
        }
        //public bool CreateShiftPreference(String VersionName, ShiftCaptain.Models.ShiftPreference shiftPreference)
        //{
        //    try
        //    {
        //        var navigated = GoToPage(VersionName, "ShiftPreference");
        //        bool changedVersion = SetDropDownValue(Driver.FindElement(By.Id("VersionId")), shiftPreference.VersionId.ToString());
        //        if (changedVersion)
        //        {   
        //            WaitTillAvailable(By.Id("Preferences"));
        //        }
        //        bool changedUser = SetDropDownValue(Driver.FindElement(By.Id("UserID")), shiftPreference.UserId.ToString());
        //        if (navigated || changedUser)
        //        {
        //            WaitTillAvailable(By.CssSelector(String.Format(".{0}", Enum.GetName(typeof(DayOfWeek), shiftPreference.Day))), TimeSpan.FromSeconds(2));
        //        }
        //        var changedDuration = SetDropDownValue(Driver.FindElement(By.Id("ShiftDuration")), ((double)shiftPreference.Duration).ToString());
        //        if (!changedDuration && GetDropDownValue(Driver.FindElement(By.Id("ShiftDuration"))) != ((double)shiftPreference.Duration).ToString())
        //        {//if changedDuration, then save loop up value operation.
        //            SetDurationDropDownValue(((double)shiftPreference.Duration).ToString());
        //        }
        //        var dropBy = By.CssSelector(String.Format(".{0} .open[s='{1}']", Enum.GetName(typeof(DayOfWeek), shiftPreference.Day), shiftPreference.StartTime.TotalHours.ToString()));
        //        if (!ElementExists(dropBy))
        //        {
        //            return false;
        //        }
        //        WaitTillAvailable(By.CssSelector("#Preferences preference"));
        //        var preferenceElement = Driver.FindElement(By.CssSelector("preference[preferenceid='" + shiftPreference.PreferenceId + "']"));
        //        var dropElement = Driver.FindElement(dropBy);
        //        var action = new OpenQA.Selenium.Interactions.Actions(Driver);
        //        action.DragAndDrop(preferenceElement, dropElement);
        //        action.Perform();
        //        var droppedBy = By.CssSelector(String.Format(".{0} .taken[s='{1}']", Enum.GetName(typeof(DayOfWeek), shiftPreference.Day), shiftPreference.StartTime.TotalHours));
                
        //        WaitForElement(new List<ByExists> { new ByExists { by = droppedBy, exists = true }, new ByExists { by = By.CssSelector("#Errors ul li"), exists = true } }, TimeSpan.FromSeconds(2));
        //        if (ElementExists(droppedBy))
        //        {
        //            var droppedElement = Driver.FindElement(droppedBy);
        //            shiftPreference.Id = Int32.Parse(droppedElement.GetAttribute("shiftpreferenceid"));
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex.Message, ex);
        //    }
        //    return false;
        //}

        public bool EditShiftPreference(String VersionName, ShiftCaptain.Models.ShiftPreference shiftPreference)
        {
            GoToPage(VersionName, "ShiftPreference");
            var dragBy = By.CssSelector(String.Format(".taken[shiftpreferenceid='{0}']", shiftPreference.Id));
            var dropBy = By.CssSelector(String.Format(".{0} .open[s='{1}']", Enum.GetName(typeof(DayOfWeek), shiftPreference.Day), shiftPreference.StartTime.TotalHours.ToString()));
            WaitTillAvailable(dragBy, TimeSpan.FromSeconds(2));
            if (!ElementExists(dragBy) || !ElementExists(dropBy))
            {
                return false;
            }
            var changedDuration = SetDropDownValue(Driver.FindElement(By.Id("ShiftDuration")), ((double)shiftPreference.Duration).ToString());
            if (!changedDuration && GetDropDownValue(Driver.FindElement(By.Id("ShiftDuration"))) != ((double)shiftPreference.Duration).ToString())
            {//if changedDuration, then save loop up value operation.
                SetDurationDropDownValue(((double)shiftPreference.Duration).ToString());
            }
            Assert.AreEqual(GetDropDownValue(Driver.FindElement(By.Id("ShiftDuration"))), ((double)shiftPreference.Duration).ToString(), "Duration not selected");
            //var action = new OpenQA.Selenium.Interactions.Actions(Driver);
            //action.DragAndDrop(Driver.FindElement(dragBy), Driver.FindElement(dropBy));
            //action.Perform();
            var success = DragElement(Driver.FindElement(dragBy), Driver.FindElement(dropBy));

            var droppedBy = By.CssSelector(String.Format(".{0} .taken[shiftpreferenceid='{1}']", Enum.GetName(typeof(DayOfWeek), shiftPreference.Day), shiftPreference.Id));
            WaitTillAvailable(droppedBy, TimeSpan.FromSeconds(2));
            if (ElementExists(droppedBy))
            {
                return true;
            }
            return false;

        }


        public bool RemoveShiftPreferenceWithUI(String VersionName, ShiftCaptain.Models.ShiftPreference shiftPreference)
        {
            GoToPage(VersionName, "ShiftPreference");
            var dragBy = By.CssSelector(String.Format(".taken[shiftpreferenceid='{0}']", shiftPreference.Id));
            var dropBy = By.CssSelector("#Preferences");
            WaitTillAvailable(dragBy, TimeSpan.FromSeconds(2));
            if (!ElementExists(dragBy) || !ElementExists(dropBy))
            {
                return false;
            }
            var action = new OpenQA.Selenium.Interactions.Actions(Driver);
            action.DragAndDrop(Driver.FindElement(dragBy), Driver.FindElement(dropBy));
            action.Perform();
            var droppedBy = By.CssSelector(String.Format(".{0} .taken[shiftpreferenceid='{1}']", Enum.GetName(typeof(DayOfWeek), shiftPreference.Day), shiftPreference.Id));
            WaitTillNotAvailable(droppedBy, TimeSpan.FromSeconds(2));
            if (!ElementExists(droppedBy))
            {
                return true;
            }
            return false;

        }
        //public void RemoveShiftPreference(String Name)
        //{
        //    RemoveShiftPreference(db.ShiftPreferences.FirstOrDefault(sp => sp.Name == Name));
        //}

        public void RemoveShiftPreference(int id)
        {
            RemoveShiftPreference(db.ShiftPreferences.FirstOrDefault(sp => sp.Id == id));

        }
        public void RemoveShiftPreference(ShiftCaptain.Models.ShiftPreference ShiftPreference)
        {
            if (ShiftPreference != null)
            {
                db.ShiftPreferences.Remove(ShiftPreference);
                db.SaveChanges();
            }
        }

        public bool CompareShiftPreference(ShiftCaptain.Models.ShiftPreference v1, ShiftCaptain.Models.ShiftPreference v2)
        {
            var invalidNames = new string[] { "Id", "Preference", "User", "Version", "VersionId" };
            var type = v1.GetType();
            foreach (System.Reflection.PropertyInfo propertyInfo in type.GetProperties())
            {
                if (propertyInfo.CanRead && !invalidNames.Contains(propertyInfo.Name))
                {
                    var value1 = propertyInfo.GetValue(v1);
                    var value2 = propertyInfo.GetValue(v2);
                    //if (value1 != value2 && (value1 != null && value2 != null && value1.ToString() != value2.ToString()))
                    //{
                    //    if (!(propertyInfo.PropertyType == typeof(Decimal) && (Decimal) value1 != (Decimal) value2))
                    //    {
                    //        return false;
                    //    }
                    //    else
                    //    {
                    //    }
                    //}
                    if (((value1 == null || value2 == null) && value1 != value2) || (value1 != null && value2 != null && !value1.Equals(value2)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        #endregion
        
        #region Shift

        public bool CreateShift(String VersionName, ShiftCaptain.Models.Shift shift, int BuildingId)
        {
            try
            {
                var navigated = GoToPage(VersionName, "Shift");
                bool changedVersion = SetDropDownValue(Driver.FindElement(By.Id("VersionId")), shift.VersionId.ToString());
                if (changedVersion)
                {
                    WaitTillAvailable(By.Id("BuildingID"), TimeSpan.FromSeconds(2));
                }
                Assert.AreEqual(GetDropDownValue(Driver.FindElement(By.Id("VersionId"))), shift.VersionId.ToString(), "Version not selected");
                var changedDuration = SetDropDownValue(Driver.FindElement(By.Id("ShiftDuration")), ((double)shift.Duration).ToString());
                if (!changedDuration && GetDropDownValue(Driver.FindElement(By.Id("ShiftDuration"))) != ((double)shift.Duration).ToString())
                {//if changedDuration, then save loop up value operation.
                    SetDurationDropDownValue(((double)shift.Duration).ToString());
                }
                Assert.AreEqual(GetDropDownValue(Driver.FindElement(By.Id("ShiftDuration"))), ((double)shift.Duration).ToString(), "Duration not selected");
                bool changedBuilding = SetDropDownValue(Driver.FindElement(By.Id("BuildingID")), BuildingId.ToString());
                if (changedBuilding)
                {
                    WaitTillAvailable(By.Id("RoomID"), TimeSpan.FromSeconds(2));
                }
                Assert.AreEqual(GetDropDownValue(Driver.FindElement(By.Id("BuildingID"))), BuildingId.ToString(), "Building not selected");
                bool changedRoom = SetDropDownValue(Driver.FindElement(By.Id("RoomID")), shift.RoomId.ToString());
                if (navigated || changedRoom)
                {
                    WaitTillAvailable(By.CssSelector(String.Format(".{0}", Enum.GetName(typeof(DayOfWeek), shift.Day))), TimeSpan.FromSeconds(2));
                }
                Assert.AreEqual(GetDropDownValue(Driver.FindElement(By.Id("RoomID"))), shift.RoomId.ToString(), "Room not selected");
                WaitTillAvailable(By.CssSelector("#Employees user"));
                var shiftElement = Driver.FindElement(By.CssSelector("user[userid='" + shift.UserId + "']"));
                var dropElement = GetDropElement(shift.Day, (double)shift.StartTime.TotalHours, (double)shift.Duration);
                if (dropElement == null)
                {
                    return false;
                }
                //var action = new OpenQA.Selenium.Interactions.Actions(Driver);
                //action.DragAndDrop(preferenceElement, dropElement);
                //action.Perform();
                var success = DragElement(shiftElement, dropElement);
                
                var droppedBy = By.CssSelector(String.Format(".{0} .taken[s='{1}'][userid='{2}']", Enum.GetName(typeof(DayOfWeek), shift.Day), shift.StartTime.TotalHours.ToString(), shift.UserId));

                WaitForElement(new List<ByExists> { new ByExists { by = droppedBy, exists = true }, new ByExists { by = By.CssSelector("#Errors ul li"), exists = true } }, TimeSpan.FromSeconds(2));
                if (ElementExists(By.CssSelector("#Errors ul li")))
                {
                    GoToPage("Shift", true);
                } else if (ElementExists(droppedBy))
                {
                    var droppedElement = Driver.FindElement(droppedBy);
                    shift.Id = Int32.Parse(droppedElement.GetAttribute("shiftid"));
                    return true;
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }
        public IWebElement GetDropElement(int day, double totalHours, double duration)
        {
            var selector = By.CssSelector(String.Format(".{0} .open[s='{1}']", Enum.GetName(typeof(DayOfWeek), day), totalHours.ToString()));
            if (!ElementExists(selector))
            {
                return null;
            }
            var start = totalHours + .5;
            var end = totalHours + duration;
            foreach (var element in Driver.FindElements(selector))
            {
                var valid = true;
                for (var idx = start; idx < end; idx += .5)
                {
                    if (!ElementExists(element, By.XPath(String.Format("following-sibling::td[contains(@class,'open')][@s='{0}']", (idx % 24).ToString()))))
                    {
                        valid = false;
                        break;
                    }

                }
                if (valid)
                {
                    return element;
                }
            }
            return null;

        }
        public bool EditShift(String VersionName, ShiftCaptain.Models.Shift shift)
        {
            GoToPage(VersionName, "Shift");
            var dragBy = By.CssSelector(String.Format(".taken[shiftid='{0}']", shift.Id));
            WaitTillAvailable(dragBy, TimeSpan.FromSeconds(2));
            if (!ElementExists(dragBy))
            {
                return false;
            }
            var changedDuration = SetDropDownValue(Driver.FindElement(By.Id("ShiftDuration")), ((double)shift.Duration).ToString());
            if (!changedDuration && GetDropDownValue(Driver.FindElement(By.Id("ShiftDuration"))) != ((double)shift.Duration).ToString())
            {//if changedDuration, then save loop up value operation.
                SetDurationDropDownValue(((double)shift.Duration).ToString());
            }
            var dropElement = GetDropElement(shift.Day, (double)shift.StartTime.TotalHours, (double)shift.Duration);
            if (dropElement == null)
            {
                return false;
            }
            //var action = new OpenQA.Selenium.Interactions.Actions(Driver);
            //action.DragAndDrop(Driver.FindElement(dragBy), dropElement);
            //action.Perform();
            var success = DragElement(Driver.FindElement(dragBy), dropElement);

            var droppedBy = By.CssSelector(String.Format(".{0} .taken[shiftid='{1}']", Enum.GetName(typeof(DayOfWeek), shift.Day), shift.Id));
            WaitTillAvailable(droppedBy, TimeSpan.FromSeconds(2));
            if (ElementExists(droppedBy))
            {
                return true;
            }
            return false;

        }


        public bool RemoveShiftWithUI(String VersionName, ShiftCaptain.Models.Shift shift)
        {
            GoToPage(VersionName, "Shift");
            var dragBy = By.CssSelector(String.Format(".taken[shiftid='{0}']", shift.Id));
            var dropBy = By.Id("Employees");
            WaitTillAvailable(dragBy, TimeSpan.FromSeconds(2));
            if (!ElementExists(dragBy) || !ElementExists(dropBy))
            {
                return false;
            }
            var action = new OpenQA.Selenium.Interactions.Actions(Driver);
            action.DragAndDrop(Driver.FindElement(dragBy), Driver.FindElement(dropBy));
            action.Perform();
            var droppedBy = By.CssSelector(String.Format(".{0} .taken[shiftid='{1}']", Enum.GetName(typeof(DayOfWeek), shift.Day), shift.Id));
            WaitTillNotAvailable(droppedBy, TimeSpan.FromSeconds(2));
            if (!ElementExists(droppedBy))
            {
                return true;
            }
            return false;

        }

        public void RemoveDefaultShifts(String VersionName)
        {
            try
            {
                foreach (var shift in db.Shifts.Where(s => s.Version.Name == VersionName).ToList())
                {
                    db.Shifts.Remove(shift);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
        public void RemoveShift(int id)
        {
            RemoveShift(db.Shifts.FirstOrDefault(sp => sp.Id == id));

        }
        public void RemoveShift(ShiftCaptain.Models.Shift Shift)
        {
            if (Shift != null)
            {
                db.Shifts.Remove(Shift);
                db.SaveChanges();
            }
        }

        public bool CompareShift(ShiftCaptain.Models.Shift v1, ShiftCaptain.Models.Shift v2)
        {
            var invalidNames = new string[] { "Id", "Room", "User", "Version", "VersionId" };
            var type = v1.GetType();
            foreach (System.Reflection.PropertyInfo propertyInfo in type.GetProperties())
            {
                if (propertyInfo.CanRead && !invalidNames.Contains(propertyInfo.Name))
                {
                    var value1 = propertyInfo.GetValue(v1);
                    var value2 = propertyInfo.GetValue(v2);
                    //if (value1 != value2 && (value1 != null && value2 != null && value1.ToString() != value2.ToString()))
                    //{
                    //    if (!(propertyInfo.PropertyType == typeof(Decimal) && (Decimal) value1 != (Decimal) value2))
                    //    {
                    //        return false;
                    //    }
                    //    else
                    //    {
                    //    }
                    //}
                    if (((value1 == null || value2 == null) && value1 != value2) || (value1 != null && value2 != null && !value1.Equals(value2)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        #endregion

        #region Manage Schedule

        public bool SubmitForApproval(ShiftCaptain.Models.Version version)
        {
            GoToPage(version.Name, "ValidateSchedule");
            var by = GetSubmitButton();
            var element = Driver.FindElement(by);
            var disabled = element.GetAttribute("disabled");
            if (disabled == "disabled")
            {
                return false;
            }
            ClickAndWaitForElements(element, new List<ByExists> { new ByExists { by = by, exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });

            db = new ShiftCaptainEntities();//Not sure why but the version isn't updated in this db instance.
            if (db.Versions.FirstOrDefault(v => v.Id == version.Id).IsReadyForApproval)
            {
                return true;
            }
            return false;
        }

        public bool RejectSchedule(ShiftCaptain.Models.Version version)
        {
            GoToPage(version.Name, "ApproveSchedule");
            var by = By.Id("RejectSchedule");
            var element = Driver.FindElement(by);
            var disabled = element.GetAttribute("disabled");
            if (disabled == "disabled")
            {
                return false;
            }
            ClickAndWaitForElements(element, new List<ByExists> { new ByExists { by = by, exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });

            db = new ShiftCaptainEntities();//Not sure why but the version isn't updated in this db instance.
            version = db.Versions.FirstOrDefault(v => v.Id == version.Id);
            if (version.IsReadyForApproval == false && version.IsApproved == false)
            {
                return true;
            }
            return false;
        }


        public bool ApproveSchedule(ShiftCaptain.Models.Version version)
        {
            GoToPage(version.Name, "ApproveSchedule");
            var by = GetSubmitButton();
            var element = Driver.FindElement(by);
            var disabled = element.GetAttribute("disabled");
            if (disabled == "disabled")
            {
                return false;
            }
            ClickAndWaitForElements(element, new List<ByExists> { new ByExists { by = by, exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });

            db = new ShiftCaptainEntities();//Not sure why but the version isn't updated in this db instance.
            if (db.Versions.FirstOrDefault(v => v.Id == version.Id).IsApproved)
            {
                return true;
            }
            return false;
        }

        public bool DisapproveSchedule(ShiftCaptain.Models.Version version)
        {
            GoToPage(version.Name, "DisapproveSchedule");
            var by = GetSubmitButton();
            var element = Driver.FindElement(by);
            var disabled = element.GetAttribute("disabled");
            if (disabled == "disabled")
            {
                return false;
            }
            ClickAndWaitForElements(element, new List<ByExists> { new ByExists { by = by, exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });

            db = new ShiftCaptainEntities();//Not sure why but the version isn't updated in this db instance.
            version = db.Versions.FirstOrDefault(v => v.Id == version.Id);
            if (!version.IsApproved && !version.IsReadyForApproval)
            {
                return true;
            }
            return false;
        }

        public bool CloneSchedule(String VersionName, String CloneName, IQueryable<UserView> CloneUsers, IQueryable<RoomView> CloneRooms, out ShiftCaptain.Models.Version version)
        {
            version = null;
            GoToPage(VersionName, "CloneSchedule");
            SetTextBoxValue(Driver.FindElement(By.Id("Version_Name")), CloneName);
            foreach (var user in CloneUsers)
            {
                Driver.FindElement(By.CssSelector("#users option[value='" + user.UserId.ToString() + "']")).Click();
            }
            Driver.FindElement(By.Id("btnCloneUser")).Click();
            foreach (var room in CloneRooms)
            {
                Driver.FindElement(By.CssSelector("#rooms option[value='" + room.RoomId.ToString() + "']")).Click();
            }
            Driver.FindElement(By.Id("btnCloneRoom")).Click();

            var by = GetSubmitButton();
            var element = Driver.FindElement(by);
            var disabled = element.GetAttribute("disabled");
            if (disabled == "disabled")
            {
                return false;
            }
            ClickAndWaitForElements(element, new List<ByExists> { new ByExists { by = by, exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });
            if (ElementExists(By.CssSelector(".validation-summary-errors")))
            {
                return false;
            }
            db = new ShiftCaptainEntities();//Not sure why but the version isn't updated in this db instance.
            version = db.Versions.FirstOrDefault(v => v.Name == CloneName);
            if (version != null)
            {
                var newId = version.Id;
                var old_version = db.Versions.FirstOrDefault(v => v.Name == VersionName);
                var new_users = db.UserViews.Where(uv => uv.VersionId == newId);
                Assert.AreEqual(new_users.Count(), CloneUsers.Count(), "Did not clone the right number of users");
                foreach (var user in new_users)
                {
                    var old_user = db.UserViews.FirstOrDefault(uv => uv.UserId == user.UserId && uv.VersionId == old_version.Id);
                    Assert.IsNotNull(old_user, String.Format("Could not find old user {0}", user.NickName));
                    Assert.IsTrue(CompareUser(user, old_user), "Cloned user does not match");
                }

                var new_rooms = db.RoomViews.Where(rv => rv.VersionId == newId);
                Assert.AreEqual(new_rooms.Count(), CloneRooms.Count(), "Did not clone the right number of rooms");
                foreach (var room in new_rooms)
                {
                    var old_room = db.RoomViews.FirstOrDefault(rv => rv.RoomId == room.RoomId && rv.VersionId == old_version.Id);
                    Assert.IsNotNull(old_room, String.Format("Could not find old room {0}", room.Name));
                    Assert.IsTrue(CompareRoom(room, old_room), "Cloned room does not match");
                }

                var new_shiftPreferences = db.ShiftPreferences.Where(sp => sp.VersionId == newId);
                var old_shiftPreferences = db.ShiftPreferences.Where(sp => sp.VersionId == old_version.Id);
                Assert.AreEqual(new_shiftPreferences.Count(), old_shiftPreferences.Count(), "Did not clone the right number of shift preferences");
                foreach (var shiftPreference in new_shiftPreferences)
                {
                    var shiftPreference_clone = old_shiftPreferences.FirstOrDefault(sp => sp.UserId == shiftPreference.UserId && sp.PreferenceId == shiftPreference.PreferenceId && sp.StartTime == shiftPreference.StartTime && sp.Duration == shiftPreference.Duration && sp.Day == shiftPreference.Day);
                    Assert.IsNotNull(shiftPreference, String.Format("Could not find old shift Preference {0}", shiftPreference.Id));
                    Assert.IsTrue(CompareShiftPreference(shiftPreference, shiftPreference_clone), "Cloned shift preference does not match");
                }

                var new_shifts = db.Shifts.Where(s => s.VersionId == newId);
                var old_shifts = db.Shifts.Where(s => s.VersionId == old_version.Id);
                Assert.AreEqual(new_shifts.Count(), old_shifts.Count(), "Did not clone the right number of shifts");
                foreach (var shift in new_shifts)
                {
                    var shift_clone = old_shifts.FirstOrDefault(s => s.UserId == shift.UserId && s.RoomId == shift.RoomId && s.StartTime == shift.StartTime && s.Duration == shift.Duration && s.Day == shift.Day);
                    Assert.IsNotNull(shift, String.Format("Could not find old shift {0}", shift.Id));
                    Assert.IsTrue(CompareShift(shift, shift_clone), "Cloned shift does not match");
                }
                return true;
            }
            
            return false;
        }
        #endregion

        public void SetTextBoxValues(object item)
        {
            var type = item.GetType();
            foreach (System.Reflection.PropertyInfo propertyInfo in type.GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    var by = By.Id(propertyInfo.Name);
                    var value = propertyInfo.GetValue(item);
                    if (value != null && ElementExists(by) && Driver.FindElement(by).Displayed)
                    {
                        if (value.GetType() == typeof(string) || value.GetType() == typeof(Decimal))
                        {
                            SetTextBoxValue(Driver.FindElement(by), value.ToString());
                        }
                        else if (value.GetType() == typeof(bool))
                        {
                            var element = Driver.FindElement(by);
                            if (element.Selected != (bool)value)
                            {
                                element.Click();
                            }
                        }
                        else if (value.GetType() == typeof(Decimal?) )
                        {
                            string val = ((Decimal?)value).HasValue ? ((Decimal?)value).Value.ToString() : "0";
                            SetTextBoxValue(Driver.FindElement(by), val);
                        }
                        else if (value.GetType() == typeof(TimeSpan) || (value.GetType() == typeof(TimeSpan?) && ((TimeSpan?)value).HasValue))
                        {
                            SetTextBoxValue(Driver.FindElement(by), ((TimeSpan)value).ToString("hh\\:mm"));
                        }
                    }
                }
            }
        }

        private bool IsDropDown(IWebElement element)
        {
            return element.TagName == "select";
        }
        public void SetDropDownValues(object item)
        {
            var type = item.GetType();
            foreach (System.Reflection.PropertyInfo propertyInfo in type.GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    var by = By.Id(propertyInfo.Name);
                    var value = propertyInfo.GetValue(item);
                    if (value != null && ElementExists(by) && Driver.FindElement(by).Displayed && IsDropDown(Driver.FindElement(by)))
                    {
                        if (value.GetType() == typeof(int))
                        {
                            SetDropDownValue(Driver.FindElement(by), value.ToString());
                        }
                    }
                }
            }
        }

        public void SetTextBoxValue<T>(T item) where T : class
        {
            var property = typeof(T).GetProperties()[0];
            if (property.GetValue(item) != null)
            {
                Driver.FindElement(By.Id(property.Name)).SendKeys(property.GetValue(item).ToString());
            }
        }
    }
}
