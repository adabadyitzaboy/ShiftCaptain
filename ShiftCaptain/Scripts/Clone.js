var $lbUsers = $("#users");
var $lbUserClones = $("#CloneUser");
var $lbRooms = $("#rooms");
var $lbRoomClones = $("#CloneRoom");
var move = function (from, to) {
    var items = from.val();
    if (items && items.length) {
        for (var idx = 0; idx < items.length; idx++) {
            var item = from.find("[value='" + items[idx] + "']");
            if (item.length) {
                to.append(item);
            }
        }
        from.val("");
        to.val("");
    }
};
$('#btnCloneAllUsers').on('click touchstart', function (e) {
    $lbUsers.find("option").prop("selected", true);
    move($lbUsers, $lbUserClones);
});
$('#btnCloneUser').on('click touchstart', function (e) {
    move($lbUsers, $lbUserClones);
});
$('#btnRemoveUser').on('click touchstart', function (e) {
    move($lbUserClones, $lbUsers);
});
$('#btnRemoveAllUsers').on('click touchstart', function (e) {
    $lbUserClones.find("option").prop("selected", true);
    move($lbUserClones, $lbUsers);
});

$('#btnCloneAllRooms').on('click touchstart', function (e) {
    $lbRooms.find("option").prop("selected", true);
    move($lbRooms, $lbRoomClones);
});
$('#btnCloneRoom').on('click touchstart', function (e) {
    move($lbRooms, $lbRoomClones);
});
$('#btnRemoveRoom').on('click touchstart', function (e) {
    move($lbRoomClones, $lbRooms);
});
$('#btnRemoveAllRooms').on('click touchstart', function (e) {
    $lbRoomClones.find("option").prop("selected", true);
    move($lbRoomClones, $lbRooms);
});

$('#btnCloneSchedule').on('click touchstart', function (e) {
    $lbUserClones.find("option").prop("selected", true);
    $lbRoomClones.find("option").prop("selected", true); 
});