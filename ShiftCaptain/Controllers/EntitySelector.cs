using ShiftCaptain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShiftCaptain.Helpers
{
    public static class EntitySelector
    {
        public static IQueryable<object> SelectShiftPreference(IQueryable<ShiftPreference> shifts)
        {
            return shifts.Select(s => new { s.VersionId, ShiftPreferenceId = s.Id, s.User.NickName, s.Day, s.StartTime, s.Duration, s.UserId, s.PreferenceId, s.Preference.CanWork, s.Preference.Color });
        }

        public static IQueryable<object> SelectPreference(IQueryable<Preference> preference)
        {
            return preference.Select(p => new { PreferenceId = p.Id, p.CanWork, p.Name, p.Description, p.Color });
        }

        public static IQueryable<object> SelectRoomInstance(IQueryable<RoomView> roomInstance)
        {
            return roomInstance.Select(rv => new { rv.RoomInstanceId, rv.Name });
        }

        public static IQueryable<object> SelectRoomHours(IQueryable<RoomHour> roomHours)
        {
            return roomHours.Select(rh => new { rh.Day, rh.StartTime, rh.Duration });
        }

        public static IQueryable<object> SelectShift(IQueryable<Shift> shifts)
        {
            return shifts.Select(s => new { s.VersionId, ShiftId = s.Id, s.User.NickName, s.RoomId, s.Day, s.StartTime, s.Duration, s.UserId });
        }

        public static IQueryable<object> SelectUserInstance(IQueryable<UserView> userView)
        {
            return userView.Select(uv => new { uv.NickName, uv.UserId, uv.MinHours, uv.MaxHours, uv.EmployeeId, uv.CurrentHours });
        }
    }
}