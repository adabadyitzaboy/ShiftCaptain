using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShiftCaptain.Infrastructure
{
    public static class SessionManager
    {
        public static bool IsLoggedIn
        {
            get
            {
                return token != null;
            }
        }

        public static int UserId
        {
            get
            {
                return GetValue<int>("UID");
            }
            set
            {
                SetValue<int>("UID", value);
            }
        }

        public static int VersionId
        {
            get
            {
                return GetValue<int>("VID");
            }
            set
            {
                SetValue<int>("VID", value);
            }
        }

        public static String NickName
        {
            get
            {
                return GetValue<String>("NN");
            }
            set
            {
                SetValue<String>("NN", value);
            }
        }

        public static bool IsManager
        {
            get
            {
                return GetValue<bool>("IM");
            }
            set
            {
                SetValue<bool>("IM", value);
            }
        }

        public static bool IsShiftManager
        {
            get
            {
                return GetValue<bool>("ISM");
            }
            set
            {
                SetValue<bool>("ISM", value);
            }
        }

        public static String token
        {
            get
            {
                return GetValue<String>("t");
            }
            set
            {
                SetValue<String>("t", value);
            }
        }
        public static void Clear()
        {
            HttpContext.Current.Session.Clear();
        }
        #region Protected Members

        private static T GetValue<T>(String name)
        {
            
            return (T)HttpContext.Current.Session[name];
        }
        private static void SetValue<T>(String name, T value)
        {
            HttpContext.Current.Session[name] = value;
        }

        #endregion

    }
}