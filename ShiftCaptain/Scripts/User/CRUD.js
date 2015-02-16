
var ShiftCaptain = ShiftCaptain || {};
ShiftCaptain.User = (function(){
    var get;

    get= function (userId, callback, fail) {
        var versionId = $("#versionId").val();
        $.get("/User/List", { VersionId: versionId, UserId: (userId || 0)}, function (data, success) {
            console.log(data);
            callback(data);
        }, 'json').fail(fail);
    };
    return {
        get: get
    };
}());
