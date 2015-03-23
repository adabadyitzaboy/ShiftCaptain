var dragger = dnd.drag;
var sc = ShiftCaptain;

$("#BuildingID").change(function (val) {
    var buildingId = $(this).val();
    sc.Room.get(buildingId, function (data) {
        $("#RoomID").empty();
        $("#RoomID").append(data);
        //$("#RoomID").trigger('change');
    });
});

var outputTime = function (time) {
    return time.Hours + ":" + time.Minutes;
};
var createShiftElement = function (shift, s) {
    var cols = shift.Duration * 2;
    return addDraggerFunctions('shift', $("<td class='taken draggable' s='" + s + "' colSpan='" + cols + "' shiftid='" + shift.ShiftId + "' starttime='" + outputTime(shift.StartTime) + "' duration='" + shift.Duration + "' userid='" + shift.UserId + "'>" + shift.NickName + "</td>").tooltip('shift', shift));
};
$("#RoomID").change(function (val) {
    var roomId = $(this).val();
    sc.Room.getHours(roomId, function (roomHours) {
        sc.app.setCurrentHours(roomHours);
        sc.Shift.get(roomId, function (roomShifts) {
            sc.app.createShiftTable(roomHours, roomShifts, createShiftElement, shiftOptions);
        }, function (error) {
            sc.app.displayError(error);
        });
    }, function (error) {
        sc.app.displayError(error);
    });
});
$("#RoomID").trigger('change');

var createUser = function (employee) {
    employee.colorClass = "";
    if (employee.CurrentHours > employee.MaxHours) {
        employee.colorClass = "over-max-hours";
    } else if (employee.CurrentHours < employee.MinHours) {
        employee.colorClass = "under-min-hours";
    }
    return addDraggerFunctions('user', $.template("user", employee).tooltip('user', employee));
};
var shiftOptions = {};
if ($("#Employees").length > 0) {
} else {
    shiftOptions.empty_row = false;
}
sc.User.get(null, function (employees) {
    var empHolder = $("#Employees .row");
    empHolder.empty();
    for (var idx = 0; idx < employees.length ; idx++) {
        empHolder.append(createUser(employees[idx]));
    }
    sc.app.resizeHeader();
    $("#shiftHolder table").trigger("page-ready");
});

var getShiftInfo= function($dragElement, $dropElement){    
    var roomId = $("#RoomID").val();
    var userId = $dragElement.attr("userid");
    var duration = $("#ShiftDuration").val();
    var s = $dropElement.attr("s");
    var startTime = (s | 0) + ":" + (s % 1) * 60;
    var day = $dropElement.closest("tr").attr("day");
    var shiftId = $dragElement.attr('shiftid');
    return {
        ShiftId: shiftId,
        roomId: roomId,
        userId: userId,
        duration: duration,
        s: s,
        startTime: startTime,
        day: day
    };
};

var addDraggerFunctions = function (elementType, $element) {
    var orig = $element;
    if (elementType == "shift") {
        $element.on('draggedout', draggedout);
        $element.on('dragstart', dragStart);
    } else if (elementType == "user") {
        $element = $($element.find("user"));
    }
    $element.on('canceldrag', cancelDrag);
    $element.on('movedaround', movedAround);
    $element.on('validatedrop', validateDrop);
    return orig;
};
var cancelDrag = function (event, temp) {
    if (temp != this) {
        $(temp).replaceWith(this);
        $(".temp").each(function (index) {
            $(this).remove();
        });
    }
    removePreferences();
};
var movedAround = function (event, data) {

    var $dragElement = $(this);
    var $dropElement = $(data.target);
    var userid = $(this).attr('userid');
    sc.app.updateTemp();
    var shiftInfo = getShiftInfo($dragElement, $dropElement);

    var updateOrCreate = shiftInfo.ShiftId != null ? "update" : "create";
    var temp = data.temp;
    sc.Shift[updateOrCreate](shiftInfo, function (shift) {
        dragComplete(sc.app.replaceDropElementWithNewShift(createShiftElement(shift[0], shiftInfo.s), $dropElement), $(temp), userid);
    });
};
var validateDrop = function (event, data) {
    var callback = data.callback;
    var $dragElement = $(this);
    var $dropElement = $(data.target);
    var span = parseFloat($("#ShiftDuration").val()) * 2;
    var next = $dropElement;
    for (var idx = 0; idx < span - 1 ; idx++) {
        next = next.next();
        if (!next.hasClass(dragger.settings.canDropClass) || (next.hasClass("preference") && next.attr('canwork') == "false")) {
            return false;
        }
    }
    var shiftInfo = getShiftInfo($dragElement, $dropElement);
    sc.Shift.validate(shiftInfo, callback, function (xhr, textStatus, errorThrown) {
        sc.app.displayError(xhr.responseText);
        callback(false);
    });
    return true;
};
var draggedout = function (event, temp) {
    sc.app.updateTemp();
    
    var shiftid = $(this).attr('shiftid');
    var userid = $(this).attr('userid');
    $.cleanData(this.getElementsByTagName("*"));
    $.cleanData([this]);
    if (shiftid) {//Is a shift so remove it
        sc.Shift.remove(shiftid, function (success) {
            dragComplete(null, $(temp), userid);
        });
    }
    removePreferences();    
};
var dragStart = function () {
    var $element = $(this);
    updatePreferences($element.attr('userid'));
    if ($element.hasClass("taken")) {
        return replaceWithOpen($element, true);
    }
};
var dragComplete = function ($newShift, $temp, userid) {
    //check old row to see if it's now empty
    var tr = $temp.closest('tr');
    var dayName = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    var day = parseInt(tr.attr('day'));
    if (!tr.hasClass('first-row') && tr.find(".taken").length == 0 && $(".drop-section ." + dayName[day]).length > 1) {
        //row is empty.   AND not the only row for this day         
        tr.remove();
    }
    if ($newShift && shiftOptions.empty_row != false) {
        //check to see if day has an "empty-row"
        tr = $newShift.closest('tr');
        day = parseInt(tr.attr('day'));
        var oldEmptyRow = $(".drop-section ." + dayName[day] + ".empty-row");
        if (oldEmptyRow.find(".taken").length > 0) {
            oldEmptyRow.removeClass(".empty-row");
            var fakeBody = $("<fake></fake>");
            var dayData = currentRoomHours[day];
            sc.app.makeRow(1/* just can't be zero*/, fakeBody, dayData, dayData.s, dayData.e, [], dayData.MinStart, dayData.MaxEnd, createShiftElement);
            $(fakeBody.children()).insertAfter(oldEmptyRow);
        }
        
    }

    //createUser
    sc.User.get(userid, function (employees) {
        for (var idx = 0; idx < employees.length ; idx++) {
            var uId= employees[idx].UserId;//just in case
            $("#Employees .row user[userid='" + uId + "'").parent().replaceWith(dragger.reinit(createUser(employees[idx])));
        }

    });
    removePreferences();
};

var updatePreferences = function (userid) {
    sc.ShiftPreference.get(userid, function (shifts) {
        for (var idx = 0; idx < shifts.length; idx++) {
            var shift = shifts[idx];
            var start = shift.StartTime.Hours + shift.StartTime.Minutes / 60;
            for (var durationIdx = 0; durationIdx < shift.Duration; durationIdx += .5) {
                var elements = $("[day='" + shift.Day + "'] .open[s='" + (start + durationIdx) + "']");
                for (var elementIdx = 0; elementIdx < elements.length; elementIdx++) {
                    var $element = $(elements[elementIdx]);
                    $element.addClass("preference");
                    $element.css('background-color', shift.Color);
                    $element.attr('canwork', shift.CanWork);
                }
            }

        }
    }, function () {
        //TODO error handling
    });
};
var removePreferences = function () {
    var preferences = $(".preference");
    for (var idx = 0; idx < preferences.length; idx++) {
        var $pref = $(preferences[idx]);
        $pref.removeClass("preference");
        $pref.css('background-color', "#fff");
        $pref.attr('canwork', null);
    }
};