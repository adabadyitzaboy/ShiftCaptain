var order = function (rows, cellIndex, asc) {
    try {
        var rtn = _order(rows, cellIndex, 0, rows.length, asc);
        //console.log('ordered-'+ (asc?"asc":"desc"), dumb(rtn, cellIndex));
        return rtn;
    } catch (e) {
        return rows;
    }
};
var compare = function (row1, row2, cellIndex, asc) {
    
    var text1 = row1.children[cellIndex].innerText.toLowerCase();
    var text2 = row2.children[cellIndex].innerText.toLowerCase();
    if (text1 || text2) {
        if (!text1 && text2) {
            return !asc;
        } else if (!text2 && text1) {
            return asc;
        }
        if (asc) {
            return text1 <= text2;
        } else {
            return text2 <= text1;
        }
            
    } else {
        if (row1.children[cellIndex].children[0].type == "checkbox") {
            if (row2.children[cellIndex].children[0].checked && !row1.children[cellIndex].children[0].checked) {
                return !asc;
            } else {
                return asc;
            }
        }
    }

    return asc;
};
var _order = function (rows, cellIndex, start, end, asc) {
    if (end <= start + 2) {
        if (end == start + 2) {
            if (compare(rows[start], rows[start + 1], cellIndex, asc)) {
                return [rows[start], rows[start + 1]];
            } else {
                return [rows[start + 1], rows[start]];
            }
            
        } else {
            return [rows[start]];
        }
    } else {
        var half = (end - start)/2 | 0;
        var first = _order(rows, cellIndex, start, start + half, asc);
        var second = _order(rows, cellIndex, start + half, end, asc);
        var rtn = [];
        for (var idx = 0; idx < first.length; ) {
            for (var idx2 = 0; idx2 < second.length; ) {
                if (compare(first[idx], second[idx2], cellIndex, asc)) {
                    rtn.push(first[idx++]);
                } else {
                    rtn.push(second[idx2++]);
                }
                if (idx >= first.length) {
                    //push the rest of second
                    for (var idx3 = idx2; idx3 < second.length; idx3++) {
                        rtn.push(second[idx3]);
                    }
                    break;
                }
            }
            if (idx < first.length) {
                //push the rest of second
                for (var idx4 = idx; idx4 < first.length; idx4++) {
                    rtn.push(first[idx4]);
                }
                break;
            }
        }
        return rtn;
    }
};
var dumb = function (arr, cellIndex) {
    var rtn = [];
    for (var idx = 0; idx < arr.length; idx++) {
        rtn.push(arr[idx].children[cellIndex].innerText);
    }
    return rtn;
};
$(document).ready(function () {
   $(".sortable th").on("touchstart click", function (event) {
        var $cell = $(event.target);
        var index = $cell.index();
        var table = $cell.closest("table");
        var rows = table.find("tr:not(:first-child)");
        if (rows.length > 1 && index != rows[0].children.length - 1) {
            var header = $(table.find("tr:first-child"));
            var asc = true;
            if ($cell.hasClass("asc")) {
                asc = false;
            }
            header.find(".asc").removeClass("asc");
            header.find(".desc").removeClass("desc");
            $cell.addClass(asc ? "asc" : "desc");
            var ordered = order(rows, index, asc);
            rows.remove();
            if (ordered.length > 0) {
                table.append(ordered);
            }
        }
    });
});