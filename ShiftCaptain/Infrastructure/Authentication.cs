using ShiftCaptain.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace ShiftCaptain.Infrastructure
{
    public static class Authentication
    {
        public static Boolean Login(EmailNPass user, ref String errorMessage)
        {
            errorMessage = "";
            var db = new ShiftCaptainEntities();
            var validUser = db.Users.FirstOrDefault(u => u.EmailAddress == user.EmailAddress);
            if (validUser != null)
            {
                if (validUser.Locked && validUser.LastLogin.HasValue && (validUser.LastLogin.Value <= DateTime.Now.AddMinutes(-30)))
                {
                    validUser.Locked = false;
                    validUser.NumTries = 0;
                }
                int numTries = 4;
                Int32.TryParse(ConfigurationManager.AppSettings["NumTries"], out numTries);
                if (validUser.NumTries++ > numTries)
                {
                    validUser.Locked = true;
                }
                if (!validUser.Locked && validUser.Pass == user.Pass)
                {
                    validUser.NumTries = 0;
                    if (validUser.IsActive)
                    {
                        var ticket = new FormsAuthenticationTicket(String.Format("{0}|{1}", validUser.Id, validUser.EmailAddress), true, 30);
                        var encr = FormsAuthentication.Encrypt(ticket);
                        //FormsAuthentication.SetAuthCookie("authCk", false);

                        SessionManager.UserId = validUser.Id;
                        SessionManager.token = encr;
                        SessionManager.GetVersion();
                        return true;
                    }
                }
                validUser.LastLogin = DateTime.Now;
                db.Entry(validUser).State = EntityState.Modified;
                db.SaveChanges();
                if (validUser.Locked)
                {
                    errorMessage = "Your account is locked.  Please wait 30 minutes or contact the system administrator.";
                    return false;
                }
            }
            errorMessage = "Email Address or Password is invalid.";
            return false;
        }
        public static void LogOff()
        {
            SessionManager.Clear();
        }

        public static bool Authenticate(HttpContextBase ctx)
        {
            if (SessionManager.token != null)
            {
                var ticket = FormsAuthentication.Decrypt(SessionManager.token);
                if (ticket != null && !ticket.Expired)
                {
                    ctx.User = new GenericPrincipal(new FormsIdentity(ticket), new string[] { });
                    var nTicket = new FormsAuthenticationTicket(ticket.Name, true, 30);//Everytime you click a new page, clock restarts
                    SessionManager.token = FormsAuthentication.Encrypt(nTicket);
                }                    
            }
            return ctx.User.Identity.IsAuthenticated;
        }
    }
}