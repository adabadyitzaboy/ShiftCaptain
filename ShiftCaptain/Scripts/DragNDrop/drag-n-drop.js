var dnd = dnd || {};

dnd.drag = (function () {
    var init,
        reinit,
        clone,
        onMouseDown,
        addFunction,
        dragging = false,
        processingDrop = false,
        overridableFunctions,
        ctx = {},
        movingCtx = {},
        settings = {
            canDropClass: "open",
            canDragClass: "draggable"
        };//initialization function
    var completeCallback = function (success) {
        if (success) {
            ctx.onMovedAround(movingCtx.target, movingCtx.dropTarget, movingCtx.temp);
        } else {
            ctx.onCancelDrag(movingCtx.target, movingCtx.temp);
        }
        movingCtx = {};
        processingDrop = false;
    };

    init = function (dropSection) {

        ctx.dropSection = dropSection || $(".drop-section");
        $("." + settings.canDragClass).on("mousedown", onMouseDown);
        $(document.body).on("mousemove", function (event) {
            if (dragging && !processingDrop) {
                movingCtx.movingDiv.css('top', event.clientY);
                movingCtx.movingDiv.css('left', event.clientX);
            }
        }).on("mouseup", function (event) {
            if (dragging && !processingDrop) {
                console.log("dropping");
                $(".drop-section").removeClass("dragging");
                processingDrop = true;
                dragging = false;
                var dropComplete = true;
                movingCtx.movingDiv.remove();
                if (event.target == movingCtx.movingDiv[0].children[0]) {
                    console.log("Browser error.  Browser clicked on floating element instead of the element behind it");
                    ctx.onCancelDrag(movingCtx.target, movingCtx.temp);
                } else {
                    if (event.target == movingCtx.temp) {
                        //put back in same place, so don't do anything.
                        ctx.onCancelDrag(movingCtx.target, movingCtx.temp);
                    } else {
                        var p = ctx.dropSection.position();
                        if (event.clientX >= p.left && event.clientX <= p.left + ctx.dropSection.width()
                            && event.clientY >= p.top && event.clientY <= p.top + ctx.dropSection.height()) {
                            //dragging element is inside the drag area
                            movingCtx.dropTarget = event.target;
                            if ($(event.target).hasClass(settings.canDropClass) && ctx.validateDrop(movingCtx.target, event.target, completeCallback)) {
                                dropComplete = false;
                            } else {
                                ctx.onCancelDrag(movingCtx.target, movingCtx.temp);
                                if ($(event.target).hasClass(settings.canDropClass)) {
                                    console.log("cannot drop here");
                                } else {
                                    console.log("drop is invalid");
                                }
                            }
                        } else {
                            //dragging element is outside the drag area
                            ctx.onDraggedOut(movingCtx.target, movingCtx.temp);
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
    reinit = function(element){
        $(element).on("mousedown", onMouseDown);
    };
    onMouseDown = function (event) {
        if (!dragging && !processingDrop) {
            console.log("dragging");
            dragging = true;
            $(".drop-section").addClass("dragging");
            movingCtx.target = event.target;
            movingCtx.clonedTarget = clone(movingCtx.target);
            movingCtx.movingDiv = $("<div class='dnd-moving-div'></div>").append(movingCtx.clonedTarget);
            ctx.dropSection.append(movingCtx.movingDiv);
            movingCtx.movingDiv.css('top', event.clientY);
            movingCtx.movingDiv.css('left', event.clientX);
            movingCtx.temp = ctx.onDragStart(event.target);
        }
        return false;        
    };
    clone = function (element) {
        var clone = $.clone(element, true, true);
        return clone;
    };
    noop = function () { };
    ctx.onDragStart = function (element) {
        console.log("Method not implemented - onDragStart");
    };
    ctx.onCancelDrag = function (dragElement, temp) {
        console.log("Method not implemented - onCancelDrag");
    };
    ctx.onDraggedOut = function (element, temp) {
        console.log("Method not implemented - onDraggedOut");
    };
    ctx.validateDrop = function (dragElement, dropElement) {
        console.log("Method not implemented - validateDrop");
    };
    ctx.onMovedAround = function (dragElement, dropElement, temp) {
        console.log("Method not implemented - onMoveAround");
    };
    addFunction = function (name, fn) {
        if (overridableFunctions[name]) {
            ctx[name] = fn;
        } else {
            throw Error("Cannot override function '" + name + "'");
        }
    };
    overridableFunctions = {
        "onCancelDrag": 1,
        "onDragStart": 1,
        "onMovedAround": 1,
        "onDraggedOut": 1,
        "validateDrop": 1
    };
    var obj = {
        init: init,
        reinit: reinit,
        addFunction: addFunction,
        settings: settings
    };
    return obj;
}());