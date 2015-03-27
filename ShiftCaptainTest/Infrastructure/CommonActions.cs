using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using OpenQA.Selenium;
using System.Linq;
using ShiftCaptain.Models;
using System.Collections.Generic;

namespace ShiftCaptainTest.Infrastructure
{
    public class CommonActions : TestHelpers
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

        public void GoToPage(string page)
        {
            switch (page)
            {
                default:
                    _goToPage(page);
                    break;
            }
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
                var defaultUser = UserTable.FirstOrDefault(u => u["HANDLE"].ToLower() == "defaultuser");
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
            ClickAndWaitForEitherElement(Driver.FindElement(By.CssSelector("#loginForm form input[type='submit']")), By.Id("EmailAddress"), By.CssSelector(".validation-summary-errors"), false, true);
            return !ElementExists(By.Id("EmailAddress"));
        }
        #region User 
        
        public bool CreateUser(UserView user)
        {
            GoToPage("User/Create");
            SetTextBoxValues(user);
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("form[action='/User/Create'] input[type='submit']")), By.Id("EmailAddress"), By.CssSelector(".validation-summary-errors"), By.CssSelector(".field-validation-error"), false, true, true);
            return !Driver.Url.Contains("User/Create");
        }

        public bool EditUser(UserView user)
        {
            GoToPage("User/Edit/"+  user.UserId);
            SetTextBoxValues(user);
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("form[action='/User/Edit/" + +  user.UserId + "'] input[type='submit']")), By.Id("EmailAddress"), By.CssSelector(".validation-summary-errors"), By.CssSelector(".field-validation-error"), false, true, true);
            return !Driver.Url.Contains("User/Edit/");
        }

        public void RemoveUser(String EmailAddress)
        {
            var user = db.Users.FirstOrDefault(u => u.EmailAddress == EmailAddress);
            if (user != null)
            {
                var userview = db.UserViews.FirstOrDefault(uv => uv.EmailAddress == EmailAddress);
                if (userview != null)
                {
                    if (userview.VersionId != null)
                    {
                        foreach (var shiftPreference in db.ShiftPreferences.Where(sp => sp.UserId == userview.UserId))
                        {
                            db.ShiftPreferences.Remove(shiftPreference);
                        }
                        foreach (var shift in db.Shifts.Where(s => s.UserId == userview.UserId))
                        {
                            db.Shifts.Remove(shift);
                        }
                        db.SaveChanges();

                        foreach (var userInstance in db.UserInstances.Where(ui => ui.UserId == userview.UserId))
                        {
                            db.UserInstances.Remove(userInstance);
                        }
                        db.SaveChanges();
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

        public bool CreateVersion(ShiftCaptain.Models.Version version)
        {
            GoToPage("Version/Create");
            SetTextBoxValues(version);
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("form[action='/Version/Create'] input[type='submit']")), By.Id("EmailAddress"), By.CssSelector(".validation-summary-errors"), By.CssSelector(".field-validation-error"), false, true, true);
            return !Driver.Url.Contains("Version/Create");
        }

        public bool EditVersion(ShiftCaptain.Models.Version version)
        {
            GoToPage("Version/Edit/" + version.Id);
            SetTextBoxValues(version);
            ClickAndWaitForElements(Driver.FindElement(By.CssSelector("form[action='/Version/Edit/" + version.Id + "'] input[type='submit']")), By.Id("EmailAddress"), By.CssSelector(".validation-summary-errors"), By.CssSelector(".field-validation-error"), false, true, true);
            return !Driver.Url.Contains("Version/Edit/");
        }

        public void RemoveVersion(int id)
        {
            var version = db.Versions.FirstOrDefault(v => v.Id == id);
            if (version != null)
            {
                foreach (var shiftPreference in db.ShiftPreferences.Where(sp => sp.VersionId == id))
                {
                    db.ShiftPreferences.Remove(shiftPreference);
                }
                foreach (var shift in db.Shifts.Where(s => s.VersionId == id))
                {
                    db.Shifts.Remove(shift);
                }
                db.SaveChanges();

                foreach (var userInstance in db.UserInstances.Where(uv => uv.VersionId == id))
                {
                    db.UserInstances.Remove(userInstance);
                }
                foreach (var roomInstance in db.RoomInstances.Where(ri => ri.VersionId == id))
                {
                    db.RoomInstances.Remove(roomInstance);
                }
                db.SaveChanges();
                db.Versions.Remove(version);
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

        public void SetTextBoxValue<T>(T item) where T : class
        {
            var property = typeof(T).GetProperties()[0];
            if (property.GetValue(item) != null)
            {
                Driver.FindElement(By.Id(property.Name)).SendKeys(property.GetValue(item).ToString());
            }
        }
        
        public T Clean<T>(IDictionary<string, string> row, string key)
        {
            key = key.ToUpper();//just in case developer forgot.
            if (row.ContainsKey(key) && !String.IsNullOrEmpty(row[key]))
            {
                if (typeof(T) == typeof(bool))
                {
                    return (T)Convert.ChangeType(row[key] == "1" || row[key].ToLower() == "true", typeof(T));
                }
                try
                {
                    return (T)Convert.ChangeType(row[key], typeof(T));
                }
                catch (Exception)
                {
                }
            }
            return default(T);
        }

    }
}
