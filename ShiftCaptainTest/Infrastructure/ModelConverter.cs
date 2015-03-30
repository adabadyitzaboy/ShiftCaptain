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
            return new ShiftCaptain.Models.RoomView
            {
                Name = Clean<string>(roomObj, "NAME"),
                PhoneNumber = Clean<string>(roomObj, "PHONE_NUMBER"),
                BuildingId = building.Id,
                BuildingName = building.Name,
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
        
        public List<ShiftCaptain.Models.RoomHour> GetRoomHours(IDictionary<string, string> roomObj, RoomInstance roomInstance)
        {
            return new List<RoomHour>{
                GetRoomHour(roomInstance.Id, 0, Clean<TimeSpan?>(roomObj, "SUNDAY_START_TIME"), Clean<Decimal?>(roomObj, "SUNDAY_DURATION")),
                GetRoomHour(roomInstance.Id, 1, Clean<TimeSpan?>(roomObj, "MONDAY_START_TIME"), Clean<Decimal?>(roomObj, "MONDAY_DURATION")),
                GetRoomHour(roomInstance.Id, 2, Clean<TimeSpan?>(roomObj, "TUESDAY_START_TIME"), Clean<Decimal?>(roomObj, "TUESDAY_DURATION")),
                GetRoomHour(roomInstance.Id, 3, Clean<TimeSpan?>(roomObj, "WEDNESDAY_START_TIME"), Clean<Decimal?>(roomObj, "WEDNESDAY_DURATION")),
                GetRoomHour(roomInstance.Id, 4, Clean<TimeSpan?>(roomObj, "THURSDAY_START_TIME"), Clean<Decimal?>(roomObj, "THURSDAY_DURATION")),
                GetRoomHour(roomInstance.Id, 5, Clean<TimeSpan?>(roomObj, "FRIDAY_START_TIME"), Clean<Decimal?>(roomObj, "FRIDAY_DURATION")),
                GetRoomHour(roomInstance.Id, 6, Clean<TimeSpan?>(roomObj, "SATURDAY_START_TIME"), Clean<Decimal?>(roomObj, "SATURDAY_DURATION"))
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
