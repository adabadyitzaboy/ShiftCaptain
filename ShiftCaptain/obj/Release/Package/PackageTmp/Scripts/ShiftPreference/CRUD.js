
var ShiftCaptain = ShiftCaptain || {};
ShiftCaptain.ShiftPreference = (function(){
    var create,
        get,
        update,
        remove,
        validate;

    create = function (preferenceInfo, callback, fail) {
        var versionId = $("#VersionId").val();
        $.post("/ShiftPreference/Create", { VersionId: versionId, PreferenceId: preferenceInfo.PreferenceId, UserId: preferenceInfo.UserId, Day: preferenceInfo.Day, StartTime: preferenceInfo.StartTime, Duration: preferenceInfo.Duration }, function (data, success) {
            callback(data);
        }, 'json').fail(fail);
    };

    update = function (preferenceInfo, callback, fail) {
        $.post("/ShiftPreference/Update", { ShiftPreferenceId: preferenceInfo.ShiftPreferenceId, PreferenceId: preferenceInfo.PreferenceId, Day: preferenceInfo.Day, StartTime: preferenceInfo.StartTime, Duration: preferenceInfo.Duration }, function (data, success) {
            callback(data);
        }, 'json').fail(fail);
    };
    remove = function (shiftPreferenceId, callback, fail) {
        $.post("/ShiftPreference/Delete/" + shiftPreferenceId, function (success) {
            callback(success);
        }).fail(fail);
    };
    get = function (userId, callback, fail) {
        var versionId = $("#VersionId").val();
        $.get("/ShiftPreference/List", { VersionId: versionId, UserId: userId }, function (data, success) {
            callback(data);
        }, 'json').fail(fail);
    };

    validate = function (preferenceInfo, callback, fail) {
        var versionId = $("#VersionId").val();
        $.get("/ShiftPreference/Validate", { VersionId: versionId, ShiftPreferenceId: preferenceInfo.ShiftPreferenceId, PreferenceId: preferenceInfo.PreferenceId, UserId: preferenceInfo.UserId, Day: preferenceInfo.Day, StartTime: preferenceInfo.StartTime, Duration: preferenceInfo.Duration }, function (data, success) {
            callback(data);
        }, 'json').fail(fail);
    };
    return {
        create: create,
        get: get,
        update: update,
        remove: remove,
        validate: validate
    };
}());
