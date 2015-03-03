var tips = [];
tips.nextId = 0;
var dragger = null;
var origReplaceWith = $.fn.replaceWith;
$.ajaxSetup({ global: false });
$.fn.extend({
    tooltip: function (selector, data) {
        if (!dragger && dnd) {
            dragger = dnd && dnd.drag;
        }
        var showHide = function (show, e) {
            var tip;
            var tipId = this.attr("tip");
            if (tipId) {
                tip = $("#" + tipId);
            }
            if (dragger && dragger.inProgress()) {
                if (tip) {
                    tip.hide();
                }
                return;
            }
            if (!tipId) {
                tipId = 'tip-id-' + ++tips.nextId;
                this.attr('tip', tipId);
                tip = $("<tip>")
                .attr({ id: tipId })
                .addClass("tooltip")
                .addClass(selector)
                .on('mouseover', function () {
                    tip.show();
                }).on('mouseout', function () {
                    tip.hide();
                })
                .html($('tip-master[type="' + selector + '"]').html())
                .databind(data || {})
                .appendTo(document.body);
            }
            if (show) {
                tip.css("top", this.offset().top + parseInt(this.css('padding-top')) + this.height());
                tip.css("left", this.position().left);
                tip.show();
            } else {
                tip.hide();
            }
        }.bind(this);
            
        this.on('mouseover', function (e) {
            showHide(true, e);
        }).on('mouseout', function (e) {
            showHide(false, e);
        }).on('dragstart', function (e) {
            showHide(false, e);
        });
        return this;
    },
    databind: function (data) {
        this.find("[data-bind]").each(function (index) {
            var $this = $(this);
            var value = data[$this.attr('data-bind')] || "";
            if ($this.attr("data-format")) {
                var format = $this.attr("data-format");
                if (typeof (value) === "object") {
                    while (true) {
                        var matches = format.match(/\[([^\]]+)\]({([0-9]+)})*/);
                        if (!matches) {
                            break;
                        }
                        var formattedValue = value[matches[1]].toString();
                        if (matches.length == 4 && matches[3] != null) {
                            var len = parseInt(matches[3])
                            if (formattedValue.length < len) {
                                while (formattedValue.length < len) {
                                    formattedValue = "0" + formattedValue;
                                }
                            }
                        }
                        format = format.replace(matches[0], formattedValue);
                    }
                } else {
                    while (true) {
                        var matches = format.match(/{([^}]+)}/);
                        if (!matches) {
                            break;
                        }
                        format = format.replace(matches[0], value);
                    }
                }
                $this.html(format);
            }else{
                $this.html(value);
            }
            if ($this.attr('data-label')) {
                var label = $("<label>").html($this.attr('data-label'));
                $this.replaceWith(label).appendTo(label);
            }            
        });
        this.find("[data-bind-attr]").each(function (index) {
            for (var idx = 0; idx < this.attributes.length; idx++) {
                if (/{([^}]+)}/.test(this.attributes[idx].value)) {
                    while (true) {
                        var matches = this.attributes[idx].value.match(/{([^}]+)}/);
                        if (!matches) {
                            break;
                        }
                        this.attributes[idx].value = this.attributes[idx].value.replace(matches[0], data[matches[1]]);
                    }
                }
            }
        });
        return this;
    },
    replaceWith: function (value, keepData) {
        if ($.isFunction(value) || keepData != true) {
            return origReplaceWith.call(this, value);
        }
        this.after(value);//if we just remove the element then the event system will not work on the element anymore.
        this.remove(null, true);
        return this;
    }
});


var height = null;
var ShiftCaptain = ShiftCaptain || {};
ShiftCaptain.app = {
    resizeHeader: function () {
        var newHeight = $("body > header > .content-wrapper").height();
        if (height != newHeight) {
            height = newHeight;
            $("body > header").height(newHeight);
        }
    }
};
$(document).ready(function () {
    ShiftCaptain.app.resizeHeader();
});