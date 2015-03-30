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
    public class Room : CommonActions
    {
        
        private List<int> roomIds = new List<int>();
        [TestMethod]
        public void CRUD()
        {
            Login();
            var createTable = new DataParser("Rooms.csv").Tables["Create"];
            var ticks = DateTime.Now.Ticks.ToString();
            var counter = 0;

            foreach (var roomObj in createTable)
            {
                GoToPage("Room");
                var version = CreateDefaultVersion(Clean<string>(roomObj, "VERSION_NAME"));
                var building = CreateDefaultBuilding(Clean<string>(roomObj, "BUILDING_NAME"));
                var room = GetRoomView(roomObj, building, version.Id);

                //Change room if necessary
                var created = CreateRoom(room);
                if (created)
                {
                    roomIds.Add(room.RoomId);
                }
                if (Clean<bool>(roomObj, "VALID"))
                {
                    Assert.IsTrue(created, String.Format("Failed to Create room - {0}", room.Name));
                    var createdRoom = db.RoomViews.FirstOrDefault(b => b.Name == room.Name);
                    Assert.IsTrue(CompareRoom(room, createdRoom), String.Format("Room does not match {0}", room.Name));
                    room = createdRoom;
                    room.Name = room.Name + counter;
                    
                    var edited = EditRoom(room);
                    Assert.IsTrue(edited, String.Format("Failed editing room {0}", room.Name));
                    createdRoom = db.RoomViews.FirstOrDefault(b => b.Name == room.Name);
                    Assert.IsTrue(CompareRoom(room, createdRoom), String.Format("Room does not match {0}", room.Name));

                    GoToPage("/Room/Delete/" + room.RoomId);
                    ClickAndWaitForNextPage(Driver.FindElement(By.CssSelector("form input[type='submit']")), "Room");

                    createdRoom = db.RoomViews.FirstOrDefault(b => b.Name == room.Name);
                    Assert.IsNull(createdRoom, String.Format("Failed to remove room {0}", room.Name));
                }
                else
                {
                    Assert.IsFalse(created, String.Format("Created room - {0}", room.Name));
                }
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            db = new ShiftCaptainEntities();
            try
            {
                var createTable = new DataParser("Rooms.csv").Tables["Create"];
                foreach (var roomObj in createTable)
                {
                    var name = Clean<string>(roomObj, "Name");
                    if (String.IsNullOrEmpty(name))
                    {
                        RemoveRoom(name);
                    }
                }

                foreach (var roomId in roomIds)
                {
                    RemoveRoom(roomId);
                }
            }
            catch (Exception)
            {
            }
            RemoveDefaultVersions();
            RemoveDefaultBuildings();

        }
    }
}
