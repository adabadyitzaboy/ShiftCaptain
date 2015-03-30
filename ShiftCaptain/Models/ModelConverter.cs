using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShiftCaptain.Models
{
    public static class ModelConverter
    {
        #region User
        public static User GetUser(UserView userview)
        {
            return GetUser(new User(), userview);
        }

        public static User GetUser(User user, UserView userview)
        {
            user.EmailAddress = userview.EmailAddress;
            user.FName = userview.FName;
            user.MName = userview.MName;
            user.LName = userview.LName;
            user.NickName = userview.NickName;
            user.EmployeeId = userview.EmployeeId;
            user.PhoneNumber = userview.PhoneNumber;
            user.IsShiftManager = userview.IsShiftManager;
            user.IsManager = userview.IsManager;
           
            user.Locked = userview.Locked;
            user.IsActive = userview.IsActive;
            user.IsMale = userview.IsMale;
            return user;
        }

        public static UserInstance GetUserInstance(UserView userview, ShiftCaptainEntities db)
        {
            if (!userview.VersionId.HasValue)
            {
                return null;
            }
            var userI = db.UserInstances.FirstOrDefault(ui => ui.UserId == userview.UserId && ui.VersionId == userview.VersionId);
            if (!userview.MinHours.HasValue || !userview.MaxHours.HasValue)
            {
                return userI;
            }
            if (userI == null)
            {
                return new UserInstance
                {
                    UserId = userview.UserId,
                    VersionId = userview.VersionId.Value,
                    MinHours = (decimal)userview.MinHours,
                    MaxHours = (decimal)userview.MaxHours
                };
            }
            else
            {
                userI.MinHours = (decimal)userview.MinHours;
                userI.MaxHours = (decimal)userview.MaxHours;
            }
            return userI;
        }

        public static void UpdateUser(User user, UserView userview)
        {
            user.EmailAddress = userview.EmailAddress;
            user.FName = userview.FName;
            user.MName = userview.MName;
            user.LName = userview.LName;
            user.NickName = userview.NickName;
            user.EmployeeId = userview.EmployeeId;
            user.PhoneNumber = userview.PhoneNumber;
            user.IsShiftManager = userview.IsShiftManager;
            user.IsManager = userview.IsManager;

            user.Locked = userview.Locked;
            user.IsActive = userview.IsActive;
            user.IsMale = userview.IsMale;
        }
        #endregion
        #region Address
        public static Address GetAddress(UserView userview, ShiftCaptainEntities db)
        {
            Address address = userview.AddressId != null ? db.Addresses.Find(userview.AddressId) : new Address();
            address.Line1 = userview.Line1;
            address.Line2 = userview.Line2;
            address.City = userview.City;
            address.State = userview.State;
            address.Country = userview.Country;
            address.ZipCode = userview.ZipCode;
            return address;
        }
        #endregion
    }
}