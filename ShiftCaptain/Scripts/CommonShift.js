var dragger = dnd.drag;
var sc = ShiftCaptain || {};
var typing = null;
var typeData = "";
var doneTyping = function () {
    if (typeData != "") {
        var duration = $("#ShiftDuration");
        var exists = duration.find("[value='" + typeData + "']");
        if (exists.length) {
            duration.val(typeData);
        } else {
            var isDecimal = true;
            try{
                parseFloat(typeData).toFixed(1);
            } catch (ex) {
                isDecimal = false;
                console.log("Duration must be a decimal or integer - " + typeData);
            }
            if (isDecimal) {
                var custom = duration.find(".custom");
                if (!custom.length) {
                    custom = $("<option class='custom' value='" + typeData + "'>" + typeData + " hours</option>");
                    duration.append(custom);
                } else {
                    custom.val(typeData);
                    custom.text(typeData + " hours");
                }
                duration.val(typeData);

            }
        }
        typeData = "";
    }
};
$(document.body).on('keypress', function (key) {
    if (!key.ctrlKey && !key.shiftKey && ((key.charCode >= 48 && key.charCode <= 57) || key.charCode == 46)) {
        if (key.charCode == 46) {
            typeData += ".";
        } else {
            typeData += (key.charCode - 48);
        }
        if (typing != null) {
            clearTimeout(typing);
        }
        typing = setTimeout(function () {
            doneTyping();
        }, 500);
    } else if (typing) {
        clearTimeout(typing);
        typing = null;
    }
});
var openTD = function (s, classname, extra) {
    return "<td class='open " + (classname || "") + "' s='" + s + "' " + (extra || "") + ">&nbsp;</td>";
}


var displayError = function (response) {
    var errorHolder = $("#Errors");
    errorHolder.empty();
    var li = $("<li>");
    li.text(JSON.parse(response).error);
    errorHolder.append($("<ul>").html(li));
    sc.app.resizeHeader();
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
var createShiftElement = function (shift, s) {
    var cols = shift.Duration * 2;
    return addDraggerFunctions('shift', $("<td class='taken draggable' s='" + s + "' colSpan='" + cols + "' shiftid='" + shift.ShiftId + "' starttime='" + outputTime(shift.StartTime) + "' duration='" + shift.Duration + "' userid='" + shift.UserId + "'>" + shift.NickName + "</td>").tooltip('shift', shift));
};
var currentRoomHours = [];
var dayName = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
var makeRow = function (rC, tbody, dayData, s, e, shifts, start, maxEnd, createShiftElement, options) {
    options = options || {};
    var emptyRow = true;
    var tr = $("<tr class='" + dayName[dayData.Day] + "' day='" + dayData.Day + "'><td>" + (rC == 0 ? dayName[dayData.Day] : "&nbsp;") + "</td></tr>");
    for (var notOpen = start; notOpen < s; notOpen += .5) {
        tr.append("<td class='closed'>&nbsp;</td>");
    }
    for (var t = s; t < e; t += .5) {
        var shift = getShiftAtTime(t % 24, shifts);
        if (shift) {
            emptyRow = false;
            tr.append(createShiftElement(shift, t % 24));
            t += shift.Duration - .5;
        } else {
            tr.append(openTD(t % 24));
        }
    }

    for (var notOpen = e; notOpen < maxEnd; notOpen += .5) {
        tr.append("<td class='closed' s='" + (notOpen % 24) + "'>&nbsp;</td>");
    }
    tbody.append(tr);
    if (rC == 0) {
        tr.addClass('first-row');
    }
    if (shifts.length > 0) {
        //recursive call
        if (emptyRow) {
            console.log("Unable to schedule the rest of the shifts");
            console.log(shifts);
        } else {
            makeRow(++rC, tbody, dayData, s, e, shifts, start, maxEnd, createShiftElement, options);
        }
    } else if (!emptyRow) {
        if (options.empty_row != false) {
            makeRow(++rC, tbody, dayData, s, e, shifts, start, maxEnd, createShiftElement, options);
        }
    } else {
        tr.addClass('empty-row');
    }
};
$("#shiftHolder table").on("table-ready page-ready", function (event) {
    this["-" + event.type] = true;
    if (this["-table-ready"] && this["-page-ready"] && !this["-dragger-init"]) {
        this["-dragger-init"] = true;
        dragger.init();
    }
});
var createShiftTable = function (roomHours, shifts, createShiftElement, options) {
    options = options || {};
    var sh = $("#shiftHolder table");
    sh.empty();
    var minStart = null;
    var maxEnd = null;
    $.each(roomHours, function (index) {
        if (minStart == null || (this.StartTime.Hours < minStart.Hours && (this.StartTime.Hours + this.StartTime.Minutes / 60 < minStart.Hours + minStart.Minutes / 60))) {
            minStart = this.StartTime;
        }
        var end = this.StartTime.Hours + (this.StartTime.Minutes / 60) + this.Duration;
        if (maxEnd == null || end > maxEnd) {
            maxEnd = end;
        }
    });
    var start = minStart.Hours + minStart.Minutes / 60;
    if (start == null || maxEnd == null || start > maxEnd) {
        return;
    }
    var topRow = $("<tr><th>&nbsp;</th></tr>");
    for (var idx = start; idx < maxEnd; idx += .5) {
        topRow.append($("<th class='" + (idx >= 13 && idx < 24 ? "PM" : "AM") + "'>" + ((idx >= 13 ? (idx >= 24 ? idx - 24 : idx - 12) : idx) | 0) + ":" + (idx % 1 == 0.5 ? "30" : "00") + "</th>"));
    }
    var thead = $("<thead></thead>").append(topRow);
    //var theadFixed = thead.clone();
    var theadBottom = thead.clone();
    sh.append(thead);
    //sh.append(thead.css("visibility", "hidden"));
    //sh.append(theadFixed.css("position", "fixed"));
    var tbody = $("<tbody></tbody>");
    var borderRow = null;
    for (var idx = 0; idx < roomHours.length; idx++) {
        var dayroomHours = roomHours[idx];
        var dayShifts = FilterShifts(shifts, dayroomHours.Day);
        if (dayShifts.length > 0) {
            window.a = dayShifts;
        }
        dayroomHours.s = dayroomHours.StartTime.Hours + dayroomHours.StartTime.Minutes / 60;
        dayroomHours.e = dayroomHours.s + dayroomHours.Duration;
        dayroomHours.MinStart = start;
        dayroomHours.MaxEnd = maxEnd;
        makeRow(0, tbody, dayroomHours, dayroomHours.s, dayroomHours.e, dayShifts, start, maxEnd, createShiftElement, options);


        if (idx < roomHours.length - 1) {
            //add border row
            if (options.border_row != false && !borderRow) {
                borderRow = $("<tr class='row-border'><td>&nbsp;</td></tr>");
                for (var notOpen = start; notOpen < maxEnd; notOpen += .5) {
                    borderRow.append("<td t='" + notOpen + "'>&nbsp;</td>");
                }
            }
            if (borderRow) {
                tbody.append(borderRow.clone(true));
            }
        }
    }
    sh.append(tbody);
    //theadFixed.css('top', thead.offset().top);
    //theadFixed.find("th:first").width(tbody.find("tr:first-child td:first-child").width()).css('display', 'block');
    sh.append(theadBottom);
    sh.trigger("table-ready");
};

var replaceWithOpen = function ($element, temp) {
    var s = parseFloat($element.attr("s"));
    var replacement = "" + openTD(s, temp?"temp": "");
    var span = $element.attr('colSpan');
    for (var idx = 0; idx < span - 1 ; idx++) {
        s += .5;
        replacement += openTD(s, temp ? "temp" : "");
        
    }
    var $replacement = $(replacement);
    var next = $element.nextSibling;
    $element.replaceWith($replacement, true);
    console.log($replacement);
    return $replacement;
};
var replaceDropElementWithNewShift = function ($newShift, $dropElement) {
    var span = $newShift.attr('colSpan');
    var next = $dropElement;
    for (var idx = 0; idx < span ; idx++) {
        var prev = next;
        next = next.next();
        if (idx != 0) {
            prev.remove();
        }
    }
    $dropElement.replaceWith($newShift);
    dragger.reinit($newShift);
    return $newShift;
};

var updateTemp = function () {
    $(".temp").each(function (index) {
        var $elem = $(this);
        $elem.removeClass("temp");
    });
};
sc.app = sc.app || {};
$.extend(sc.app, {
    createShiftTable: createShiftTable,
    displayError: displayError,
    makeRow: makeRow,
    replaceWithOpen: replaceWithOpen,
    replaceDropElementWithNewShift: replaceDropElementWithNewShift,
    setCurrentHours: function (roomHours) {
        currentRoomHours = roomHours;
    },
    updateTemp: updateTemp
});

