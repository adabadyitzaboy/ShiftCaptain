var dragger = dnd.drag;
var sc = ShiftCaptain;

var outputTime = function (time) {
    return time.Hours + ":" + time.Minutes;
};
var createShiftElement = function (shift, s) {
    var cols = shift.Duration * 2;
    return addDraggerFunctions('shift', $("<td class='taken draggable' style='background-color: " + shift.Color + ";' s='" + s + "' colSpan='" + cols + "' shiftpreferenceid='" + shift.ShiftPreferenceId + "' starttime='" + outputTime(shift.StartTime) + "' duration='" + shift.Duration + "' userid='" + shift.UserId + "'>" + shift.NickName + "</td>").tooltip('shiftpreference', shift));
};
var getHoursForDay = function (hours, dayId) {
    var hoursForDay = [];
    for (var idx = 0; idx < hours.length; idx++) {
        if (hours[idx].Day == dayId) {
            hoursForDay.push(hours.splice(idx--, 1)[0]);
        }
    }
    return hoursForDay;
};
var compactDays = function (hours) {
    var compacted = [];
    for (var day = 0; day < 7; day++) {
        var dayHours = getHoursForDay(hours, day);
        var minStart = null;
        var maxEnd = null;
        for (var idx = 0; idx < dayHours.length; idx++) {
            if (minStart == null || (dayHours[idx].StartTime.Hours < minStart.Hours && (dayHours[idx].StartTime.Hours + dayHours[idx].StartTime.Minutes / 60 < minStart.Hours + minStart.Minutes / 60))) {
                minStart = dayHours[idx].StartTime;
            }
            var end = dayHours[idx].StartTime.Hours + (dayHours[idx].StartTime.Minutes / 60) + dayHours[idx].Duration;
            if (maxEnd == null || end > maxEnd) {
                maxEnd = end;
            }
        }
        compacted.push({ Day: day, Duration: maxEnd - (minStart.Hours + minStart.Minutes / 60), StartTime: minStart });
    }
    return compacted;
};
var roomHours = [];
sc.Room.getHours(null, function (hours) {
    roomHours = compactDays(hours);
    sc.app.setCurrentHours(roomHours);
    $("#UserID").trigger('change');
}, function (error) {
    sc.app.displayError(error);
});
var createPreference = function (preference) {
    return addDraggerFunctions('pref', $("<div class='small-6 large-3 columns'><preference style='background-color: " + preference.Color + ";' class='draggable has-tip' preferenceid='" + preference.PreferenceId + "'>" + preference.Name + "</user></div>").tooltip(
        'preference', preference
        ));
};

$("#UserID").change(function (val) {
    var userId = $(this).val();
    sc.ShiftPreference.get(userId, function (preferences) {
        sc.app.createShiftTable(roomHours, preferences, createShiftElement, { empty_row: false, border_row: false });
    });
});
var getShiftInfo = function ($dragElement, $dropElement) {
    var userId = $("#UserID").val();
    var preferenceId = $dragElement.attr("preferenceid");
    var shiftPreferenceId = $dragElement.attr("shiftpreferenceid");
    var duration = $("#ShiftDuration").val();
    var s = $dropElement.attr("s");
    var startTime = (s | 0) + ":" + (s % 1) * 60;
    var day = $dropElement.closest("tr").attr("day");
    return {
        PreferenceId: preferenceId,
        ShiftPreferenceId: shiftPreferenceId,
        UserId: userId,
        Duration: duration,
        s: s,
        StartTime: startTime,
        Day: day
    };
};

var addDraggerFunctions = function (elementType, $element) {
    return $element;
};
