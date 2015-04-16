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
    [DeploymentItem("DataSources/Buildings.csv", "")]
    public class Building : CommonActions
    {
        private List<int> buildingIds = new List<int>();
        [TestMethod]
        public void CRUD()
        {
            Login();
            var createTable = new DataParser("Buildings.csv").Tables["Create"];
            var ticks = DateTime.Now.Ticks.ToString();
            var counter = 0;
                    
            foreach (var buildingObj in createTable)
            {
                GoToPage("Building");
                var building = GetBuildingView(buildingObj, ticks, ++counter);

                //Change building if necessary
                var created = CreateBuilding(building);
                if (created)
                {
                    buildingIds.Add(building.BuildingId);
                }
                if (Clean<bool>(buildingObj, "VALID"))
                {
                    Assert.IsTrue(created, String.Format("Failed to Create building - {0}", building.Name));
                    var createdBuilding = db.BuildingViews.FirstOrDefault(b => b.Name == building.Name);
                    Assert.IsTrue(CompareBuilding(building, createdBuilding), String.Format("Building does not match {0}", building.Name));
                    building = createdBuilding;
                    building.Name = building.Name + counter;

                    var edited = EditBuilding(building);
                    Assert.IsTrue(edited, String.Format("Failed editing building {0}", building.Name));
                    createdBuilding = db.BuildingViews.FirstOrDefault(b => b.Name == building.Name);
                    Assert.IsTrue(CompareBuilding(building, createdBuilding), String.Format("Building does not match {0}", building.Name));

                    GoToPage("/Building/Delete/" + building.BuildingId);
                    ClickAndWaitForNextPage(Driver.FindElement(By.CssSelector("form input[type='submit']")), "Building");

                    createdBuilding = db.BuildingViews.FirstOrDefault(b => b.Name == building.Name);
                    Assert.IsNull(createdBuilding, String.Format("Failed to remove building {0}", building.Name));
                }
                else
                {
                    Assert.IsFalse(created, String.Format("Created building - {0}", building.Name));
                }
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            RefreshDB();
            try
            {
                var createTable = new DataParser("Buildings.csv").Tables["Create"];

                foreach (var buildingObj in createTable)
                {
                    var name = Clean<string>(buildingObj, "Name");
                    if (String.IsNullOrEmpty(name))
                    {
                        RemoveBuilding(name);
                    }
                }

                foreach (var buildingId in buildingIds)
                {
                    RemoveBuilding(buildingId);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
