using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftCaptainTest.Infrastructure;
using ShiftCaptain.Models;
using System.Linq;
using OpenQA.Selenium;

namespace ShiftCaptainTest
{
    /// <summary>
    /// Tests User CRUD operations
    /// </summary>
    [TestClass]
    public class User : CommonActions
    {

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
    
        [TestMethod]
        public void CRUD()
        {
            Login();
            var createTable = new DataParser("Users.csv").Tables["Create"];
            var ticks = DateTime.Now.Ticks.ToString();
            var counter = 0;
                    
            foreach (var userObj in createTable)
            {
                GoToPage("User");
                var user = new UserView
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
                    IsMale = Clean<bool>(userObj, "IS_MALE"),
                    VersionId = Clean<int>(userObj, "VERSION_ID"),
                    MinHours = Clean<decimal?>(userObj, "MIN_HOURS"),
                    MaxHours = Clean<decimal?>(userObj, "MAX_HOURS"),
                    Line1 = Clean<string>(userObj, "LINE_1"),
                    Line2 = Clean<string>(userObj, "LINE_2"),
                    City = Clean<string>(userObj, "CITY"),
                    State = Clean<string>(userObj, "STATE"),
                    ZipCode = Clean<string>(userObj, "ZIPCODE"),
                    Country = Clean<string>(userObj, "COUNTRY")
                };

                //Change version if necessary
                var created = CreateUser(user);
                if (Clean<bool>(userObj, "VALID"))
                {
                    Assert.IsTrue(created, String.Format("Failed to Create user - {0}", user.EmailAddress));
                    var createdUser = db.UserViews.FirstOrDefault(uv => uv.EmailAddress == user.EmailAddress);
                    Assert.IsTrue(CompareUser(user, createdUser), String.Format("User does not match {0}", user.EmailAddress));
                    user = createdUser;
                    user.EmailAddress = counter + user.EmailAddress;
                    user.FName = (user.FName ?? "") + counter;
                    user.LName = (user.LName ?? "") + counter;
                    user.NickName = (user.NickName ?? "") + counter;
                    user.EmployeeId = (user.EmployeeId ?? "") + counter;
                    user.PhoneNumber = (user.PhoneNumber ?? "") + counter;
                    user.MinHours = (user.MinHours ?? 0) + counter;
                    user.MaxHours = (user.MaxHours ?? 0) + counter;
                    user.Line1 = (user.Line1 ?? "") + counter;
                    user.Line2 = (user.Line2 ?? "") + counter;
                    user.City = (user.City ?? "") + counter;
                    user.State = (user.State ?? "") + counter;
                    user.ZipCode = (user.ZipCode ?? "") + counter;
                    user.Country = (user.Country ?? "") + counter;

                    var edited = EditUser(user);
                    Assert.IsTrue(edited, String.Format("Failed editing user {0}", user.EmailAddress));
                    createdUser = db.UserViews.FirstOrDefault(uv => uv.EmailAddress == user.EmailAddress);
                    Assert.IsTrue(CompareUser(user, createdUser), String.Format("User does not match {0}", user.EmailAddress));

                    GoToPage("/User/Delete/" + user.UserId);
                    ClickAndWaitForNextPage(Driver.FindElement(By.CssSelector("form input[type='submit']")), "User");

                    createdUser = db.UserViews.FirstOrDefault(uv => uv.EmailAddress == user.EmailAddress);
                    Assert.IsNull(createdUser, String.Format("Failed to remove user {0}", user.EmailAddress));
                } else {
                    Assert.IsFalse(created, String.Format("Create user did not fail as intended. - {0}", user.EmailAddress));
                }
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            try
            {
                var createTable = new DataParser("Users.csv").Tables["Create"];

                foreach (var userObj in createTable)
                {
                    RemoveUser(userObj["EmailAddress"]);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
