using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShiftCaptain.Models;
using ShiftCaptain.Filters;

namespace ShiftCaptain.Controllers
{
    public class BuildingController : BaseController
    {
        public BuildingController()
        {
            ClassName = "building";
        }

        private BuildingView GetBuildingView(int id = 0)
        {
            return db.BuildingViews.Where(bv => bv.BuildingId == id).FirstOrDefault();
        }
        //
        // GET: /Building/
        [ShiftManagerAccess]
        public ActionResult Index()
        {
            return View(db.BuildingViews.ToList());
        }

        //
        // GET: /Building/Details/5
        [ShiftManagerAccess]
        public ActionResult Details(int id = 0)
        {
            BuildingView buildingview = GetBuildingView(id);
            if (buildingview == null)
            {
                return HttpNotFound();
            }
            return View(buildingview);
        }

        //
        // GET: /Building/Create
        [ManagerAccess]
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Building/Create
        [ManagerAccess]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BuildingView buildingview)
        {
            if (ModelState.IsValid)
            {
                var building = new Building { 
                    ManagerPhone = buildingview.ManagerPhone,
                    Name = buildingview.Name,
                    PhoneNumber = buildingview.PhoneNumber
                };

                if (buildingview.Line1 != null)
                {
                    var address = new Address
                    {
                        Line1 = buildingview.Line1,
                        Line2 = buildingview.Line2,
                        City = buildingview.City,
                        State = buildingview.State,
                        Country = buildingview.Country,
                        ZipCode = buildingview.ZipCode
                    };
                    db.Addresses.Add(address);
                    SaveChange();
                    building.AddressId = address.Id;
                }

                db.Buildings.Add(building);
                SaveChange();
                return RedirectToAction("Index");
            }

            return View(buildingview);
        }

        //
        // GET: /Building/Edit/5
        [ManagerAccess]
        public ActionResult Edit(int id = 0)
        {
            BuildingView buildingview = GetBuildingView(id);
            if (buildingview == null)
            {
                return HttpNotFound();
            }
            return View(buildingview);
        }

        //
        // POST: /Building/Edit/5

        [HttpPost]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BuildingView buildingview)
        {
            if (ModelState.IsValid)
            {
                var building = db.Buildings.Find(buildingview.BuildingId);
                building.ManagerPhone = buildingview.ManagerPhone;
                building.Name = buildingview.Name;
                building.PhoneNumber = buildingview.PhoneNumber;
                
                if (buildingview.Line1 != null)
                {
                    Address address = buildingview.AddressId != null ? db.Addresses.Find(buildingview.AddressId) : new Address();
                    address.Line1 = buildingview.Line1;
                    address.Line2 = buildingview.Line2;
                    address.City = buildingview.City;
                    address.State = buildingview.State;
                    address.Country = buildingview.Country;

                    if (buildingview.AddressId != null)
                    {
                        db.Addresses.Add(address);
                        SaveChange();
                        building.AddressId = address.Id;
                    }
                    else
                    {
                        db.Entry(address).State = EntityState.Modified;
                        SaveChange();
                    }
                }
                else
                {
                    if (buildingview.AddressId != null)
                    {
                        db.Addresses.Remove(db.Addresses.Find(buildingview.AddressId));
                        buildingview.AddressId = null;
                    }
                }

                db.Entry(building).State = EntityState.Modified;
                SaveChange();
                return RedirectToAction("Index");
            }
            return View(buildingview);
        }

        //
        // GET: /Building/Delete/5
        [ManagerAccess]
        public ActionResult Delete(int id = 0)
        {
            BuildingView buildingview = GetBuildingView(id);
            if (buildingview == null)
            {
                return HttpNotFound();
            }
            var canDelete = true;
            var roomCount = db.Rooms.Count(r=>r.BuildingId == buildingview.BuildingId && r.Shifts.Count(s=>s.Version.IsApproved) > 0);
            if (roomCount > 0)
            {
                canDelete = false;
                ViewBag.CantDeleteReason = "Cannot delete a building that contains room with shifts in an approved version.";
            }
            ViewBag.CanDelete = canDelete;

            return View(buildingview);
        }

        //
        // POST: /Building/Delete/5

        [HttpPost, ActionName("Delete")]
        [ManagerAccess]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var building = db.Buildings.Find(id);
            if (building.AddressId != null)
            {
                var address = db.Addresses.Find(building.AddressId);
                db.Addresses.Remove(address);
                SaveChange();
            }
            db.Buildings.Remove(building);
            SaveChange();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}