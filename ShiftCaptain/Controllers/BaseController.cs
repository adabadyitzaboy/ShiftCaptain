using ShiftCaptain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShiftCaptain.Controllers
{
    public class BaseController : Controller
    {
        private ShiftCaptainEntities db = new ShiftCaptainEntities();
        
        public BaseController()
        {
            var currentVersion = db.Versions.OrderByDescending(v => v.Id).First();
            if (currentVersion != null)
            {
                ViewData["VersionId"] = new SelectList(db.Versions, "Id", "Name", currentVersion.Id);
            }
        }
    }
}