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

        public bool GoToPage(string page, bool force = false)
        {
            if (!force && (BaseUrl + page == Driver.Url || BaseUrl + page + "/" == Driver.Url))
            {
                return false;
            }
            switch (page)
            {
                default:
                    _goToPage(page);
                    break;
            }
            return true;
        }

        private void _goToPage(string page, string pageTitle = null)
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
            if (currentUrl != BaseUrl + page)
            {
                WaitForNextPage(currentUrl);
            }
            if (pageTitle != null)
            {
                var pageTitleText = Driver.FindElement(By.Id("divPageTitle")).Text;
                Assert.AreEqual(pageTitle, pageTitleText);
            }
            else
            {
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
                var user = db.Users.First(u => u.EmailAddress == defaultEmail);
                if (user == null)
                {
                    throw new Exception(String.Format("User {0} not found", defaultEmail));
                }
                email = user.EmailAddress;
                password = user.Pass;
            }
            Driver.FindElement(By.Id("EmailAddress")).SendKeys(email);
            Driver.FindElement(By.Id("Pass")).SendKeys(password);
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("#loginForm form input[type='submit']")), new List<ByExists> { new ByExists { by = By.Id("EmailAddress"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true } });
            return !ElementExists(By.Id("EmailAddress"));
        }
        #region User 
        
        public bool CreateUser(UserView user)
        {
            GoToPage("User/Create");
            SetTextBoxValues(user);
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("form[action='/User/Create'] input[type='submit']")), new List<ByExists> { new ByExists { by = By.Id("EmailAddress"), exists = false}, new ByExists{ by = By.CssSelector(".validation-summary-errors"), exists = true}, new ByExists{ by = By.CssSelector(".field-validation-error"), exists = true}});
            return !Driver.Url.Contains("User/Create");
        }

        public ShiftCaptain.Models.UserView CreateDefaultUser(String NickName, int? VersionId, String VersionName)
        {
            var ticks = DateTime.Now.Ticks.ToString();
            var counter = 0;

            if (db.UserViews.Count(uv => uv.NickName == NickName && uv.VersionId == VersionId) == 0)
            {
                var createTable = new DataParser("Users.csv").Tables["DefaultUser"];
                foreach (var userObj in createTable)
                {
                    
                    var user= new ShiftCaptain.Models.User
                    {
                        EmailAddress = Clean<string>(userObj, "EMAIL_ADDRESS") ?? String.Format("{0}-{1}@emails.com", ticks, ++counter),
                        Pass = Clean<string>(userObj, "PASS"),
                        FName = Clean<string>(userObj, "FIRST_NAME"),
                        LName = Clean<string>(userObj, "LAST_NAME"),
                        NickName = Clean<string>(userObj, "NICK_NAME"),
                        EmployeeId = Clean<string>(userObj, "EMPLOYEE_ID"),
                        PhoneNumber = Clean<string>(userObj, "PHONE_NUMBER"),
                        IsManager = Clean<bool>(userObj, "IS_MANAGER"),
                        IsShiftManager = Clean<bool>(userObj, "IS_SHIFT_MANAGER"),
                        IsActive = Clean<bool>(userObj, "IS_ACTIVE"),
                        IsMale = Clean<bool>(userObj, "IS_MALE")
                    };
                    if (user.NickName == NickName && Clean<string>(userObj, "VERSION_NAME") == VersionName)
                    {
                        if (db.UserViews.Count(uv => uv.NickName == NickName) == 0)
                        {
                            var address = CreateAddress(userObj);
                            
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

        public void RemoveDefaultUsers()
        {
            try
            {
                var createTable = new DataParser("Users.csv").Tables["DefaultUser"];
                foreach (var userObj in createTable)
                {
                    if (!Clean<bool>(userObj, "CREATED"))
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
            }
        }
        
        public bool EditUser(UserView user)
        {
            GoToPage("User/Edit/"+  user.UserId);
            SetTextBoxValues(user);
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("form[action='/User/Edit/" + +user.UserId + "'] input[type='submit']")), new List<ByExists> { new ByExists { by = By.Id("EmailAddress"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });
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
            if (db.Versions.Count(v => v.Name == VersionName) == 0)
            {
                var createTable = new DataParser("Versions.csv").Tables["DefaultVersion"];
                foreach (var versionObj in createTable)
                {
                    var version = new ShiftCaptain.Models.Version
                    {
                        Name = Clean<string>(versionObj, "NAME"),
                        IsVisible = Clean<bool>(versionObj, "IS_VISIBLE"),
                        IsActive = Clean<bool>(versionObj, "IS_ACTIVE")
                    };
                    if (version.Name == VersionName)
                    {
                        db.Versions.Add(version);
                        db.SaveChanges();
                        break;
                    }
                }
            }
            return db.Versions.FirstOrDefault(v => v.Name == VersionName);
        }
        public void RemoveDefaultVersions()
        {
            try
            {
                var createTable = new DataParser("Versions.csv").Tables["DefaultVersion"];
                foreach (var versionObj in createTable)
                {
                    var name = Clean<string>(versionObj, "NAME");
                    var version = db.Versions.FirstOrDefault(v => v.Name == name);
                    if (version != null)
                    {
                        db.Versions.Remove(version);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        public bool CreateVersion(ShiftCaptain.Models.Version version)
        {
            GoToPage("Version/Create");
            SetTextBoxValues(version);
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("form[action='/Version/Create'] input[type='submit']")), new List<ByExists> { new ByExists { by = By.Id("EmailAddress"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });
            return !Driver.Url.Contains("Version/Create");
        }

        public bool EditVersion(ShiftCaptain.Models.Version version)
        {
            GoToPage("Version/Edit/" + version.Id);
            SetTextBoxValues(version);
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("form[action='/Version/Edit/" + version.Id + "'] input[type='submit']")), new List<ByExists> { new ByExists { by = By.Id("EmailAddress"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });
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
        
        public ShiftCaptain.Models.Address CreateAddress(IDictionary<string, string> obj)
        {
            if (Clean<string>(obj, "LINE_1") != null)
            {
                var address = new ShiftCaptain.Models.Address
                {
                    Line1 = Clean<string>(obj, "LINE_1"),
                    Line2 = Clean<string>(obj, "LINE_2"),
                    City = Clean<string>(obj, "CITY"),
                    State = Clean<string>(obj, "STATE"),
                    ZipCode = Clean<string>(obj, "ZIP_CODE"),
                    Country = Clean<string>(obj, "COUNTRY")
                };

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
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("form[action='/Building/Create'] input[type='submit']")), new List<ByExists> { new ByExists { by = By.Id("EmailAddress"), exists = false }, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true }, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true } });
            return !Driver.Url.Contains("Building/Create");
        }
        public ShiftCaptain.Models.Building CreateDefaultBuilding(String BuildingName)
        {
            if (db.Buildings.Count(b => b.Name == BuildingName) == 0)
            {
                var createTable = new DataParser("Buildings.csv").Tables["DefaultBuilding"];
                foreach (var buildingObj in createTable)
                {
                    var building = new ShiftCaptain.Models.Building
                    {
                        Name = Clean<string>(buildingObj, "NAME"),
                        ManagerPhone = Clean<string>(buildingObj, "MANAGER_PHONE"),
                        PhoneNumber = Clean<string>(buildingObj, "PHONE_NUMBER")
                    };
                    if (building.Name == BuildingName)
                    {
                        var address = CreateAddress(buildingObj);
                        if (address != null)
                        {
                            building.AddressId = address.Id;
                        }
                        db.Buildings.Add(building);
                        db.SaveChanges();
                        
                        break;
                    }
                }
            }
            return db.Buildings.FirstOrDefault(v => v.Name == BuildingName);
        }
        public void RemoveDefaultBuildings()
        {
            try
            {
                var createTable = new DataParser("Buildings.csv").Tables["DefaultBuilding"];
                foreach (var buildingObj in createTable)
                {
                    var name = Clean<string>(buildingObj, "NAME");
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
            catch (Exception ex)
            {
            }
        }

        public bool EditBuilding(BuildingView Building)
        {
            GoToPage("Building/Edit/" + Building.BuildingId);
            SetTextBoxValues(Building);
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("form[action='/Building/Edit/" + +Building.BuildingId + "'] input[type='submit']")), new List<ByExists> { new ByExists { by = By.Id("EmailAddress"), exists = false}, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true}, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true}});
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
        public bool CreateRoom(RoomView Room)
        {
            GoToPage("Room/Create");
            SetTextBoxValues(Room);
            SetDropDownValues(Room);
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("form[action='/Room/Create'] input[type='submit']")), new List<ByExists> { new ByExists { by = By.Id("EmailAddress"), exists = false}, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true}, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true}});
            return !Driver.Url.Contains("Room/Create");
        }

        public ShiftCaptain.Models.RoomView CreateDefaultRoom(String RoomName, int VersionId)
        {
            if (db.RoomViews.Count(r => r.Name == RoomName && r.VersionId == VersionId) == 0)
            {
                var createTable = new DataParser("Rooms.csv").Tables["DefaultRoom"];
                foreach (var roomObj in createTable)
                {
                    if (Clean<string>(roomObj, "NAME") == RoomName)
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
                            foreach (var roomHour in GetRoomHours(roomObj, roomInstance))
                            {
                                db.RoomHours.Add(roomHour);
                                db.SaveChanges();
                            }
                        }
                        break;
                    }
                }
            }
            return db.RoomViews.FirstOrDefault(rv => rv.Name == RoomName && rv.VersionId == VersionId);
        }

        public ShiftCaptain.Models.RoomView CreateDefaultRoom(int VersionId, String VersionName)
        {
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
                            foreach (var roomHour in GetRoomHours(roomObj, roomInstance))
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

        public void RemoveDefaultRooms()
        {
            try
            {
                var createTable = new DataParser("Rooms.csv").Tables["DefaultRoom"];
                foreach (var roomObj in createTable)
                {
                    var name = Clean<string>(roomObj, "NAME");
                    foreach(var room in db.Rooms.Where(r => r.Name == name).ToList()){
                        RemoveRoom(room.Id);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public bool EditRoom(RoomView Room)
        {
            GoToPage("Room/Edit/" + Room.RoomId);
            SetTextBoxValues(Room);
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("form[action='/Room/Edit/" + +Room.RoomId + "'] input[type='submit']")), new List<ByExists> { new ByExists { by = By.Id("EmailAddress"), exists = false}, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true}, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true}});
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
            if (db.Preferences.Count(p => p.Name == PreferenceName) == 0)
            {
                var createTable = new DataParser("Preferences.csv").Tables["DefaultPreference"];
                foreach (var preferenceObj in createTable)
                {
                    var preference = new ShiftCaptain.Models.Preference
                    {
                        Name = Clean<string>(preferenceObj, "NAME"),
                        Description = Clean<string>(preferenceObj, "DESCRIPTION"),
                        CanWork = Clean<bool>(preferenceObj, "CAN_WORK"),
                        Color = Clean<string>(preferenceObj, "COLOR")
                    };
                    if (preference.Name == PreferenceName)
                    {
                        db.Preferences.Add(preference);
                        db.SaveChanges();
                        break;
                    }
                }
            }
            return db.Preferences.FirstOrDefault(p => p.Name == PreferenceName);
        }
        public void RemoveDefaultPreferences()
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
        public bool CreatePreference(ShiftCaptain.Models.Preference preference)
        {
            GoToPage("Preference/Create");
            SetTextBoxValues(preference);
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("form[action='/Preference/Create'] input[type='submit']")), new List<ByExists> { new ByExists { by = By.Id("EmailAddress"), exists = false}, new ByExists { by = By.CssSelector(".validation-summary-errors"), exists = true}, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true}});
            return !Driver.Url.Contains("Preference/Create");
        }

        public bool EditPreference(ShiftCaptain.Models.Preference preference)
        {
            GoToPage("Preference/Edit/" + preference.Id);
            SetTextBoxValues(preference);
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("form[action='/Preference/Edit/" + preference.Id + "'] input[type='submit']")), new List<ByExists> { new ByExists { by = By.Id("EmailAddress"),exists = false}, new ByExists { by =  By.CssSelector(".validation-summary-errors"), exists = true}, new ByExists { by = By.CssSelector(".field-validation-error"), exists = true}});
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
        //public void RemoveDefaultShiftPreferences()
        //{
        //    var createTable = new DataParser("ShiftPreferences.csv").Tables["DefaultShiftPreference"];
        //    foreach (var shiftPreferenceObj in createTable)
        //    {
        //        var name = Clean<string>(shiftPreferenceObj, "NAME");
        //        var shiftPreference = db.ShiftPreferences.FirstOrDefault(sp => sp.Name == name);
        //        if (shiftPreference != null)
        //        {
        //            db.ShiftPreferences.Remove(shiftPreference);
        //            db.SaveChanges();
        //        }
        //    }
        //}

        public bool CreateShiftPreference(ShiftCaptain.Models.ShiftPreference shiftPreference)
        {
            try
            {
                var navigated = GoToPage("ShiftPreference");
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
                SetDropDownValue(Driver.FindElement(By.Id("ShiftDuration")), ((double)shiftPreference.Duration).ToString());
                var dropBy = By.CssSelector(String.Format(".{0} .open[s='{1}']", Enum.GetName(typeof(DayOfWeek), shiftPreference.Day), shiftPreference.StartTime.TotalHours));
                if (!ElementExists(dropBy))
                {
                    return false;
                }
                WaitTillAvailable(By.CssSelector("#Preferences preference"));
                var preferenceElement = Driver.FindElement(By.CssSelector("preference[preferenceid='" + shiftPreference.PreferenceId + "']"));
                var dropElement = Driver.FindElement(dropBy);
                var action = new OpenQA.Selenium.Interactions.Actions(Driver);
                action.DragAndDrop(preferenceElement, dropElement);
                action.Perform();
                var droppedBy = By.CssSelector(String.Format(".{0} .taken[s='{1}']", Enum.GetName(typeof(DayOfWeek), shiftPreference.Day), shiftPreference.StartTime.TotalHours));
                
                WaitForElement(new List<ByExists> { new ByExists { by = droppedBy, exists = true }, new ByExists { by = By.CssSelector("#Errors ul li"), exists = true } }, TimeSpan.FromSeconds(2));
                if (ElementExists(droppedBy))
                {
                    var droppedElement = Driver.FindElement(droppedBy);
                    shiftPreference.Id = Int32.Parse(droppedElement.GetAttribute("shiftpreferenceid"));
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public bool EditShiftPreference(ShiftCaptain.Models.ShiftPreference shiftPreference)
        {
            GoToPage("ShiftPreference");
            var dragBy = By.CssSelector(String.Format(".taken[shiftpreferenceid='{0}']", shiftPreference.Id));
            var dropBy = By.CssSelector(String.Format(".{0} .open[s='{1}']", Enum.GetName(typeof(DayOfWeek), shiftPreference.Day), shiftPreference.StartTime.TotalHours));
            WaitTillAvailable(dragBy, TimeSpan.FromSeconds(2));
            if (!ElementExists(dragBy) || !ElementExists(dropBy))
            {
                return false;
            }
            SetDropDownValue(Driver.FindElement(By.Id("ShiftDuration")), ((double)shiftPreference.Duration).ToString());
            var action = new OpenQA.Selenium.Interactions.Actions(Driver);
            action.DragAndDrop(Driver.FindElement(dragBy), Driver.FindElement(dropBy));
            action.Perform();
            var droppedBy = By.CssSelector(String.Format(".{0} .taken[shiftpreferenceid='{1}']", Enum.GetName(typeof(DayOfWeek), shiftPreference.Day), shiftPreference.Id));
            WaitTillAvailable(droppedBy, TimeSpan.FromSeconds(2));
            if (ElementExists(droppedBy))
            {
                return true;
            }
            return false;

        }


        public bool RemoveShiftPreferenceWithUI(ShiftCaptain.Models.ShiftPreference shiftPreference)
        {
            GoToPage("ShiftPreference");
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
            var invalidNames = new string[] { "Id", "Preference", "User", "Version" };
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

        public bool CreateShift(ShiftCaptain.Models.Shift shift, int BuildingId)
        {
            try
            {
                var navigated = GoToPage("Shift");
                bool changedVersion = SetDropDownValue(Driver.FindElement(By.Id("VersionId")), shift.VersionId.ToString());
                if (changedVersion)
                {
                    WaitTillAvailable(By.Id("BuildingID"), TimeSpan.FromSeconds(2));
                }
                SetDropDownValue(Driver.FindElement(By.Id("ShiftDuration")), ((double)shift.Duration).ToString());
                bool changedBuilding = SetDropDownValue(Driver.FindElement(By.Id("BuildingID")), BuildingId.ToString());
                if (changedBuilding)
                {
                    WaitTillAvailable(By.Id("RoomID"), TimeSpan.FromSeconds(2));
                }
                bool changedRoom = SetDropDownValue(Driver.FindElement(By.Id("RoomID")), shift.RoomId.ToString());
                if (navigated || changedRoom)
                {
                    WaitTillAvailable(By.CssSelector(String.Format(".{0}", Enum.GetName(typeof(DayOfWeek), shift.Day))), TimeSpan.FromSeconds(2));
                }
                var dropBy = By.CssSelector(String.Format(".{0} .open[s='{1}']", Enum.GetName(typeof(DayOfWeek), shift.Day), shift.StartTime.TotalHours));
                if (!ElementExists(dropBy))
                {
                    return false;
                }

                WaitTillAvailable(By.CssSelector("#Employees user"));
                var preferenceElement = Driver.FindElement(By.CssSelector("user[userid='" + shift.UserId + "']"));
                var dropElement = Driver.FindElement(dropBy);
                var action = new OpenQA.Selenium.Interactions.Actions(Driver);
                action.DragAndDrop(preferenceElement, dropElement);
                action.Perform();
                var droppedBy = By.CssSelector(String.Format(".{0} .taken[s='{1}'][userid='{2}']", Enum.GetName(typeof(DayOfWeek), shift.Day), shift.StartTime.TotalHours, shift.UserId));

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

        public bool EditShift(ShiftCaptain.Models.Shift shift)
        {
            GoToPage("Shift");
            var dragBy = By.CssSelector(String.Format(".taken[shiftid='{0}']", shift.Id));
            var dropBy = By.CssSelector(String.Format(".{0} .open[s='{1}']", Enum.GetName(typeof(DayOfWeek), shift.Day), shift.StartTime.TotalHours));
            WaitTillAvailable(dragBy, TimeSpan.FromSeconds(2));
            if (!ElementExists(dragBy) || !ElementExists(dropBy))
            {
                return false;
            }
            SetDropDownValue(Driver.FindElement(By.Id("ShiftDuration")), ((double)shift.Duration).ToString());
            var action = new OpenQA.Selenium.Interactions.Actions(Driver);
            action.DragAndDrop(Driver.FindElement(dragBy), Driver.FindElement(dropBy));
            action.Perform();
            var droppedBy = By.CssSelector(String.Format(".{0} .taken[shiftid='{1}']", Enum.GetName(typeof(DayOfWeek), shift.Day), shift.Id));
            WaitTillAvailable(droppedBy, TimeSpan.FromSeconds(2));
            if (ElementExists(droppedBy))
            {
                return true;
            }
            return false;

        }


        public bool RemoveShiftWithUI(ShiftCaptain.Models.Shift shift)
        {
            GoToPage("Shift");
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
        //public void RemoveShift(String Name)
        //{
        //    RemoveShift(db.Shifts.FirstOrDefault(sp => sp.Name == Name));
        //}

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
            var invalidNames = new string[] { "Id", "Room", "User", "Version" };
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
                        if (value.GetType() == typeof(string))
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
