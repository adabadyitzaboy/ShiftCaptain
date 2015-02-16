$.ajaxSetup({ global: false });
var dragger = dnd.drag;
var sc = ShiftCaptain;

var openTD = function (s, classname, extra) {
    return "<td class='open " + (classname || "") + "' s='" + s + "' " + (extra || "") + ">&nbsp;</td>";
}

$("#BuildingID").change(function (val) {
    var buildingId = $(this).val();
    sc.Room.get(buildingId, function (data) {
        $("#RoomID").empty();
        $("#RoomID").append(data);
        //$("#RoomID").trigger('change');
    });
});
var displayError = function (response) {
    var errorHolder = $("#Errors ul");
    errorHolder.empty();
    var li = $("<li></li>");
    li.text(JSON.parse(response).error);
    errorHolder.append(li);
};
var FilterShifts = function (shifts, dayId) {
    var filtered = [];
    for (var idx = 0; idx < shifts.length; idx++) {
        if (shifts[idx].Day == dayId) {
            shifts[idx].s = shifts[idx].StartTime.Hours + shifts[idx].StartTime.Minutes / 60;
            filtered.push(shifts[idx]);
        }
    };
    return filtered;
};
var getShiftAtTime = function (startTime, data) {
    for (var idx = 0; idx < data.length; idx++) {
        if (data[idx].s == startTime) {
            return data.splice(idx, 1)[0];
        }
    }
};

var outputTime = function (time) {
    return time.Hours + ":" + time.Minutes;
};
var createShiftString = function (shift, s) {
    var cols = shift.Duration.Hours * 2 + (shift.Duration.Minutes == 30 ? 1 : 0);
    return "<td class='taken draggable' s='" + s+ "' colSpan='" + cols + "' shiftid='" + shift.ShiftId + "' starttime='" + outputTime(shift.StartTime) + "' duration='" + outputTime(shift.Duration) + "' userid='"+ shift.UserId + "'>" + shift.NickName + "</td>";
};
var currentRoomHours = [];
var dayName = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
var makeRow = function (rC, tbody, dayData, s, e, shifts, start, maxEnd) {
    var emptyRow = true;
    var tr = $("<tr class='" + dayName[dayData.Day] + "' day='" + dayData.Day + "'><td>" + (rC == 0 ? dayName[dayData.Day] : "&nbsp;") + "</td></tr>");
    for (var notOpen = start; notOpen < s; notOpen += .5) {
        tr.append("<td class='closed'>&nbsp;</td>");
    }
    for (var t = s; t < e; t += .5) {
        var shift = getShiftAtTime(t, shifts);
        if (shift) {
            emptyRow = false;
            tr.append(createShiftString(shift, t));
            t += shift.Duration.Hours + (shift.Duration.Minutes / 60) - .5;
        } else {
            tr.append(openTD(t));
        }
    }

    for (var notOpen = e; notOpen < maxEnd; notOpen += .5) {
        tr.append("<td class='closed' s='" + notOpen + "'>&nbsp;</td>");
    }
    tbody.append(tr);
    if (shifts.length > 0) {
        //recursive call
        if (rC == 0) {
            tr.addClass('first-row');
        }
        makeRow(++rC, tbody, dayData, s, e, shifts, start, maxEnd);
    } else if (!emptyRow) {
        makeRow(++rC, tbody, dayData, s, e, shifts, start, maxEnd);
    } else {
        tr.addClass('empty-row');
    }
};

$("#RoomID").change(function (val) {
    var roomId = $(this).val();
    sc.Room.getHours(roomId, function (data) {
        currentRoomHours = data;
        sc.Shift.get(roomId, function (roomShifts) {
            var sh = $("#shiftHolder table");
            sh.empty();
            var minStart = null;
            var maxEnd = null;
            $.each(data, function (index) {
                if (minStart == null || this.StartTime < minStart) {
                    minStart = this.StartTime;
                }
                var end = this.StartTime.Hours + this.Duration.Hours + (this.StartTime.Minutes + this.Duration.Minutes) / 60;
                if (maxEnd == null || end > maxEnd) {
                    maxEnd = end;
                }
            });
            var start = minStart.Hours + minStart.Minutes / 60;
            if (start == null || maxEnd == null || start > maxEnd) {
                return;
            }
            var topRow = $("<tr><td>&nbsp;</td></tr>");
            for (var idx = start; idx < maxEnd; idx += .5) {
                topRow.append($("<th class='" + (idx >= 13 ? "PM" : "AM") + "'>" + ((idx >= 13 ? idx - 12 : idx) | 0) + ":" + (idx % 1 == 0.5 ? "30" : "00") + "</th>"));
            }
            sh.append($("<thead></thead>").append(topRow));
            var tbody = $("<tbody></tbody>");
            var borderRow;
            for (var idx = 0; idx < data.length; idx++) {
                var dayData = data[idx];
                var dayShifts = FilterShifts(roomShifts, dayData.Day);
                if (dayShifts.length > 0) {
                    window.a = dayShifts;
                }
                dayData.s = dayData.StartTime.Hours + dayData.StartTime.Minutes / 60;
                dayData.e = dayData.s + dayData.Duration.Hours + dayData.Duration.Minutes / 60;
                dayData.MinStart = start;
                dayData.MaxEnd = maxEnd;
                makeRow(0, tbody, dayData, dayData.s, dayData.e, dayShifts, start, maxEnd);

                
                if (idx < data.length - 1) {
                    //add border row
                    if (!borderRow) {
                        borderRow = $("<tr class='row-border'><td>&nbsp;</td></tr>");
                        for (var notOpen = start; notOpen < maxEnd; notOpen += .5) {
                            borderRow.append("<td t='" + notOpen + "'>&nbsp;</td>");
                        }
                    }
                    tbody.append(borderRow.clone(true));
                }
            }
            sh.append(tbody);
            dragger.init();
        }, function (error) {
            displayError(error);
        });
    }, function (error) {
        displayError(error);
    });
});
$("#RoomID").trigger('change');
var timeToDecimal = function (time) {
    return time.Hours + time.Minutes/60;
}
var createUser = function (employee) {
    var colorClass = "";
    if (employee.CurrentHours > timeToDecimal(employee.MaxHours)) {
        colorClass = "over-max-hours";
    } else if (employee.CurrentHours < timeToDecimal(employee.MinHours)) {
        colorClass = "under-min-hours";
    }
    return $("<user class='small-6 large-3 columns draggable " + colorClass + "' minhours='" + outputTime(employee.MinHours) + "' maxhours='" + outputTime(employee.MaxHours) + "' currenthours='" + employee.CurrentHours + "' employeeid='" + employee.EmployeeId + "' userid='" + employee.UserId + "'>" + employee.NickName + "</user>");
};
sc.User.get(null, function (employees) {
    var empHolder = $("#Employees .row");
    empHolder.empty();
    for (var idx = 0; idx < employees.length ; idx++) {
        empHolder.append(createUser(employees[idx]));
    }

});
var replaceWithOpen = function ($element, temp) {
    var s = parseInt($element.attr("s"));
    var replacement = "" + openTD(s, temp?"temp": "", temp?"tempid='" + $element.attr('shiftid') + "'":"");
    var span = $element.attr('colSpan');
    for (var idx = 0; idx < span - 1 ; idx++) {
        replacement += openTD(s + idx + 1, temp ? "temp" : "");
    }
    var $replacement = $(replacement);
    $element.replaceWith($replacement);
    return $replacement;
};
var dropElementFN = function ($dragElement, $dropElement) {
    var span = $dragElement.attr('colSpan');
    var next = $dropElement;
    for (var idx = 0; idx < span ; idx++) {
        var prev = next;
        next = next.next();
        if (idx != 0) {
            prev.remove();
        }
    }
    $dropElement.replaceWith($dragElement);
    dragger.reinit($dragElement);
};

dragger.addFunction('onCancelDrag', function (dragElement, temp) {
    $(temp).replaceWith(dragElement);
    $(".temp").each(function (index) {
        $(this).remove();
    });
    dragger.reinit(dragElement);
});
var getShiftInfo= function($dragElement, $dropElement){    
    var roomId = $("#RoomID").val();
    var userId = $dragElement.attr("userid");
    var duration = $("#ShiftDuration").val();
    duration = (duration | 0) + ":" + (duration % 1) * 60
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
dragger.addFunction('onMovedAround', function (dragElement, dropElement, temp) {
    var $dragElement = $(dragElement);
    var $dropElement = $(dropElement);
    updateTemp();
    var shiftInfo = getShiftInfo($dragElement, $dropElement);

    var updateOrCreate = shiftInfo.ShiftId != null? "update": "create";
    sc.Shift[updateOrCreate](shiftInfo, function (shift) {
        var $newShift = $(createShiftString(shift[0], shiftInfo.s));
        dropElementFN($newShift, $dropElement);
        dragComplete($newShift, $(temp));
    });
});
var updateTemp = function () {
    $(".temp").each(function (index) {
        var $elem = $(this);
        $elem.removeClass("temp");
        if ($elem.attr('tempid')) {
            $elem.attr('tempid', "");
        }
    });
};
dragger.addFunction('validateDrop', function (dragElement, dropElement, callback) {
    var $dragElement = $(dragElement);
    var $dropElement = $(dropElement);
    var span = parseFloat($("#ShiftDuration").val()) * 2;
    var next = $dropElement;
    for (var idx = 0; idx < span - 1 ; idx++) {
        next = next.next();
        if (!$(next).hasClass(dragger.settings.canDropClass)) {
            return false;
        }
    }
    var shiftInfo = getShiftInfo($dragElement, $dropElement);
    sc.Shift.validate(shiftInfo, callback, function (xhr, textStatus, errorThrown) {
        displayError(xhr.responseText);
        callback(false);
    });
    return true;
});
dragger.addFunction('onDraggedOut', function (element, temp) {
    var $element = $(element);
    updateTemp();
    sc.Shift.remove($element.attr('shiftid'), function (success) {
        dragComplete(null, $(temp));
    });
});

dragger.addFunction('onDragStart', function (element) {
    var $element = $(element);
    if ($element.hasClass("taken")) {
        return replaceWithOpen($element, true);
    }
});
var dragComplete = function ($newShift, $temp) {
    //check old row to see if it's now empty
    var tr = $temp.closest('tr');
    var dayName = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    var day = parseInt(tr.attr('day'));
    if (!tr.hasClass('first-row') && tr.find(".taken").length == 0 && $(".drop-section ." + dayName[day]).length > 1) {
        //row is empty.   AND not the only row for this day         
        tr.remove();
    }
    if ($newShift) {
        //check to see if day has an "empty-row"
        tr = $newShift.closest('tr');
        day = parseInt(tr.attr('day'));
        var oldEmptyRow = $(".drop-section ." + dayName[day] + ".empty-row");
        if (oldEmptyRow.find(".taken").length > 0) {
            oldEmptyRow.removeClass(".empty-row");
            var fakeBody = $("<fake></fake>");
            var dayData = currentRoomHours[day];
            makeRow(1/* just can't be zero*/, fakeBody, dayData, dayData.s, dayData.e, [], dayData.MinStart, dayData.MaxEnd);
            $(fakeBody.children()).insertAfter(oldEmptyRow);
        }
    }

    //createUser
    sc.User.get($newShift.attr('userid'), function (employees) {
        for (var idx = 0; idx < employees.length ; idx++) {
            var userId = employees[idx].UserId;
            $("#Employees .row user[userid='" + userId + "'").replaceWith(createUser(employees[idx]));
        }

    });
};