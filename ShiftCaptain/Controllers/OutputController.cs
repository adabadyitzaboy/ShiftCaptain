using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShiftCaptain.Models;
using System.Net;
using System.Data.Objects.SqlClient;
using System.Data.Objects;
using ShiftCaptain.Helpers;
using ShiftCaptain.Filters;
using ShiftCaptain.Infrastructure;

namespace ShiftCaptain.Controllers
{
    public class OutputController : BaseController
    {
        //
        // GET: /Shift/
        [VersionRequired(Order=1)]
        [BuildingRequired(Order = 2)]
        [RoomRequired(Order = 3)]
        [UserRequired(Order = 4)]
        public ActionResult Download(String VersionName)
        {
            var version = GetVersion(VersionName);
            var outputManager = new OutputManager();
            var workbook = outputManager.Export(version.Id);
            
            var cd = new System.Net.Mime.ContentDisposition
            {
                // for example foo.bak
                FileName = version.EncodedName + ".xlsx",

                // always prompt the user for downloading, set to true if you want 
                // the browser to try to show the file inline
                Inline = false,
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());
            var ms = new System.IO.MemoryStream();
            workbook.SaveAs(ms);
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            return File(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            //using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            //{
            //    workbook.SaveAs(memoryStream);
            //    return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            //}
            //if(fsr != null){
            //    return fsr;
            //}
            //return RedirectToRoute("Error", "InternalError");
        }
    }
}