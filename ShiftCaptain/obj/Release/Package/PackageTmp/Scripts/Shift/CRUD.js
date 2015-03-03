
var ShiftCaptain = ShiftCaptain || {};
ShiftCaptain.Shift = (function(){
    var validate,
        create,
        get,
        getPreferences,
        update,
        remove;

    validate = function (shiftInfo, callback, fail) {
        var versionId = $("#VersionId").val();
        //$.get("/Shift/Validate", { VersionId: versionId, ShiftId: shiftInfo.ShiftId, RoomId: shiftInfo.roomId, UserId: shiftInfo.userId, Day: shiftInfo.day, StartTime: shiftInfo.startTime, Duration: shiftInfo.duration }, function (data, success) {
        $.ajax({
            url: "/Shift/Validate",
            data: { VersionId: versionId, ShiftId: shiftInfo.ShiftId, RoomId: shiftInfo.roomId, UserId: shiftInfo.userId, Day: shiftInfo.day, StartTime: shiftInfo.startTime, Duration: shiftInfo.duration },
            success: function (data, success) {
                console.log(data);
                callback(data);
            },
            dataType: 'json',
            fail: fail
        }).fail(fail);
        //    console.log(data);
        //    callback(data);
        //}, 'json').fail(fail);
    };
    create = function (shiftInfo, callback, fail) {
        var versionId = $("#VersionId").val();
        $.post("/Shift/Create", { VersionId: versionId, RoomId: shiftInfo.roomId, UserId: shiftInfo.userId, Day: shiftInfo.day, StartTime: shiftInfo.startTime, Duration: shiftInfo.duration }, function (data, success) {
            console.log(data);
            callback(data);
        }, 'json').fail(fail);
    };

    update = function (shiftInfo, callback, fail) {
        var versionId = $("#VersionId").val();
        $.post("/Shift/Update", { ShiftId: shiftInfo.ShiftId, RoomId: shiftInfo.roomId, Day: shiftInfo.day, StartTime: shiftInfo.startTime, Duration: shiftInfo.duration }, function (data, success) {
            console.log(data);
            callback(data);
        }, 'json').fail(fail);
    };
    remove = function (shiftId, callback, fail) {
        $.post("/Shift/Delete/" + shiftId, function (success) {
            callback(success);
        }).fail(fail);
    };
    get = function (roomId, callback, fail) {
        var versionId = $("#VersionId").val();
        $.get("/Shift/List", { VersionId: versionId, RoomId: roomId }, function (data, success) {
            console.log(data);
            callback(data);
        }, 'json').fail(fail);
    };
    getPreferences = function (userId, callback, fail) {
        var versionId = $("#VersionId").val();
        $.get("/Shift/ListPreferences", { VersionId: versionId, UserId: userId}, function (data, success) {
            console.log(data);
            callback(data);
        }, 'json').fail(fail);
    };
    return {
        validate: validate,
        create: create,
        get: get,
        getPreferences: getPreferences,
        update: update,
        remove: remove
    };
}());
