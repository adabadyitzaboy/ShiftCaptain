using ShiftCaptain.Filters;
using ShiftCaptain.Infrastructure;
using ShiftCaptain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShiftCaptain.Controllers
{
    [Authorized]
    public class BaseController : Controller
    {
        private ShiftCaptainEntities db = new ShiftCaptainEntities();
        public User currentUser;
        public String ClassName = "Base";
        public int CurrentVersionId = 0;
        public BaseController()
        {
            if (SessionManager.IsLoggedIn)
            {
                currentUser = db.Users.FirstOrDefault(u => u.Id == SessionManager.UserId);
                SessionManager.IsManager = currentUser.IsManager;//Set the admin rights on the session every request.
                SessionManager.IsShiftManager = currentUser.IsShiftManager;//Set the admin rights on the session every request.
                SessionManager.NickName = currentUser.NickName;

                CurrentVersionId = SessionManager.VersionId;
                if (CurrentVersionId != 0)
                {
                    var version = db.Versions.FirstOrDefault(v => v.Id == CurrentVersionId);
                    if (version != null)
                    {
                        ViewBag.CurrentVersion = version;
                    }
                }
            }
        }
        public void AddVersionDropDown(){
            IQueryable<Models.Version> versions = db.Versions;
            if (SessionManager.IsLoggedIn && !(currentUser.IsManager || currentUser.IsShiftManager))
            {
                versions = db.Versions.Where(v => v.IsVisible);
            }
            if (CurrentVersionId != 0)
            {
                ViewData["VersionId"] = new SelectList(versions, "Id", "DisplayName", CurrentVersionId);
            }
            else
            {
                ViewData["VersionId"] = new SelectList(versions, "Id", "DisplayName");
            }
        }

        //this is for posts only
        public int GetVersionId()
        {
            if (ViewBag.VersionId != null)
            {
                return (int)((SelectList)ViewBag.VersionId).SelectedValue;
            }
            else
            {
                return CurrentVersionId;
            }
        }
    }
}