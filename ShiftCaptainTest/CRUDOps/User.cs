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
    public class User : CommonActions
    {
        List<String> VersionNames = new List<string>();
        [TestMethod]
        public void CRUD()
        {
            Login();
            var createTable = new DataParser("Users.csv").Tables["Create"];
            var ticks = DateTime.Now.Ticks.ToString();
            var counter = 0;
                    
            foreach (var userObj in createTable)
            {
                var version = CreateDefaultVersion(Clean<string>(userObj, "VERSION_NAME"));
                if (!VersionNames.Contains(version.Name))
                {
                    VersionNames.Add(version.Name);
                }
                GoToPage(version.Name, "User", true);

                var user = GetUserView(userObj, version.Id, ticks, ++counter);

                //Change version if necessary
                var created = CreateUser(version.Name, user);
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

                    var edited = EditUser(version.Name, user);
                    Assert.IsTrue(edited, String.Format("Failed editing user {0}", user.EmailAddress));
                    createdUser = db.UserViews.FirstOrDefault(uv => uv.EmailAddress == user.EmailAddress);
                    Assert.IsTrue(CompareUser(user, createdUser), String.Format("User does not match {0}", user.EmailAddress));

                    GoToPage(version.Name, "/User/Delete/" + user.UserId);
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
            RefreshDB();
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
            foreach (var VersionName in VersionNames)
            {
                RemoveDefaultVersions(VersionName);
            }
        }
    }
}
