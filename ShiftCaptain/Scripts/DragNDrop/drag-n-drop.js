var dnd = dnd || {};

dnd.drag = (function () {
    var init,
        reinit,
        inProgress,
        clone,
        onMouseDown,
        dragging = false,
        processingDrop = false,
        overridableFunctions,
        counter = 0,
        ctx = {},
        movingCtx = {},
        settings = {
            canDropClass: "open",
            canDragClass: "draggable"
        };//initialization function
    var completeCallback = function (success) {
        if (success) {
            $(movingCtx.target).trigger('movedaround', { target: movingCtx.dropTarget, temp: movingCtx.temp });
        } else {
            $(movingCtx.target).trigger('canceldrag', movingCtx.temp);
        }
        movingCtx = {};
        processingDrop = false;
    };

    init = function (dropSection) {

        ctx.dropSection = dropSection || $(".drop-section");
        $("." + settings.canDragClass).on("mousedown", onMouseDown);
        $(document.body).on("mousemove", function (event) {
            if (dragging && !processingDrop) {
                movingCtx.movingDiv.css('top', event.pageY);
                movingCtx.movingDiv.css('left', event.pageX - parseFloat($("body").css("margin-left")));
            }
        }).on("mouseup", function (event) {
            if (dragging && !processingDrop) {
                console.log("dropping");
                $(".drop-section").removeClass("dragging");
                processingDrop = true;
                dragging = false;
                var dropComplete = true;
                movingCtx.movingDiv.remove();
                var $target = $(movingCtx.target);
                if (event.target == movingCtx.movingDiv[0].children[0]) {
                    console.log("Browser error.  Browser clicked on floating element instead of the element behind it");
                    $target.trigger('canceldrag', movingCtx.temp);
                } else {
                    if (movingCtx.temp && event.target == movingCtx.temp[0]) {
                        //put back in same place, so don't do anything.
                        $target.trigger('canceldrag', movingCtx.temp);
                    } else {
                        var p = ctx.dropSection.position();
                        if (event.clientX >= p.left && event.clientX <= p.left + ctx.dropSection.width()
                            && event.clientY >= p.top && event.clientY <= p.top + ctx.dropSection.height()) {
                            //dragging element is inside the drag area
                            movingCtx.dropTarget = event.target;
                            if ($(event.target).hasClass(settings.canDropClass) && $target.triggerHandler('validatedrop', { target: event.target, callback: completeCallback })) {
                                dropComplete = false;
                            } else {
                                $target.trigger('canceldrag', movingCtx.temp);
                                if ($(event.target).hasClass(settings.canDropClass)) {
                                    console.log("cannot drop here");
                                } else {
                                    console.log("drop is invalid");
                                }
                            }
                        } else {
                            //dragging element is outside the drag area
                            $target.trigger('draggedout', movingCtx.temp);
                        }
                    }
                }
                if (dropComplete) {
                    movingCtx = {};
                    processingDrop = false;
                }
            }
            return false;
        });
    };
    reinit = function ($element) {
        $element.find("." + settings.canDragClass).on("mousedown", onMouseDown);
        if ($element.hasClass(settings.canDragClass)) {
            $element.on("mousedown", onMouseDown);
        }
        return $element;
    };
    onMouseDown = function (event) {
        if (event.which == 1 && !dragging && !processingDrop) {
            var i_counter = ++counter;
            console.log("dragging");
            dragging = true;
            $(".drop-section").addClass("dragging");
            movingCtx.target = event.target;
            movingCtx.clonedTarget = clone(movingCtx.target);
            movingCtx.movingDiv = $("<div class='dnd-moving-div'></div>").append(movingCtx.clonedTarget);
            ctx.dropSection.append(movingCtx.movingDiv);
            movingCtx.movingDiv.css('top', event.pageY);
            movingCtx.movingDiv.css('left', event.pageX - parseFloat($("body").css("margin-left")));
            movingCtx.temp = movingCtx.target;
            $(event.target).on('dragstart-complete', function (event, temp) {
                if (i_counter == counter) {
                    movingCtx.temp = temp;
                }
            }).trigger("dragstart");
        }
        return false;        
    };
    clone = function (element) {
        var clone = $.clone(element, true, true);
        return clone;
    };
    inProgress = function () {
        return dragging || processingDrop;
    };
    var obj = {
        init: init,
        reinit: reinit,
        settings: settings,
        inProgress: inProgress
    };
    return obj;
}());