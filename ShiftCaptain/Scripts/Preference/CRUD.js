
var ShiftCaptain = ShiftCaptain || {};
ShiftCaptain.Preference = (function(){
    var get;

    get = function (preferenceId, callback, fail) {
        $.get("/Preference/List", { PreferenceId: preferenceId}, function (data, success) {
            callback(data);
        }, 'json').fail(fail);
    };
    return {
        get: get
    };
}());
