
var ShiftCaptain = ShiftCaptain || {};
ShiftCaptain.Room = (function(){
    var get,
        getHours;

    get= function (buildingId, callback, fail) {
        var versionId = $("#VersionId").val();
        $.get("/Room/List", { VersionId: versionId, BuildingId: buildingId }, function (data, success) {
            callback(data);
        }, 'json').fail(fail);
    };

    getHours = function (roomId, callback, fail) {
        var versionId = $("#VersionId").val();
        $.get("/Room/ListHours", { RoomId: roomId, VersionId: versionId}, function (data, success) {
            callback(data);
        }, 'json').fail(fail);
    };
    return {
        get: get,
        getHours: getHours
    };
}());
