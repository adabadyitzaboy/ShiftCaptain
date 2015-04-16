using ShiftCaptain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShiftCaptainTest.Infrastructure
{
    public class ModelConverter : TestHelpers
    {
        #region Room
        public ShiftCaptain.Models.RoomView GetRoomView(IDictionary<string, string> roomObj, ShiftCaptain.Models.Building building, int VersionId)
        {
            return GetRoomView(roomObj, building.Id, building.Name, VersionId);
        }

        public ShiftCaptain.Models.RoomView GetRoomView(IDictionary<string, string> roomObj, ShiftCaptain.Models.BuildingView building, int VersionId)
        {
            return GetRoomView(roomObj, building.BuildingId, building.Name, VersionId);
        }

        private ShiftCaptain.Models.RoomView GetRoomView(IDictionary<string, string> roomObj, int BuildingId, String BuildingName, int VersionId)
        {
            return new ShiftCaptain.Models.RoomView
            {
                Name = Clean<string>(roomObj, "NAME"),
                PhoneNumber = Clean<string>(roomObj, "PHONE_NUMBER"),
                BuildingId = BuildingId,
                BuildingName = BuildingName,
                RoomNumber = Clean<string>(roomObj, "ROOM_NUMBER"),
                VersionId = VersionId,
                SundayStartTime = Clean<TimeSpan?>(roomObj, "SUNDAY_START_TIME"),
                SundayDuration = Clean<Decimal?>(roomObj, "SUNDAY_DURATION"),
                MondayStartTime = Clean<TimeSpan?>(roomObj, "MONDAY_START_TIME"),
                MondayDuration = Clean<Decimal?>(roomObj, "MONDAY_DURATION"),
                TuesdayStartTime = Clean<TimeSpan?>(roomObj, "TUESDAY_START_TIME"),
                TuesdayDuration = Clean<Decimal?>(roomObj, "TUESDAY_DURATION"),
                WednesdayStartTime = Clean<TimeSpan?>(roomObj, "WEDNESDAY_START_TIME"),
                WednesdayDuration = Clean<Decimal?>(roomObj, "WEDNESDAY_DURATION"),
                ThursdayStartTime = Clean<TimeSpan?>(roomObj, "THURSDAY_START_TIME"),
                ThursdayDuration = Clean<Decimal?>(roomObj, "THURSDAY_DURATION"),
                FridayStartTime = Clean<TimeSpan?>(roomObj, "FRIDAY_START_TIME"),
                FridayDuration = Clean<Decimal?>(roomObj, "FRIDAY_DURATION"),
                SaturdayStartTime = Clean<TimeSpan?>(roomObj, "SATURDAY_START_TIME"),
                SaturdayDuration = Clean<Decimal?>(roomObj, "SATURDAY_DURATION")
            };
        }
        
        public ShiftCaptain.Models.Room GetRoom(IDictionary<string, string> roomObj, int buildingId)
        {
            return new ShiftCaptain.Models.Room
            {
                Name = Clean<string>(roomObj, "NAME"),
                PhoneNumber = Clean<string>(roomObj, "PHONE_NUMBER"),
                BuildingId = buildingId,
                RoomNumber = Clean<string>(roomObj, "ROOM_NUMBER")
            };
        }

        public bool HasRoomHours(IDictionary<string, string> roomObj)
        {
            return Clean<TimeSpan?>(roomObj, "SUNDAY_START_TIME").HasValue ||
                Clean<TimeSpan?>(roomObj, "MONDAY_START_TIME").HasValue ||
                Clean<TimeSpan?>(roomObj, "TUESDAY_START_TIME").HasValue ||
                Clean<TimeSpan?>(roomObj, "WEDNESDAY_START_TIME").HasValue ||
                Clean<TimeSpan?>(roomObj, "THURSDAY_START_TIME").HasValue ||
                Clean<TimeSpan?>(roomObj, "FRIDAY_START_TIME").HasValue ||
                Clean<TimeSpan?>(roomObj, "SATURAY_START_TIME").HasValue;
        }            
        
        public List<ShiftCaptain.Models.RoomHour> GetRoomHours(IDictionary<string, string> roomObj, int roomInstanceId)
        {
            return new List<RoomHour>{
                GetRoomHour(roomInstanceId, 0, Clean<TimeSpan?>(roomObj, "SUNDAY_START_TIME"), Clean<Decimal?>(roomObj, "SUNDAY_DURATION")),
                GetRoomHour(roomInstanceId, 1, Clean<TimeSpan?>(roomObj, "MONDAY_START_TIME"), Clean<Decimal?>(roomObj, "MONDAY_DURATION")),
                GetRoomHour(roomInstanceId, 2, Clean<TimeSpan?>(roomObj, "TUESDAY_START_TIME"), Clean<Decimal?>(roomObj, "TUESDAY_DURATION")),
                GetRoomHour(roomInstanceId, 3, Clean<TimeSpan?>(roomObj, "WEDNESDAY_START_TIME"), Clean<Decimal?>(roomObj, "WEDNESDAY_DURATION")),
                GetRoomHour(roomInstanceId, 4, Clean<TimeSpan?>(roomObj, "THURSDAY_START_TIME"), Clean<Decimal?>(roomObj, "THURSDAY_DURATION")),
                GetRoomHour(roomInstanceId, 5, Clean<TimeSpan?>(roomObj, "FRIDAY_START_TIME"), Clean<Decimal?>(roomObj, "FRIDAY_DURATION")),
                GetRoomHour(roomInstanceId, 6, Clean<TimeSpan?>(roomObj, "SATURDAY_START_TIME"), Clean<Decimal?>(roomObj, "SATURDAY_DURATION"))
            };
        }
        
        private ShiftCaptain.Models.RoomHour GetRoomHour(int RoomInstanceId, int Day, TimeSpan? StartTime, Decimal? Duration)
        {
            if (!StartTime.HasValue)
            {
                return null;
            }
            return new RoomHour()
            {
                RoomInstanceId = RoomInstanceId,
                StartTime = StartTime.Value,
                Duration = Duration.Value,
                Day = Day
            };
        }
        #endregion

        #region ShiftPreference

        public ShiftCaptain.Models.ShiftPreference GetShiftPreference(IDictionary<string, string> ShiftPreference, int VersionId, int UserId, int PreferenceId)
        {
            return new ShiftCaptain.Models.ShiftPreference
            {
                VersionId = VersionId,
                Day = Clean<int>(ShiftPreference, "DAY"),
                Duration = Clean<Decimal>(ShiftPreference, "DURATION"),
                StartTime = Clean<TimeSpan>(ShiftPreference, "START_TIME"),
                UserId = UserId,
                PreferenceId = PreferenceId
            };
        }

        #endregion


        #region Shift

        public ShiftCaptain.Models.Shift GetShift(IDictionary<string, string> Shift, int VersionId, int UserId, int RoomId)
        {
            return new ShiftCaptain.Models.Shift
            {
                VersionId = VersionId,
                Day = Clean<int>(Shift, "DAY"),
                Duration = Clean<Decimal>(Shift, "DURATION"),
                StartTime = Clean<TimeSpan>(Shift, "START_TIME"),
                UserId = UserId,
                RoomId = RoomId
            };
        }

        #endregion

        #region Users
        public ShiftCaptain.Models.User GetUser(IDictionary<string, string> User, String ticks, int counter)
        {
            var user = new User
            {
                EmailAddress = Clean<string>(User, "EMAIL_ADDRESS") ?? String.Format("{0}-{1}@emails.com", ticks, counter),
                Pass = Clean<string>(User, "PASS"),
                FName = Clean<string>(User, "FIRST_NAME"),
                LName = Clean<string>(User, "LAST_NAME"),
                NickName = Clean<string>(User, "NICK_NAME"),
                EmployeeId = Clean<string>(User, "EMPLOYEE_ID"),
                PhoneNumber = Clean<string>(User, "PHONE_NUMBER"),
                IsManager = Clean<bool>(User, "IS_MANAGER"),
                IsShiftManager = Clean<bool>(User, "IS_SHIFT_MANAGER"),
                IsActive = Clean<bool>(User, "IS_ACTIVE"),
                IsMale = Clean<bool>(User, "IS_MALE"),
                Address = GetAddress(User)
            };
            return user;
        }
        public ShiftCaptain.Models.UserView GetUserView(IDictionary<string, string> User, int VersionId, String ticks, int counter)
        {
            return new UserView
            {
                EmailAddress = Clean<string>(User, "EMAIL_ADDRESS") ?? String.Format("{0}-{1}@emails.com", ticks, counter),
                Pass = Clean<string>(User, "PASS"),
                FName = Clean<string>(User, "FIRST_NAME"),
                LName = Clean<string>(User, "LAST_NAME"),
                NickName = Clean<string>(User, "NICK_NAME"),
                EmployeeId = Clean<string>(User, "EMPLOYEE_ID"),
                PhoneNumber = Clean<string>(User, "PHONE_NUMBER"),
                IsManager = Clean<bool>(User, "IS_MANAGER"),
                IsShiftManager = Clean<bool>(User, "IS_SHIFT_MANAGER"),
                IsActive = Clean<bool>(User, "IS_ACTIVE"),
                IsMale = Clean<bool>(User, "IS_MALE"),
                VersionId = VersionId,
                MinHours = Clean<decimal?>(User, "MIN_HOURS"),
                MaxHours = Clean<decimal?>(User, "MAX_HOURS"),
                Line1 = Clean<string>(User, "LINE_1"),
                Line2 = Clean<string>(User, "LINE_2"),
                City = Clean<string>(User, "CITY"),
                State = Clean<string>(User, "STATE"),
                ZipCode = Clean<string>(User, "ZIP_CODE"),
                Country = Clean<string>(User, "COUNTRY")
            };
        }
        #endregion

        #region Versions
        public ShiftCaptain.Models.Version GetVersion(IDictionary<string, string> Version)
        {
            return new ShiftCaptain.Models.Version
            {
                Name = Clean<string>(Version, "NAME"),
                IsVisible = Clean<bool>(Version, "IS_VISIBLE"),
                IsActive = Clean<bool>(Version, "IS_ACTIVE")
            };
        }
        #endregion

        #region Preference
        public ShiftCaptain.Models.Preference GetPreference(IDictionary<string, string> Preference)
        {
            return new ShiftCaptain.Models.Preference
            {
                Name = Clean<string>(Preference, "NAME"),
                Description = Clean<string>(Preference, "DESCRIPTION"),
                CanWork = Clean<bool>(Preference, "CAN_WORK"),
                Color = Clean<string>(Preference, "COLOR")
            };
        }
        #endregion

        #region Building
        public ShiftCaptain.Models.Building GetBuilding(IDictionary<string, string> Building, string ticks = "", int counter = 0)
        {
            return new ShiftCaptain.Models.Building
            {
                Name = Clean<string>(Building, "NAME") ?? String.Format("Test Building - {0}", ticks, counter),
                ManagerPhone = Clean<string>(Building, "MANAGER_PHONE"),
                PhoneNumber = Clean<string>(Building, "PHONE_NUMBER"),
                Address = GetAddress(Building)
            };
        }

        public ShiftCaptain.Models.BuildingView GetBuildingView(IDictionary<string, string> Building, string ticks = "", int counter = 0)
        {
            return new ShiftCaptain.Models.BuildingView
            {
                Name = Clean<string>(Building, "NAME") ?? String.Format("Test Building - {0}", ticks, counter),
                Line1 = Clean<string>(Building, "LINE_1"),
                Line2 = Clean<string>(Building, "LINE_2"),
                City = Clean<string>(Building, "CITY"),
                State = Clean<string>(Building, "STATE"),
                ZipCode = Clean<string>(Building, "ZIP_CODE"),
                Country = Clean<string>(Building, "COUNTRY"),
                ManagerPhone = Clean<string>(Building, "MANAGER_PHONE"),
                PhoneNumber = Clean<string>(Building, "PHONE_NUMBER")
            };
        }
        #endregion

        #region Address
        public ShiftCaptain.Models.Address GetAddress(IDictionary<string, string> Address)
        {
            if (Clean<string>(Address, "LINE_1") == null)
            {
                return null;
            }
            return new ShiftCaptain.Models.Address
            {
                Line1 = Clean<string>(Address, "LINE_1"),
                Line2 = Clean<string>(Address, "LINE_2"),
                City = Clean<string>(Address, "CITY"),
                State = Clean<string>(Address, "STATE"),
                ZipCode = Clean<string>(Address, "ZIP_CODE"),
                Country = Clean<string>(Address, "COUNTRY")
            };
        }
        #endregion

        #region Clone
        public Clone GetClone(ShiftCaptain.Models.Version version, IQueryable<RoomView> rooms, IQueryable<UserView> users)
        {
            Clone clone = new Clone
            {
                Version = new ShiftCaptain.Models.Version
                {
                    Name = version.Name
                },
                CloneUser = new List<string>(),
                CloneRoom = new List<string>()
            };
            var userItems = (List<string>) clone.CloneUser;
            var roomItems = (List<string>) clone.CloneRoom;
            foreach (var user in users)
            {
                userItems.Add(user.NickName);
            }

            foreach (var room in rooms)
            {
                roomItems.Add(room.Name);
            }

            return clone;
        }
        #endregion
        
        #region helpers
        public T Clean<T>(IDictionary<string, string> row, string key)
        {
            key = key.ToUpper();//just in case developer forgot.
            if (row.ContainsKey(key) && !String.IsNullOrEmpty(row[key]))
            {
                if (typeof(T) == typeof(bool))
                {
                    return (T)Convert.ChangeType(row[key] == "1" || row[key].ToLower() == "true", typeof(T));
                }
                if (typeof(T) == typeof(TimeSpan) || typeof(T) == typeof(TimeSpan?))
                {
                    TimeSpan ts;
                    if (TimeSpan.TryParseExact(row[key], @"hh\:mm", null, out ts))
                    {
                        if (typeof(T) == typeof(TimeSpan?))
                        {
                            TimeSpan? nts = ts;
                            return (T)System.ComponentModel.TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(nts);
                            //return (T)Convert.ChangeType(nts, typeof(T));
                        }
                        return (T)Convert.ChangeType(ts, typeof(T));
                    }
                }
                //if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?))
                //{
                //    return (T)Convert.ChangeType(row[key] == "1" || row[key].ToLower() == "true", typeof(T));
                //}
                try
                {
                    return (T)Convert.ChangeType(row[key], typeof(T));
                }
                catch (Exception ex)
                {
                    try
                    {
                        return (T)System.ComponentModel.TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(row[key]);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            return default(T);
        }
        #endregion
    }
}
