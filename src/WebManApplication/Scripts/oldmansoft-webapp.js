/*
* v0.14.60
* https://github.com/Oldmansoft/webapp
* Copyright 2016 Oldmansoft, Inc; http://www.apache.org/licenses/LICENSE-2.0
*/
if (!window.oldmansoft) window.oldmansoft = {};
window.oldmansoft.webapp = new (function () {
    var $this = this,
    _setting = {
        timeover: 180000,
        loading_show_time: 1000,
        loading_hide_time: 200
    },
    _isIeCore = "ActiveXObject" in window,
    _text = {
        ok: "Ok",
        yes: "Yes",
        no: "No",
        loading: "Loading"
    },
    _mainView = null,
    _openView = null,
    _modalView = null,
    _activeView = null,
    _fnOnUnauthorized = function () {
        return false;
    },
    _currentViewEvent = null,
    _isDealLinkEmptyTarget = true,
    _isReplacePCScrollBar = true,
    _WindowScrollBar = null,
    _scrollbar = [],
    _canTouch = null,
    _globalViewEvent = null,
    _dealHrefTarget,
    _messageBox,
    _windowBox,
    _modalBox;

    function getAbsolutePath(path, basePath, fullLink) {
        var indexOfAmpersand,
            indexOfQuestion,
            pathnames;
        if (path == "") path = fullLink;
        indexOfAmpersand = path.indexOf("&");
        if (indexOfAmpersand > -1) {
            indexOfQuestion = path.indexOf("?");
            if (indexOfQuestion == -1 || indexOfAmpersand < indexOfQuestion) {
                path = path.substr(0, indexOfAmpersand);
            }
        }
        if (path.substr(0, 1) == "/") {
            return path;
        }
        if (!basePath) {
            basePath = document.location.pathname;
        } else {
            indexOfQuestion = basePath.indexOf("?");
            if (indexOfQuestion > -1) {
                basePath = basePath.substr(0, indexOfQuestion);
            }
            indexOfAmpersand = basePath.indexOf("&");
            if (indexOfAmpersand > -1) {
                basePath = basePath.substr(0, indexOfAmpersand);
            }
        }
        pathnames = basePath.split("/");
        pathnames.pop();
        return pathnames.join("/") + "/" + path;
    }

    function getPathHasAbsolutePathFromArray(array, index, fullLink) {
        for (var i = index; i > -1; i--) {
            if (array[i] == "" && fullLink.substr(0, 1) == "/") return fullLink;
            if (array[i].substr(0, 1) == "/") return array[i];
        }
        return null;
    }

    function isHtmlDocument(data) {
        var spaceIndex,
            c;

        if (!data) return false;

        for (spaceIndex = 0; spaceIndex < data.length; spaceIndex++) {
            c = data.substr(spaceIndex, 1);
            if (c == " ") continue;
            if (c == "\r") continue;
            if (c == "\n") continue;
            if (c == "\t") continue;
            break;
        }
        if (data.substr(spaceIndex, 15) == "<!DOCTYPE html>") return true;
        if (data.substr(spaceIndex, 5) == "<html") return true;
        return false;
    }

    this.scriptLoader = new function () {
        var hasScripts = [];

        function loadScriptExecute(args, index, deferred) {
            if (args.length == index + 1) {
                deferred.resolve();
            } else {
                loadScript(args, index + 1, deferred);
            }
        }

        function loadScript(args, index, deferred) {
            if (hasScripts[args[index]]) {
                loadScriptExecute(args, index, deferred);
                return;
            }
            $.getScript(args[index], function () {
                hasScripts[args[index]] = true;
                loadScriptExecute(args, index, deferred);
            });
        }

        this.load = function () {
            var result = $.Deferred();
            if (arguments.length == 0) result.resolve();
            else loadScript(arguments, 0, result);
            return result;
        }
    }

    this.bodyManagement = new function () {
        var count = 0;

        this.expand = function () {
            if (count == 0) {
                $("body").addClass("layout-expanded");
                if (_WindowScrollBar) _WindowScrollBar.hide();
            }
            count++;
        }

        this.shrink = function () {
            count--;
            if (count == 0) {
                $("body").removeClass("layout-expanded");
                if (_WindowScrollBar) _WindowScrollBar.show();
            }
            if (count < 0) {
                throw "shrink error";
            }
        }
    }

    this.scrollbar = function (target) {
        if (!target) return;
        var targetDom,
            targetHelper,
            container,
            track,
            trackWidth,
            arrow,
            arrowHeight,
            html,
            downTargetTop,
            downMouseY,
            isShow = true;

        function scrollTop(element, value) {
            if (element.selector == "body" && _isIeCore) {
                if (value != undefined)
                    $(document).scrollTop(value);
                else
                    return $(document).scrollTop();
            } else {
                if (value != undefined)
                    element.scrollTop(value);
                else
                    return element.scrollTop();
            }
        }

        function bodyTarget(dom) {
            var contentHeight,
                viewHeight
            isSetTrackPosition = false;
            this.contentHeight = function () {
                return contentHeight;
            }
            this.viewHeight = function () {
                return viewHeight;
            }
            this.setHeight = function () {
                contentHeight = dom.scrollHeight;
                viewHeight = window.innerHeight;
            }
            this.setTrackPosition = function (track) {
                if (isSetTrackPosition) return true;
                track.css("right", 0);
                track.css("top", 0);
                isSetTrackPosition = true;
                return true;
            }
            this.bindMouseWheel = function () {
                $(window).on("mousewheel", targetMouseWheel);
            }
            this.unbindMouseWheel = function () {
                $(window).off("mousewheel", targetMouseWheel);
            }
        }

        function otherTarget($t, dom) {
            var contentHeight,
                viewHeight;

            this.contentHeight = function () {
                return contentHeight;
            }
            this.viewHeight = function () {
                return viewHeight;
            }
            this.setHeight = function () {
                contentHeight = dom.scrollHeight;
                viewHeight = $t.innerHeight();
            }
            this.setTrackPosition = function () {
                return false;
            }
            this.bindMouseWheel = function () {
                $t.on("mousewheel", targetMouseWheel);
            }
            this.unbindMouseWheel = function () {
                $t.off("mousewheel", targetMouseWheel);
            }
        }

        function setArrowPosition() {
            var height = (targetHelper.viewHeight() - arrowHeight) * scrollTop(target) / (targetHelper.contentHeight() - targetHelper.viewHeight());
            arrow.css("top", height);
        }

        function targetMouseWheel(e) {
            if (targetHelper.contentHeight() <= targetHelper.viewHeight()) return true;
            var delta = e.originalEvent.wheelDelta,
                targetScrollTop = scrollTop(target);
            if (delta < 0) {
                if (targetScrollTop >= (targetHelper.contentHeight() - targetHelper.viewHeight())) {
                    return true;
                }
            } else {
                if (targetScrollTop == 0) {
                    return true;
                }
            }
            scrollTop(target, targetScrollTop - delta);
            setArrowPosition();
            return false;
        }

        function htmlSelectStart() {
            return false;
        }

        function arrowMouseDown(e) {
            downMouseY = e.clientY;
            downTargetTop = scrollTop(target);
            html.on("selectstart", htmlSelectStart);
            html.on("mousemove", htmlMouseMove);
            html.on("mouseup", htmlMouseUp);
            track.addClass("focus");
        }

        function htmlMouseUp() {
            html.off("selectstart", htmlSelectStart);
            html.off("mousemove", htmlMouseMove);
            html.off("mouseup", htmlMouseUp);
            track.removeClass("focus");
        }

        function htmlMouseMove(e) {
            var per = (targetHelper.contentHeight() - targetHelper.viewHeight()) / (targetHelper.viewHeight() - arrowHeight)
            scrollTop(target, downTargetTop - (downMouseY - e.clientY) * per);
            setArrowPosition();
        }

        function reset() {
            targetHelper.setHeight();
            if (targetHelper.viewHeight() == 0 || targetHelper.contentHeight() <= targetHelper.viewHeight()) {
                track.hide();
                return;
            } else {
                track.show();
            }
            track.height(targetHelper.viewHeight());
            if (!targetHelper.setTrackPosition(track)) {
                track.css("left", target.innerWidth() - parseInt(target.css("padding-left")) - trackWidth);
                track.css("top", -parseInt(target.css("padding-top")));
            }
            arrowHeight = track.height() * targetHelper.viewHeight() / targetHelper.contentHeight();
            if (arrowHeight < 20) arrowHeight = 20;
            arrow.height(arrowHeight);
            setArrowPosition();
        }

        target = $(target);
        targetDom = target.get(0);
        targetHelper = targetDom.tagName == "BODY" ? new bodyTarget(targetDom) : new otherTarget(target, targetDom);
        html = $("html");
        target.css("overflow", "hidden");
        container = $("<div></div>").addClass("scrollbar-container");
        track = $("<div></div>").addClass("scrollbar-track");
        arrow = $("<div></div>").addClass("scrollbar-arrow");
        container.append(track);
        track.append(arrow);
        target.prepend(container);
        trackWidth = track.width();
        reset();
        _scrollbar.push(this);
        track.mousedown(arrowMouseDown);
        $(window).on("resize", reset);
        targetHelper.bindMouseWheel();

        this.show = function () {
            if (isShow) return;
            container.show();
            targetHelper.bindMouseWheel();
            isShow = true;
        }
        this.hide = function () {
            if (!isShow) return;
            targetHelper.unbindMouseWheel();
            container.hide();
            isShow = false;
        }
        this.reset = function () {
            reset();
        }
    }

    this.resetScrollbar = function () {
        for (var i = 0 ; i < _scrollbar.length; i++) {
            _scrollbar[i].reset();
        }
    }

    this.resetWindowScrollbar = function () {
        if (_WindowScrollBar) {
            _WindowScrollBar.reset();
            return;
        }
        if (_canTouch == null) {
            _canTouch = "ontouchmove" in document;
        }
        if (!_canTouch && _isReplacePCScrollBar) {
            _WindowScrollBar = new $this.scrollbar("body");
            setInterval(_WindowScrollBar.reset, 500);
        }
    }

    function viewEvent() {
        this.load = function () { };
        this.unload = function () { };
        this.active = function () { };
        this.inactive = function () { };
    }

    function viewEventParameter(node, name, level) {
        this.node = node;
        this.name = name;
        this.level = level;
    }

    function linkManagement() {
        var context = [];

        function item(name, link, level) {
            var eventParameter,
                visible = true,
                scrollTop = 0,
                scrollLeft = 0,
                localViewEvent,
                option = { closed: null };

            this.link = link;
            this.node = $("<div></div>").addClass(name + "-view").data("link", link);
            this.valid = false;
            eventParameter = new viewEventParameter(this.node, name, level);

            this.hide = function () {
                if (!this.valid || !visible) return;
                var win = $(window);
                scrollTop = win.scrollTop();
                scrollLeft = win.scrollLeft();
                _globalViewEvent.inactive(eventParameter, localViewEvent.inactive(eventParameter));
                this.node.hide();
                visible = false;
            }

            this.callLoadAndActive = function () {
                _globalViewEvent.load(eventParameter, localViewEvent.load(eventParameter));
                _globalViewEvent.active(eventParameter, localViewEvent.active(eventParameter));
            }

            this.callInactiveAndUnload = function () {
                _globalViewEvent.inactive(eventParameter, localViewEvent.inactive(eventParameter));
                _globalViewEvent.unload(eventParameter, localViewEvent.unload(eventParameter));
            }

            this.remove = function () {
                this.node.remove();
                if (!this.valid) return;
                this.callInactiveAndUnload();
                this.node = null;
                this.valid = false;
                localViewEvent = null;
            }

            this.show = function () {
                if (!this.valid || visible) return;
                this.node.show();
                _globalViewEvent.active(eventParameter, localViewEvent.active(eventParameter));
                $(window).scrollLeft(scrollLeft);
                $(window).scrollTop(scrollTop);
                visible = true;
            }

            this.activeEvent = function () {
                if (!localViewEvent) return;
                _globalViewEvent.active(eventParameter, localViewEvent.active(eventParameter));
            }

            this.inactiveEvent = function () {
                if (!localViewEvent) return;
                _globalViewEvent.inactive(eventParameter, localViewEvent.inactive(eventParameter));
            }

            this.setContext = function () {
                _currentViewEvent = new viewEvent();

                if (arguments.length == 1) {
                    this.node.html(arguments[0]);
                } else {
                    this.node.empty();
                    for (var i = 0; i < arguments.length; i++) {
                        this.node.append(arguments[i]);
                    }
                }

                localViewEvent = _currentViewEvent;
                this.valid = true;
            }

            this.getOption = function () {
                return option;
            }
        }

        this.push = function (name, link) {
            context.push(new item(name, link, this.count() + 1));
        }

        this.pop = function () {
            return context.pop();
        }

        this.last = function () {
            return context[context.length - 1];
        }

        this.like = function (links) {
            if (context.length == 0) return false;
            for (var i = 0; i < context.length; i++) {
                if (links.length == i) break;
                if (context[i].link != links[i]) return false;
            }
            return true;
        }

        this.count = function () {
            return context.length;
        }

        this.get = function (index) {
            return context[index];
        }

        this.replace = function (index, name, link) {
            var newItem = new item(name, link, index + 1);
            context[index].node.after(newItem.node);
            context[index].remove();
            context[index] = newItem;
        }

        this.getBackLink = function () {
            var link = this.getLinks().slice();
            link.splice(0, 0, "");
            link.splice(link.length - 1, 1);
            return link.join("#");
        }

        this.getLink = function () {
            var link = this.getLinks().slice();
            link.splice(0, 0, "");
            return link.join("#");
        }

        this.getLinks = function () {
            var result = [],
                i;
            for (i = 0; i < context.length; i++) {
                result.push(context[i].link);
            }
            return result;
        }
    }

    this.box = function (className, isMiddle) {
        var isInit = false,
	        element,
            core,
	        store = [],
	        current = null;

        function close(event, fn) {
            if (event && event.target != event.currentTarget) return;

            element.stop(true);
            element.fadeOut(store.length > 0 ? 0 : 200, function () {
                $this.bodyManagement.shrink();
                if (current == null) {
                    if (fn) fn();
                    return;
                }

                if (current.close) current.close();
                current.node.remove();

                if (store.length > 0) {
                    current = store.pop();
                    core.append(current.node);
                    if (fn) fn();
                    element.stop(true, true);
                    element.fadeIn(0);
                    return;
                }

                current = null;
                if (fn) fn();
            });
        }
        function initElement() {
            if (isInit) return;
            isInit = true;
            element = $("<div></div>").addClass(className).addClass("box-background");
            if (isMiddle) {
                core = $("<div></div>").addClass("layout-horizontal")
                element.append(core);
                element.append($("<div></div>").addClass("layout-vertical"));
            } else {
                core = element;
            }
            element.prependTo($("body"));
        }

        this.open = function (node, fnClose) {
            initElement();
            if (current) {
                if (current.node.data("type") == "message") {
                    throw "not allow show again after message show.";
                }
                store.push({ node: current.node.detach(), close: current.close });
            }
            $this.bodyManagement.expand();
            current = { node: node, close: fnClose };
            core.append(node);
            element.stop(true, true);
            element.fadeIn(200);
        }

        this.close = function (event, fn) {
            close(event, fn);
        }

        this.clear = function () {
            if (!current) return;
            if (current.close) current.close();
            current.node.remove();
            while (store.length > 0) {
                current = store.pop();
                core.append(current.node);
                if (current.close) current.close();
                current.node.remove();
            }
            current = null;
            element.hide();
        }
    }

    _activeView = new function () {
        var current,
            stack = [];
        this.push = function (view) {
            stack.push(current);
            current = view;
        }
        this.get = function () {
            return current;
        }
        this.pop = function () {
            if (stack.length == 1) {
                throw "error call";
            }
            current = stack.pop();
            return current;
        }
    }
    _messageBox = new $this.box("dialog-background", true);
    _windowBox = new $this.box("window-background");
    _modalBox = new function () {
        var isInit = false,
	        element,
	        store = [],
	        current = null;

        function close(event, fn) {
            if (event && event.target != event.currentTarget) return;

            current.node.stop(true);
            current.node.fadeOut(store.length > 0 ? 0 : 200, function () {
                $this.bodyManagement.shrink();
                if (current == null) {
                    if (fn) fn();
                    return;
                }

                if (current.close) current.close();
                current.node.remove();

                if (store.length > 0) {
                    current = store.pop();
                    if (fn) fn();
                    current.node.stop(true, true);
                    current.node.fadeIn(0);
                    return;
                }

                current = null;
                if (fn) fn();
            });
        }
        function initElement() {
            if (isInit) return;
            isInit = true;

            element = $("<div></div>").addClass("modal-areas");
            element.prependTo($("body"));
        }
        function createNode(node) {
            var container,
                main;

            container = $("<div></div>").addClass("modal-background").addClass("box-background");
            main = $("<div></div>").addClass("layout-horizontal")
            main.append(node);
            container.append(main);
            container.append($("<div></div>").addClass("layout-vertical"));
            container.appendTo(element);
            container.on("click", function (e) {
                if (e.currentTarget != e.target) return;
                _modalView.close();
            });
            return container;
        }

        this.open = function (node, fnClose) {
            initElement();
            if (current) {
                store.push({ node: current.node, close: current.close });
            }
            $this.bodyManagement.expand();

            current = { node: createNode(node), close: fnClose };
            current.node.stop(true, true);
            current.node.fadeIn(200);
        }

        this.close = function (event, fn) {
            close(event, fn);
        }

        this.clear = function () {
            if (!current) return;
            if (current.close) current.close();
            current.node.remove();
            while (store.length > 0) {
                current = store.pop();
                if (current.close) current.close();
                current.node.remove();
            }
            current = null;
        }
    }

    this.dialog = new function () {
        function elementBuilder() {
            var element = $("<div></div>").addClass("dialog-box").addClass("box-panel");

            this.setHead = function (title) {
                var header = $("<div></div>").addClass("dialog-header");
                header.append($("<h4></h4>").text(title));
                element.append(header);
            }
            this.setBody = function (text) {
                var body = $("<div></div>").addClass("dialog-body").text(text);;
                element.append(body);
            }
            this.setFooter = function () {
                var footer = $("<div></div>").addClass("dialog-footer");

                function option(node) {
                    this.set = function (text) {
                        var closeCallback,
                            button = $("<button></button>").text(text);

                        button.click(function (event) {
                            _messageBox.close(event, function () {
                                if (closeCallback) closeCallback();
                            });
                        });
                        node.append(button);
                        return new function () {
                            this.setCallback = function (fn) {
                                closeCallback = fn;
                            }
                        }
                    }
                }
                element.append(footer);
                return new option(footer);
            }
            this.get = function (type) {
                element.data("type", type);
                return element;
            }
            this.getElement = function () {
                return element;
            }
        }
        this.alert = function (content, title, fn) {
            var okButton,
                builder = new elementBuilder();

            function option(button) {
                this.onConfirm = function (fn) {
                    console.warn("onConfirm is obsolete. commend use ok");
                    button.setCallback(fn);
                    return this;
                }
                this.ok = function (fn) {
                    button.setCallback(fn);
                    return this;
                }
            }
            if (typeof title == "function") {
                fn = title;
                title = null;
            }
            if (title) {
                builder.setHead(title);
            }
            builder.setBody(content);
            okButton = builder.setFooter().set(_text.ok);
            if (fn) {
                console.warn("fn is obsolete. commend use ok");
                okButton.setCallback(fn);
            }
            _messageBox.open(builder.get("alert"));
            return new option(okButton);
        }
        this.confirm = function (content, title, fnYes, fnNo) {
            var yesButton,
                noButton,
                footer,
                builder = new elementBuilder();

            function option(yesButton, noButton) {
                this.onConfirm = function (fn) {
                    console.warn("onConfirm is obsolete. commend use yes");
                    yesButton.setCallback(fn);
                    return this;
                }
                this.yes = function (fn) {
                    yesButton.setCallback(fn);
                    return this;
                }
                this.onCancel = function (fn) {
                    console.warn("onCancel is obsolete. commend use no");
                    noButton.setCallback(fn);
                    return this;
                }
                this.no = function (fn) {
                    noButton.setCallback(fn);
                    return this;
                }
            }
            if (typeof title == "function") {
                fnNo = fnYes;
                fnYes = title;
                title = null;
            }
            if (title) {
                builder.setHead(title);
            }
            builder.setBody(content);
            footer = builder.setFooter();
            yesButton = footer.set(_text.yes, fnYes);
            if (fnYes) {
                console.warn("fnYes is obsolete.");
                yesButton.setCallback(fnYes);
            }
            noButton = footer.set(_text.no, fnNo);
            if (fnNo) {
                console.warn("fnNo is obsolete.");
                noButton.setCallback(fnNo);
            }
            _messageBox.open(builder.get("confirm"));
            return new option(yesButton, noButton);
        }
        this.message = function (content) {
            var builder = new elementBuilder();
            builder.setBody(content);
            _messageBox.open(builder.get("message"));
            return new function () {
                this.close = function (fn) {
                    _messageBox.close(null, fn);
                }
                this.change = function (text) {
                    builder.getElement().find(".dialog-body").text(text);
                }
            }
        }
    }

    this.loadingTip = new function () {
        var element;

        function initElement() {
            if (element != null) return;
            element = $("<div></div>").addClass("loading-background").addClass("box-background");
            var dialog = $("<div></div>").addClass("loading-box").addClass("box-panel"),
                text = $("<span></span>").text(_text.loading);

            dialog.append(text);
            element.append($("<div></div>").addClass("layout-horizontal").append(dialog)).append($("<div></div>").addClass("layout-vertical"));
            element.prependTo($("body"));
        }
        this.show = function () {
            initElement();
            $this.bodyManagement.expand();
            element.stop(true, true);
            element.fadeIn(_setting.loading_show_time);
            return new function () {
                this.hide = function () {
                    $this.loadingTip.hide();
                }
            }
        }
        this.hide = function () {
            initElement();
            element.stop(true);
            element.fadeOut(_setting.loading_hide_time, function () {
                $this.bodyManagement.shrink();
            });
        }
    }

    this.linker = new function () {
        var initHashChange = false,
            lastHash = null,
            changeCallback = null,
            changeCompleted = null;

        function fixHref(href) {
            if (!href) return href;
            if (href.substr(0, 1) == "#") return href.substr(1);
            return href;
        }
        function callLeave() {
            changeCallback(lastHash);
        }
        function hashChange() {
            var href = fixHref(window.location.hash);
            if (lastHash == href) {
                return;
            }
            lastHash = href;
            callLeave();
        }

        this.setChangeCompleted = function (fn) {
            changeCompleted = fn;
        }

        this.callChangeCompleted = function (isNewContent) {
            if (!changeCompleted) return;
            changeCompleted(isNewContent);
            changeCompleted = null;
        }

        this.modify = function (href) {
            window.location.hash = href;
            lastHash = fixHref(href);
        }
        this.hash = function (href) {
            if (href == undefined) return window.location.hash;

            window.location.hash = href;
            if (href == lastHash) {
                callLeave();
            }
            return href;
        }
        this.addHash = function (href) {
            href = fixHref(href);
            if (window.location.hash == "") {
                window.location.hash = "##" + href;
            } else {
                window.location.hash += "#" + href;
            }
        }
        this.sameHash = function (href) {
            href = fixHref(href);
            var source = window.location.hash.split("#");
            if (source.length == 1) {
                source = ["", ""];
            }
            source.pop();
            source.push(href);
            window.location.hash = source.join("#");
            if (href == lastHash) {
                callLeave();
            }
        }
        this.refresh = function () {
            callLeave();
        }
        this._init = function (fnChangeCall) {
            if (initHashChange) return;
            initHashChange = true;

            changeCallback = fnChangeCall;
            lastHash = fixHref(window.location.hash);
            if ("onhashchange" in window) {
                window.onhashchange = hashChange;
            } else {
                window.setInterval(function () {
                    if (lastHash != fixHref(window.location.hash)) {
                        hashChange();
                    }
                }, 100);
            }
            callLeave();
        }
    }

    function modalArea() {
        var links = new linkManagement(),
            loadOption = { closed: null };

        function setView(link, first, second) {
            var last;

            if (links.count() == 0) {
                _activeView.push(_modalView);
            }

            links.push("modal", link);
            last = links.last();
            last.node.addClass("box-panel");
            if (second == undefined) {
                last.setContext(first);
            } else {
                last.setContext(first, second);
            }
            _modalBox.open(last.node, function () {
                last.remove();
            });
            last.callLoadAndActive();
            last.getOption().closed = loadOption.closed;
        }

        this.load = function (link, data, type, loadCompleted) {
            var loading = $this.loadingTip.show(),
                loadPath;

            loadPath = getAbsolutePath(link, getPathHasAbsolutePathFromArray(links.getLinks(), links.count() - 2, _mainView.getDefaultLink()), _mainView.getDefaultLink());
            $.ajax({
                mimeType: 'text/html; charset=utf-8',
                url: loadPath,
                data: data,
                type: type,
                timeout: _setting.timeover
            }).done(function (data, textStatus, jqXHR) {
                loading.hide();
                var json = jqXHR.getResponseHeader("X-Responded-JSON"),
	                responded;

                if (json) {
                    responded = JSON.parse(json);
                    if (responded.status == 401) {
                        if (!_fnOnUnauthorized(responded.headers.location)) {
                            if (responded.headers && responded.headers.location) {
                                document.location = responded.headers.location;
                                return;
                            }
                        }
                    }
                }

                if (isHtmlDocument(data)) {
                    alert("You try to load wrong content: " + loadPath);
                    return;
                }

                setView(link, data);
                if (loadCompleted) loadCompleted(true);
            }).fail(function (jqXHR, textStatus, errorThrown) {
                loading.hide();
                if (jqXHR.status == 401) {
                    _fnOnUnauthorized(link);
                }
                var response = $(jqXHR.responseText),
                    title = $("<h4></h4>").text(errorThrown),
                    content = $("<pre></pre>");

                if (response[11] != null && response[11].nodeType == 8) {
                    content.text(response[11].data);
                } else {
                    content.text(response.eq(1).text());
                }

                setView(link, title, content);
                if (loadCompleted) loadCompleted(true);
            });
            loadOption.closed = null;
            return loadOption;
        }

        this.close = function (parameter, closeCompleted) {
            var lastClosed = links.pop().getOption().closed;
            _modalBox.close(null, function () {
                var current = links.last();
                if (!current) {
                    _activeView.pop();
                }
                if (lastClosed) lastClosed(parameter);
                if (closeCompleted) closeCompleted(false);
            });
        }

        this.replace = function (link, data) {
            var last = links.last();
            last.callInactiveAndUnload();
            last.link = link;
            last.setContext(data);
            last.callLoadAndActive();
        }

        this.clear = function () {
            _modalBox.clear();
            if (links.count() > 0) {
                links = new linkManagement();
            }
        }

        this.getNode = function () {
            return links.last().node;
        }
    }

    function openArea() {
        var links = new linkManagement(),
            loadOption = { closed: null };

        function setView(link, first, second) {
            var last;

            if (links.count() > 0) {
                links.last().hide();
            } else {
                _mainView.inactiveCurrent();
                _activeView.push(_openView);
            }

            links.push("open", link);
            last = links.last();
            if (second == undefined) {
                last.setContext(first);
            } else {
                last.setContext(first, second);
            }
            _windowBox.open(last.node, function () {
                last.remove();
            });
            last.callLoadAndActive();
            last.getOption().closed = loadOption.closed;
        }

        this.load = function (link, data, type, loadCompleted) {
            var loading = $this.loadingTip.show(),
                loadPath;

            _modalView.clear();

            loadPath = getAbsolutePath(link, getPathHasAbsolutePathFromArray(links.getLinks(), links.count() - 2, _mainView.getDefaultLink()), _mainView.getDefaultLink());
            $.ajax({
                mimeType: 'text/html; charset=utf-8',
                url: loadPath,
                data: data,
                type: type,
                timeout: _setting.timeover
            }).done(function (data, textStatus, jqXHR) {
                loading.hide();
                var json = jqXHR.getResponseHeader("X-Responded-JSON"),
	                responded;

                if (json) {
                    responded = JSON.parse(json);
                    if (responded.status == 401) {
                        if (!_fnOnUnauthorized(responded.headers.location)) {
                            if (responded.headers && responded.headers.location) {
                                document.location = responded.headers.location;
                                return;
                            }
                        }
                    }
                }

                if (isHtmlDocument(data)) {
                    alert("You try to load wrong content: " + loadPath);
                    return;
                }

                setView(link, data);
                if (loadCompleted) loadCompleted(true);
            }).fail(function (jqXHR, textStatus, errorThrown) {
                loading.hide();
                if (jqXHR.status == 401) {
                    _fnOnUnauthorized(link);
                }
                var response = $(jqXHR.responseText),
                    title = $("<h4></h4>").text(errorThrown),
                    content = $("<pre></pre>");

                if (response[11] != null && response[11].nodeType == 8) {
                    content.text(response[11].data);
                } else {
                    content.text(response.eq(1).text());
                }

                setView(link, title, content);
                if (loadCompleted) loadCompleted(true);
            });
            loadOption.closed = null;
            return loadOption;
        }

        this.close = function (parameter, closeCompleted) {
            var lastClosed = links.pop().getOption().closed;
            _windowBox.close(null, function () {
                var current = links.last();
                if (current) {
                    current.show();
                } else {
                    _activeView.pop();
                    _mainView.activeCurrent();
                }
                if (lastClosed) lastClosed(parameter);
                if (closeCompleted) closeCompleted(false);
            });
        }

        this.replace = function (link, data) {
            var last = links.last();
            last.callInactiveAndUnload();
            last.link = link;
            last.setContext(data);
            last.callLoadAndActive();
        }

        this.clear = function () {
            _windowBox.clear();
            if (links.count() > 0) {
                links = new linkManagement();
            }
        }

        this.getNode = function () {
            return links.last().node;
        }
    }

    function viewArea(viewNode, link) {
        var loadId = 0,
            element = $(viewNode),
            defaultLink = link,
            links = new linkManagement();

        function setView(link, first, second) {
            var last;

            last = links.last();
            if (second == undefined) {
                last.setContext(first);
            } else {
                last.setContext(first, second);
            }
            last.callLoadAndActive();
            $this.resetWindowScrollbar();
            $(window).scrollTop(0);
            $this.dealScrollToVisibleLoading();
        }

        function loadContent(link, basePath, loadContentCompleted, loadCompleted) {
            var currentId = ++loadId,
                loading,
                loadPath;

            loading = $this.loadingTip.show();
            loadPath = getAbsolutePath(link, basePath, defaultLink);
            $.ajax({
                mimeType: 'text/html; charset=utf-8',
                url: loadPath,
                type: 'GET',
                timeout: _setting.timeover
            }).done(function (data, textStatus, jqXHR) {
                if (currentId != loadId) {
                    return;
                }
                loading.hide();

                var json = jqXHR.getResponseHeader("X-Responded-JSON"),
                    responded;

                if (json) {
                    responded = JSON.parse(json);
                    if (responded.status == 401) {
                        if (!_fnOnUnauthorized(responded.headers.location)) {
                            if (responded.headers && responded.headers.location) {
                                document.location = responded.headers.location;
                                return;
                            }
                        }
                    }
                }

                if (isHtmlDocument(data)) {
                    alert("You try to load wrong content: " + loadPath);
                    return;
                }
                loadContentCompleted();
                setView(link, data);
                if (loadCompleted) loadCompleted(true);
            }).fail(function (jqXHR, textStatus, errorThrown) {
                if (currentId != loadId) {
                    return;
                }
                loading.hide();

                if (jqXHR.status == 401) {
                    _fnOnUnauthorized(link);
                }
                var response = $(jqXHR.responseText),
                    title = $("<h4></h4>").text(errorThrown),
                    content = $("<pre></pre>");

                if (response[11] != null && response[11].nodeType == 8) {
                    content.text(response[11].data);
                } else {
                    content.text(response.eq(1).text());
                }
                loadContentCompleted();
                setView(link, title, content);
                if (loadCompleted) loadCompleted(true);
            });
        }

        this.load = function (link, loadCompleted) {
            var hrefs,
                i;

            link = link.replace(/%23/g, '#');
            hrefs = link.split("#")
            _modalView.clear();
            _openView.clear();
            _messageBox.clear();

            if (links.count() > hrefs.length && links.like(hrefs) && links.get(hrefs.length - 1).valid) {
                for (i = links.count() - 1; i > hrefs.length - 1; i--) {
                    links.pop().remove();
                }
                links.last().show();
                $this.resetWindowScrollbar();
                if (loadCompleted) loadCompleted(false);
                return;
            }

            loadContent(hrefs[hrefs.length - 1], getPathHasAbsolutePathFromArray(hrefs, hrefs.length - 2, defaultLink), function () {
                var i,
                    linksCount;
                if (links.count() > hrefs.length && links.like(hrefs)) {
                    for (i = links.count() - 1; i > hrefs.length - 1; i--) {
                        links.pop().remove();
                    }
                } else {
                    linksCount = links.count();
                    for (i = linksCount - 1; i > hrefs.length - 1; i--) {
                        links.pop().remove();
                    }
                    for (i = 0; i < hrefs.length; i++) {
                        if (linksCount > i) {
                            if (links.get(i).link != hrefs[i] || (linksCount == i + 1 && hrefs.length == linksCount)) {
                                links.replace(i, "main", hrefs[i]);
                            } else {
                                links.get(i).hide();
                            }
                        } else {
                            links.push("main", hrefs[i]);
                            element.append(links.last().node);
                        }
                    }
                }
            }, loadCompleted);
        }

        // this parameter just for like openView.close
        this.close = function (parameter, closeCompleted) {
            if (links.count() > 1) {
                if (links.get(links.count() - 2).valid) {
                    links.pop().remove();
                    links.last().show();
                    $this.resetWindowScrollbar();
                    $this.linker.modify(links.getLink());
                    if (closeCompleted) closeCompleted(false);
                } else {
                    $this.linker.setChangeCompleted(closeCompleted);
                    $this.linker.hash(links.getBackLink());
                }
            }
        }

        this.replace = function (link, data) {
            var last = links.last();
            last.callInactiveAndUnload();
            last.link = link;
            last.setContext(data);
            last.callLoadAndActive();
        }

        this.getElement = function () {
            return element;
        }

        this.setElement = function (node) {
            return element = $(node);
        }

        this.getDefaultLink = function () {
            return defaultLink;
        }

        this.setDefaultLink = function (link) {
            defaultLink = link;
        }

        this.activeCurrent = function () {
            links.last().activeEvent();
        }

        this.inactiveCurrent = function () {
            links.last().inactiveEvent();
        }

        this.getLinks = function () {
            return links.getLinks();
        }

        this.getNode = function () {
            return links.last().node;
        }
    }

    this.current = function () {
        return {
            node: _activeView.get().getNode(),
            view: _activeView.get()
        };
    }

    this.event = function () {
        return new function () {
            this.onLoad = function (fn) {
                _currentViewEvent.load = fn;
                return this;
            }

            this.onUnload = function (fn) {
                _currentViewEvent.unload = fn;
                return this;
            }

            this.onActive = function (fn) {
                _currentViewEvent.active = fn;
                return this;
            }

            this.onInactive = function (fn) {
                _currentViewEvent.inactive = fn;
                return this;
            }
        }
    }

    this.viewClose = function (parameter, closeCompleted) {
        _activeView.get().close(parameter, closeCompleted);
    }

    this.dealScrollToVisibleLoading = function () {
        var loading = $(".webapp-loading:visible"),
	        src;

        if (loading.length > 0 && !loading.data("work") && $(window).scrollTop() + $(window).height() > loading.offset().top) {
            loading.data("work", true);
            src = loading.attr("data-src");
            if (!src) return;
            $.get(src, function (data) {
                loading.before(data);
                loading.remove();
                $this.resetWindowScrollbar();
                $this.dealScrollToVisibleLoading();
            });
        }
    }

    function dealTouchMove(e) {
        var path,
            i,
            isCancel = true;

        if ($("body").hasClass("layout-expanded")) {
            path = e.originalEvent.path
            for (i = 0; i < path.length; i++) {
                if (path[i].tagName == "BODY") break;
                if (path[i].scrollHeight > path[i].clientHeight) {
                    isCancel = false;
                    break;
                }
            }
            if (isCancel) {
                e.preventDefault();
            }
        }
    }

    _dealHrefTarget = {
        _base: function (href) {
            $this.linker.hash(href);
        },
        _add: function (href) {
            $this.linker.addHash(href);
        },
        _same: function (href) {
            $this.linker.sameHash(href);
        },
        _open: function (href, caller) {
            $this.open(href, caller.attr("data-data"));
        },
        _modal: function (href, caller) {
            $this.modal(href, caller.attr("data-data"));
        }
    }

    this.configTarget = function (fn) {
        if (typeof fn == "function") fn(_dealHrefTarget);
    }

    this.hashes = function () {
        return _mainView.getLinks();
    }

    this.open = function (href, data) {
        var option;
        if (data) {
            option = _openView.load(href, data, 'POST');
        } else {
            option = _openView.load(href, data, 'GET');
        }
        return new function () {
            this.closed = function (fn) {
                option.closed = fn;
            }
        }
    }

    this.modal = function (href, data) {
        var option;
        if (data) {
            option = _modalView.load(href, data, 'POST');
        } else {
            option = _modalView.load(href, data, 'GET');
        }
        return new function () {
            this.closed = function (fn) {
                option.closed = fn;
            }
        }
    }

    this.configSetting = function (fn) {
        if (typeof fn == "function") fn(_setting);
    }

    this.configText = function (fn) {
        if (typeof fn == "function") fn(_text);
    }

    this.init = function (viewNode, defaultLink) {
        function option(main) {
            this.viewNode = function (node) {
                if (!node) return main.getElement();
                main.setElement(node);
                return this;
            }
            this.defaultLink = function (link) {
                if (!link) return main.getDefaultLink();
                main.setDefaultLink(link);
                return this;
            }
            this.unauthorized = function (fn) {
                _fnOnUnauthorized = fn;
                return this;
            }
            this.dealLinkEmptyTarget = function (b) {
                if (b == undefined) return _isDealLinkEmptyTarget;
                _isDealLinkEmptyTarget = b;
                return this;
            }
            this.replacePCScrollBar = function (b) {
                _isReplacePCScrollBar = b;
                return this;
            }
            this.viewLoaded = function (fn) {
                _globalViewEvent.load = fn;
                return this;
            }
            this.viewActived = function (fn) {
                _globalViewEvent.active = fn;
                return this;
            }
            this.viewInactived = function (fn) {
                _globalViewEvent.inactive = fn;
                return this;
            }
            this.viewUnloaded = function (fn) {
                _globalViewEvent.unload = fn;
                return this;
            }
        }

        if (!!window.ActiveXObject || "ActiveXObject" in window) {
            $.ajaxSetup({ cache: false });
        }
        $(document).on("click", "a", function (e) {
            var target = $(this).attr("target"),
                href = $(this).attr('href');

            if (href == '#') {
                e.preventDefault();
                return;
            }

            if (!target) {
                if (!_isDealLinkEmptyTarget) return;
                e.preventDefault();
                _dealHrefTarget._base(href);
                return;
            }
            if (target == "_none") {
                return;
            }
            if (_dealHrefTarget[target]) {
                e.preventDefault();
                _dealHrefTarget[target](href, $(this));
            }
        });
        $(document).on("click", ".webapp-close", function (e) {
            e.preventDefault();
            $this.viewClose($(this).attr("data-close"));
        });
        $(window).on("scroll", $this.dealScrollToVisibleLoading);
        $(window).on("resize", $this.dealScrollToVisibleLoading);
        $(document).on("touchmove", dealTouchMove);

        _globalViewEvent = new viewEvent();
        _mainView = new viewArea(viewNode, defaultLink);
        _openView = new openArea();
        _modalView = new modalArea();
        _activeView.push(_mainView);
        $this.linker._init(function (link) {
            _mainView.load(link, $this.linker.callChangeCompleted);
        });
        return new option(_mainView);
    }

    window.$app = {
        configSetting: $this.configSetting,
        configText: $this.configText,
        alert: $this.dialog.alert,
        confirm: $this.dialog.confirm,
        message: $this.dialog.message,
        loading: $this.loadingTip.show,
        loadScript: $this.scriptLoader.load,
        hash: $this.linker.hash,
        baseHash: $this.linker.hash,
        addHash: $this.linker.addHash,
        sameHash: $this.linker.sameHash,
        reload: $this.linker.refresh,
        open: $this.open,
        modal: $this.modal,
        event: $this.event,
        close: $this.viewClose,
        current: $this.current,
        init: $this.init
    };
})();