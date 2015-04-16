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
    public class Version : CommonActions
    {    
        [TestMethod]
        public void CRUD()
        {
            Login();
            var createTable = new DataParser("Versions.csv").Tables["Create"];
            var ticks = DateTime.Now.Ticks.ToString();
            var counter = 0;
                    
            foreach (var versionObj in createTable)
            {
                GoToPage("Version");
                var version = GetVersion(versionObj);

                //Change version if necessary
                var created = CreateVersion(version);
                if (Clean<bool>(versionObj, "VALID"))
                {
                    Assert.IsTrue(created, String.Format("Failed to Create version - {0}", version.Name));
                    var createdVersion = db.Versions.FirstOrDefault(v => v.Name == version.Name);
                    Assert.IsTrue(CompareVersion(version, createdVersion), String.Format("Version does not match {0}", version.Name));
                    version = createdVersion;
                    version.Name = version.Name + counter;
                    version.IsVisible = !version.IsVisible;

                    var edited = EditVersion(version);
                    Assert.IsTrue(edited, String.Format("Failed editing version {0}", version.Name));
                    createdVersion = db.Versions.FirstOrDefault(v => v.Name == version.Name);
                    Assert.IsTrue(CompareVersion(version, createdVersion), String.Format("Version does not match {0}", version.Name));

                    GoToPage("/Version/Delete/" + version.Id);
                    ClickAndWaitForNextPage(Driver.FindElement(By.CssSelector("form input[type='submit']")), "Version");

                    createdVersion = db.Versions.FirstOrDefault(v => v.Name == version.Name);
                    Assert.IsNull(createdVersion, String.Format("Failed to remove version {0}", version.Name));
                }
                else
                {
                    Assert.IsFalse(created, String.Format("Created version - {0}", version.Name));
                }
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            RefreshDB();
            try
            {
                var createTable = new DataParser("Versions.csv").Tables["Create"];

                foreach (var versionObj in createTable)
                {
                    var name = Clean<string>(versionObj, "Name");
                    if (String.IsNullOrEmpty(name))
                    {
                        RemoveVersion(name);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
