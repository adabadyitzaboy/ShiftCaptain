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
    [DeploymentItem("DataSources/Preferences.csv", "")]
    public class Preference : CommonActions
    {
        [TestMethod]
        public void CRUD()
        {
            Login();
            var createTable = new DataParser("Preferences.csv").Tables["Create"];
            var ticks = DateTime.Now.Ticks.ToString();
            var counter = 0;

            foreach (var preferenceObj in createTable)
            {
                GoToPage("Preference");
                var preference = GetPreference(preferenceObj);

                //Change preference if necessary
                var created = CreatePreference(preference);
                if (Clean<bool>(preferenceObj, "VALID"))
                {
                    Assert.IsTrue(created, String.Format("Failed to Create preference - {0}", preference.Name));
                    var createdPreference = db.Preferences.FirstOrDefault(v => v.Name == preference.Name);
                    Assert.IsTrue(ComparePreference(preference, createdPreference), String.Format("Preference does not match {0}", preference.Name));
                    preference = createdPreference;
                    preference.Name = preference.Name + counter;
                    preference.Description = preference.Description + counter;
                    preference.Color = preference.Color + counter;
                    preference.CanWork = !preference.CanWork;

                    var edited = EditPreference(preference);
                    Assert.IsTrue(edited, String.Format("Failed editing preference {0}", preference.Name));
                    createdPreference = db.Preferences.FirstOrDefault(v => v.Name == preference.Name);
                    Assert.IsTrue(ComparePreference(preference, createdPreference), String.Format("Preference does not match {0}", preference.Name));

                    GoToPage("/Preference/Delete/" + preference.Id);
                    ClickAndWaitForNextPage(Driver.FindElement(By.CssSelector("form input[type='submit']")), "Preference");

                    createdPreference = db.Preferences.FirstOrDefault(v => v.Name == preference.Name);
                    Assert.IsNull(createdPreference, String.Format("Failed to remove preference {0}", preference.Name));
                }
                else
                {
                    Assert.IsFalse(created, String.Format("Created preference - {0}", preference.Name));
                }
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            try
            {
                var createTable = new DataParser("Preferences.csv").Tables["Create"];

                foreach (var preferenceObj in createTable)
                {
                    var name = Clean<string>(preferenceObj, "Name");
                    if (String.IsNullOrEmpty(name))
                    {
                        RemovePreference(name);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
