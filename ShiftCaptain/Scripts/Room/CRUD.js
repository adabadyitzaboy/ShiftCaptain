
var ShiftCaptain = ShiftCaptain || {};
ShiftCaptain.Room = (function(){
    var get,
        getHours;

    get= function (buildingId, callback, fail) {
        var versionId = $("#versionId").val();
        $.get("/Room/List", { VersionId: versionId, BuildingId: buildingId }, function (data, success) {
            console.log(data);
            callback(data);
        }, 'json').fail(fail);
    };

    getHours = function (roomId, callback, fail) {
        $.get("/Room/ListHours", { RoomInstanceId: roomId}, function (data, success) {
            console.log(data);
            callback(data);
        }, 'json').fail(fail);
    };
    return {
        get: get,
        getHours: getHours
    };
}());
