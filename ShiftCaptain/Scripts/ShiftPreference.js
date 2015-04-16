var dragger = dnd.drag;
var sc = ShiftCaptain;

var outputTime = function (time) {
    return time.Hours + ":" + time.Minutes;
};
var createShiftElement = function (shift, s) {
    var cols = shift.Duration * 2;
    return addDraggerFunctions('shift', $("<td class='taken draggable' style='background-color: " + shift.Color + ";' s='" + s + "' colSpan='" + cols + "' shiftpreferenceid='" + shift.ShiftPreferenceId + "' starttime='" + outputTime(shift.StartTime) + "' duration='" + shift.Duration + "' userid='" + shift.UserId + "'>" + shift.NickName + "</td>").tooltip('shiftpreference', shift));
};
var getHoursForDay = function(hours, dayId){
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
        if (minStart != null) {
            compacted.push({ Day: day, Duration: maxEnd - (minStart.Hours + minStart.Minutes / 60), StartTime: minStart });
        }
    }
    console.log("hours", compacted);
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
    return addDraggerFunctions('pref', $("<div class='small-6 large-3 columns'><preference style='background-color: "+ preference.Color + ";' class='draggable has-tip' preferenceid='" + preference.PreferenceId + "'>" + preference.Name + "</user></div>").tooltip(
        'preference', preference
        ));
};
sc.Preference.get(null, function (preferences) {
    var prefHolder = $("#Preferences .row");
    prefHolder.empty();
    for (var idx = 0; idx < preferences.length ; idx++) {
        prefHolder.append(createPreference(preferences[idx]));
    }
    sc.app.resizeHeader();
    $("#shiftHolder table").trigger("page-ready");
}, function (error) {
    sc.app.displayError(error);
});

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
    var orig = $element;
    if (elementType == "shift") {
        $element.on('draggedout', draggedout);
        $element.on('dragstart', dragStart);
    } else if (elementType == "user") {
        $element = $($element.find("user"));
    } else if (elementType == "pref") {
        $element = $($element.find("preference"));
    }
    $element.on('canceldrag', cancelDrag);
    $element.on('movedaround', movedAround);
    $element.on('validatedrop', validateDrop);
    return orig;
};
var cancelDrag = function (event, temp) {
    if (temp != this) {
        $(temp).replaceWith(this);
    }
    $(".temp").each(function (index) {
        $(this).remove();
    });
};
var movedAround = function (event, data) {

    var $dragElement = $(this);
    var $dropElement = $(data.target);
    sc.app.updateTemp();
    var shiftInfo = getShiftInfo($dragElement, $dropElement);

    var updateOrCreate = shiftInfo.ShiftPreferenceId != null ? "update" : "create";
    var temp = data.temp;
    sc.ShiftPreference[updateOrCreate](shiftInfo, function (shift) {
        dragComplete(sc.app.replaceDropElementWithNewShift(createShiftElement(shift[0], shiftInfo.s), $dropElement), $(temp));
    });
};
var validateDrop = function (event, data) {
    console.log("validating drop start");
    var callback = data.callback;
    var $dragElement = $(this);
    var $dropElement = $(data.target);
    var span = parseFloat($("#ShiftDuration").val()) * 2;
    var next = $dropElement;
    for (var idx = 0; idx < span - 1 ; idx++) {
        next = next.next();
        if (!next.hasClass(dragger.settings.canDropClass)) {
            return false;
        }
    }
    var shiftInfo = getShiftInfo($dragElement, $dropElement);
    console.log("validating drop", $dropElement);
    sc.ShiftPreference.validate(shiftInfo, callback, function (xhr, textStatus, errorThrown) {
        console.log("error validating drop", $dropElement);
        sc.app.displayError(xhr.responseText);
        callback(false);
    });
    return true;
};
var draggedout = function (temp) {
    sc.app.updateTemp();

    var shiftpreferenceid = $(this).attr('shiftpreferenceid');
    $.cleanData(this.getElementsByTagName("*"));
    $.cleanData([this]);
    if (shiftpreferenceid) {//Is a shift so remove it
        sc.ShiftPreference.remove(shiftpreferenceid, function (success) {
            dragComplete(null, $(temp));
        });
    }
};
var dragStart = function () {
    var $element = $(this);
    if ($element.hasClass("taken")) {
        return replaceWithOpen($element, true);
    }
};
var dragComplete = function ($newShift, $temp) {
    //check old row to see if it's now empty
    var tr = $temp.closest('tr');
    var dayName = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    var day = parseInt(tr.attr('day'));
    if (!tr.hasClass('first-row') && tr.find(".taken").length == 0 && $(".drop-section ." + dayName[day]).length > 1) {
        //row is empty.   AND not the only row for this day         
        tr.remove();
    }
    //we don't want an empty row for this page.
    //if ($newShift) {
    //    //check to see if day has an "empty-row"
    //    tr = $newShift.closest('tr');
    //    day = parseInt(tr.attr('day'));
    //    var oldEmptyRow = $(".drop-section ." + dayName[day] + ".empty-row");
    //    if (oldEmptyRow.find(".taken").length > 0) {
    //        oldEmptyRow.removeClass("empty-row");
    //        var fakeBody = $("<fake></fake>");
    //        var dayData = currentRoomHours[day];
    //        sc.app.makeRow(1/* just can't be zero*/, fakeBody, dayData, dayData.s, dayData.e, [], dayData.MinStart, dayData.MaxEnd, createShiftElement);
    //        $(fakeBody.children()).insertAfter(oldEmptyRow);
    //    }
    //}
};