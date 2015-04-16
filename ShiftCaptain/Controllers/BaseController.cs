using log4net;
using ShiftCaptain.Filters;
using ShiftCaptain.Infrastructure;
using ShiftCaptain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ShiftCaptain.Controllers
{
    [Authorized]
    [ShiftCaptain.Filters.BaseActionFilterAttribute.HandleExceptions]
    public class BaseController : Controller
    {
        public ILog Logger = LogManager.GetLogger(typeof(BaseController));
        public ShiftCaptainEntities db = new ShiftCaptainEntities();
        public User currentUser;
        public String ClassName = "Base";
        
        public BaseController()
        {
            if (SessionManager.IsLoggedIn)
            {
                currentUser = db.Users.AsNoTracking().FirstOrDefault(u => u.Id == SessionManager.UserId);
                SessionManager.IsManager = currentUser.IsManager;//Set the admin rights on the session every request.
                SessionManager.IsShiftManager = currentUser.IsShiftManager;//Set the admin rights on the session every request.
                SessionManager.NickName = currentUser.NickName;

                var CurrentVersionId = SessionManager.VersionId;
                if (CurrentVersionId != 0)
                {
                    var version = db.Versions.AsNoTracking().FirstOrDefault(v => v.Id == CurrentVersionId);
                    if (version != null)
                    {
                        ViewBag.CurrentVersion = version;
                        ViewBag.CurrentVersionName = version.EncodedName;
                    }
                }
            }
        }

        public void AddVersionDropDown(ShiftCaptain.Models.Version version){
            IQueryable<Models.Version> versions = db.Versions.AsNoTracking();
            if (SessionManager.IsLoggedIn && !(currentUser.IsManager || currentUser.IsShiftManager))
            {
                versions = db.Versions.AsNoTracking().Where(v => v.IsVisible);
            }
            if (version != null)
            {
                ViewData["VersionId"] = new SelectList(versions, "Id", "DisplayName", version.Id);
            }
            else
            {
                ViewData["VersionId"] = new SelectList(versions, "Id", "DisplayName");
            }
        }

        //this is for posts only
        //public int GetVersionId()
        //{
        //    if (ViewBag.VersionId != null)
        //    {
        //        return (int)((SelectList)ViewBag.VersionId).SelectedValue;
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //}
        
        public String DecodeVersionName(String VersionName)
        {
            if (String.IsNullOrEmpty(VersionName))
            {
                return String.Empty;
            }
            return Uri.UnescapeDataString(VersionName);
        }
        public ShiftCaptain.Models.Version GetVersion(String VersionName)
        {
            return SessionManager.GetVersion(VersionName);
        }
        public void RefreshDB()
        {
            var modifiedEntries = db.ChangeTracker.Entries().Where(e => e.State == System.Data.EntityState.Modified);
            foreach (var modifiedEntry in modifiedEntries)
            {
                modifiedEntry.Reload();
            }
        }
        public void SaveChange()
        {
            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }
                Logger.Error("Entity Validation Failed - errors follow:\n" +
                    sb.ToString(), ex);
                throw new System.Data.Entity.Validation.DbEntityValidationException(
                    "Entity Validation Failed - errors follow:\n" +
                    sb.ToString(), ex
                ); // Add the original exception as the innerException
            }
        }
    }
}