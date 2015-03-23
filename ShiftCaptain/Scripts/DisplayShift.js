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
            sc.app.createShiftTable(roomHours, roomShifts, createShiftElement, {empty_row: false});
        }, function (error) {
            sc.app.displayError(error);
        });
    }, function (error) {
        sc.app.displayError(error);
    });
});
$("#RoomID").trigger('change');
var addDraggerFunctions = function (elementType, $element) {
    return $element;
};