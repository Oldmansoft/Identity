/*
* v0.34.120
* https://github.com/Oldmansoft/webapp
* Copyright 2016 Oldmansoft, Inc; http://www.apache.org/licenses/LICENSE-2.0
*/
if (!window.oldmansoft) window.oldmansoft = {};
window.oldmansoft.webapp = new (function () {
    var $this = this,
    _setting = {
        timeover: 180000,
        loading_show_time: 1000,
        loading_hide_time: 200,
        server_charset: "utf-8"
    },
    _text = {
        ok: "Ok",
        yes: "Yes",
        no: "No",
        loading: "Loading",
        load_layout_error: "load layout error, click Ok to reload.",
    },
    _mainViewArea = null,
    _openViewArea = null,
    _modalViewArea = null,
    _activeViewAreaManager = null,
    _fnOnUnauthorized = function () {
        return false;
    },
    _fnOnLoadTimeout = function () {
        return false;
    },
    _currentViewEvent = null,
    _globalViewEvent = null,
    _visibleLoadingCompleted = [],
    _isDealLinkEmptyTarget = true,
    _dealHrefTarget,
    _isReplacePCScrollBar = true,
    _WindowScrollBar = null,
    _scrollbar = [],
    _canTouch = null,
    _messageBox,
    _windowBox,
    _modalBox,
    _canSetup = true,
    _initialledEvents = [],
    _initialization_option,
    _hideMainViewFirstLoading = false;

    function linkEncode(text) {
        if (!text) return "";
        if (text.indexOf("#") == 0) return text.substr(1, text.length - 1);
        return text.replace(/\(/g, "(9)").replace(/\?/g, "(0)").replace(/\//g, "(1)").replace(/_/g, "(2)").replace(/#/g, "(3)").replace(/&/g, "(4)");
    }

    function linkDecode(code) {
        if (!code) return "";
        return code.replace(/\(0\)/g, "?").replace(/\(1\)/g, "/").replace(/\(2\)/g, "_").replace(/\(3\)/g, "#").replace(/\(4\)/g, "&").replace(/\(9\)/g, "(");
    }

    function linkDecode_tilde(code) {
        if (!code) return "";
        return code.replace(/\$7e/g, "~").replace(/\$23/g, "#").replace(/\$2f/g, "/").replace(/\$3f/g, "?").replace(/\$24/g, "$");
    }

    function linkParser(input) {
        var store = [],
            hashContent,
            i;

        function getHashContent(hash) {
            if (!hash) return "";
            if (hash.substr(0, 1) == "#") return hash.substr(1);
            return hash;
        }

        if (input instanceof Array) store = input.slice();
        else {
            hashContent = getHashContent(input);
            if (hashContent.indexOf("#") > -1 || hashContent.indexOf("%23") > -1) {
                store = hashContent.replace(/%23/g, "#").split("#");
            } else if (hashContent.indexOf("$") > -1) {
                store = hashContent.split("~");
                for (i = 0; i < store.length; i++) {
                    store[i] = linkDecode_tilde(store[i]);
                }
            } else {
                store = hashContent.split("_");
                for (i = 0; i < store.length; i++) {
                    store[i] = linkDecode(store[i]);
                }
            }
        }

        this.getLinks = function () {
            return store.slice();
        }

        this.push = function (item) {
            return store.push(getHashContent(item));
        }

        this.pop = function () {
            return store.pop();
        }

        this.last = function () {
            return store[store.length - 1];
        }

        this.getContent = function () {
            var links = [],
                i;
            for (i = 0; i < store.length; i++) {
                links.push(linkEncode(store[i]));
            }
            return links.join("_");
        }
    }

    this.parser = function (input) {
        return new linkParser(input);
    }

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
                throw new Error("shrink error");
            }
        }
    }

    this.scrollbar = function (target) {
        if (!target) {
            return;
        }
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
            if (element.selector == "body") {
                if (value != undefined) {
                    $(document).scrollTop(value);
                } else {
                    return $(document).scrollTop();
                }
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
            var node = e.target,
                delta = e.originalEvent.wheelDelta,
                targetScrollTop,
                overflowY;
            while (node != e.currentTarget && node != null && node.tagName != "HTML" && node.tagName != "BODY") {
                overflowY = node.style.overflowY;
                if ((overflowY == "auto" || overflowY == "scroll") && node.clientHeight != node.scrollHeight) {
                    if (delta > 0 && node.scrollTop > 0) {
                        return true;
                    }
                    if (delta < 0 && node.scrollTop + node.clientHeight < node.scrollHeight) {
                        return true;
                    }
                }
                node = node.parentElement;
            }

            if (targetHelper.contentHeight() <= targetHelper.viewHeight()) return true;
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
            if (isShow) {
                return;
            }
            container.show();
            targetHelper.bindMouseWheel();
            isShow = true;
        }
        this.hide = function () {
            if (!isShow) {
                return;
            }
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
        function events() {
            var list = [];
            this.add = function (fn) {
                if (typeof (fn) != "function") throw new Error("fn is not a function");
                list.push(fn);
            }
            this.execute = function (para) {
                for (var i = 0; i < list.length; i++) {
                    if (list[i](para) === false) return false;
                }
            }
        }

        this.load = new events();
        this.unload = new events();
        this.active = new events();
        this.inactive = new events();
    }

    function viewEventParameter(node, name, level) {
        this.node = node;
        this.name = name;
        this.level = level;
    }

    function linkManagement() {
        var context = [];

        function item(name, link, level, option) {
            var eventParameter,
                visible = true,
                scrollTop = 0,
                scrollLeft = 0,
                localViewEvent;

            this.link = link;
            this.node = $("<div></div>").addClass(name + "-view").data("link", link);
            this.valid = false;
            eventParameter = new viewEventParameter(this.node, name, level);

            this.hide = function () {
                if (!this.valid || !visible) {
                    return;
                }
                var win = $(window);
                scrollTop = win.scrollTop();
                scrollLeft = win.scrollLeft();
                if (localViewEvent.inactive.execute(eventParameter) !== false) {
                    _globalViewEvent.inactive.execute(eventParameter);
                }
                this.node.hide();
                visible = false;
            }

            this.callLoadAndActive = function () {
                if (localViewEvent.load.execute(eventParameter) !== false) {
                    _globalViewEvent.load.execute(eventParameter);
                }
                if (localViewEvent.active.execute(eventParameter) !== false) {
                    _globalViewEvent.active.execute(eventParameter);
                }
            }

            this.callInactiveAndUnload = function () {
                if (localViewEvent.inactive.execute(eventParameter) !== false) {
                    _globalViewEvent.inactive.execute(eventParameter);
                }
                if (localViewEvent.unload.execute(eventParameter) !== false) {
                    _globalViewEvent.unload.execute(eventParameter);
                }
            }

            this.remove = function () {
                if (this.valid && localViewEvent.inactive.execute(eventParameter) !== false) {
                    _globalViewEvent.inactive.execute(eventParameter);
                }
                this.node.remove();
                if (!this.valid) {
                    return;
                }
                if (localViewEvent.unload.execute(eventParameter) !== false) {
                    _globalViewEvent.unload.execute(eventParameter);
                }
                this.node = null;
                this.valid = false;
                localViewEvent = null;
            }

            this.show = function () {
                if (!this.valid || visible) {
                    return;
                }
                this.node.show();
                if (localViewEvent.active.execute(eventParameter) !== false) {
                    _globalViewEvent.active.execute(eventParameter);
                }
                $(window).scrollLeft(scrollLeft);
                $(window).scrollTop(scrollTop);
                visible = true;
            }

            this.activeEvent = function () {
                if (!localViewEvent) {
                    _globalViewEvent.active.execute(eventParameter);
                    return;
                }
                if (localViewEvent.active.execute(eventParameter) !== false) {
                    _globalViewEvent.active.execute(eventParameter);
                }
            }

            this.inactiveEvent = function () {
                if (!localViewEvent) {
                    _globalViewEvent.inactive.execute(eventParameter);
                    return;
                }
                if (localViewEvent.inactive.execute(eventParameter) !== false) {
                    _globalViewEvent.inactive.execute(eventParameter);
                }
            }

            this.setContext = function () {
                _currentViewEvent = new viewEvent();

                this.node.empty();
                if (arguments.length == 1) {
                    this.node.html(arguments[0]);
                } else {
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

        this.push = function (name, link, option) {
            context.push(new item(name, link, this.count() + 1, option));
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

        this.replace = function (index, name, link, option) {
            var newItem = new item(name, link, index + 1, option);
            context[index].node.after(newItem.node);
            context[index].remove();
            context[index] = newItem;
        }

        this.getBackLink = function () {
            var link = new linkParser(this.getLinks());
            link.pop();
            return link.getContent();
        }

        this.getLink = function () {
            var link = new linkParser(this.getLinks());
            return link.getContent();
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
            if (event && event.target != event.currentTarget) {
                return;
            }

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
                    element.stop(true, true);
                    element.fadeIn(0);
                    if (fn) fn();
                    return;
                }

                current = null;
                if (fn) fn();
            });
        }
        function initElement() {
            if (isInit) {
                return;
            }
            isInit = true;
            element = $("<div></div>").addClass(className).addClass("box-background");
            if (isMiddle) {
                core = $("<div></div>").addClass("layout-center-content");
                element.append($("<div></div>").addClass("layout-center").append(core));
            } else {
                core = element;
            }
            element.prependTo($("body"));
            element.on("scroll", $this.dealScrollToVisibleLoading);
        }

        this.open = function (node, fnClose) {
            initElement();
            if (current) {
                if (current.node.data("type") == "message") {
                    throw new Error("not allow show again after message show.");
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
            if (!current) {
                return;
            }
            $this.bodyManagement.shrink();
            if (current.close) current.close();
            current.node.remove();
            while (store.length > 0) {
                current = store.pop();
                core.append(current.node);
                $this.bodyManagement.shrink();
                if (current.close) current.close();
                current.node.remove();
            }
            current = null;
            element.hide();
        }
    }

    _activeViewAreaManager = new function () {
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
                throw new Error("error call");
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
            if (event && event.target != event.currentTarget) {
                return;
            }

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
                    current.node.stop(true, true);
                    current.node.fadeIn(0);
                    if (fn) fn();
                    return;
                }

                current = null;
                if (fn) fn();
            });
        }
        function initElement() {
            if (isInit) {
                return;
            }
            isInit = true;

            element = $("<div></div>").addClass("modal-areas");
            element.prependTo($("body"));
        }
        function createNode(node) {
            var container,
                main;

            container = $("<div></div>").addClass("modal-background").addClass("box-background");
            main = $("<div></div>").addClass("layout-center-content")
            main.append(node);
            container.append($("<div></div>").addClass("layout-center").append(main));
            container.appendTo(element);
            container.on("click", function (e) {
                if (e.currentTarget != e.target && e.currentTarget != e.target.parentElement) {
                    return;
                }
                if ($(e.currentTarget).find(".force").length > 0) {
                    return;
                }
                _modalViewArea.close();
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
            if (!current) {
                return;
            }
            $this.bodyManagement.shrink();
            if (current.close) current.close();
            current.node.remove();
            while (store.length > 0) {
                current = store.pop();
                $this.bodyManagement.shrink();
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
                var body = $("<div></div>").addClass("dialog-body");
                if (text.indexOf("\n") > -1) {
                    $.each(text.split("\n"), function (i, n) {
                        body.append($("<p></p>").text(n));
                    });
                } else {
                    body.text(text);
                }
                element.append(body);
            }
            this.setFooter = function () {
                var footer = $("<div></div>").addClass("dialog-footer");

                function option(node) {
                    this.set = function (text, className) {
                        var closeCallback,
                            button = $("<button></button>").text(text).addClass(className);

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
            okButton = builder.setFooter().set(_text.ok, "ok");
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
            yesButton = footer.set(_text.yes, "yes");
            if (fnYes) {
                console.warn("fnYes is obsolete.");
                yesButton.setCallback(fnYes);
            }
            noButton = footer.set(_text.no, "no");
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
            if (element != null) {
                return;
            }
            element = $("<div></div>").addClass("loading-background").addClass("box-background");
            var dialog = $("<div></div>").addClass("loading-box").addClass("box-panel"),
                text = $("<span></span>").text(_text.loading);

            dialog.append(text);
            element.append($("<div></div>").addClass("layout-center").append($("<div></div>").addClass("layout-center-content").append(dialog)));
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
            changeCompleted = null,
            changeCompletedParameters = null;

        function fixHref(href) {
            if (!href) return href;
            if (href.substr(0, 1) == "#") return href.substr(1);
            return href;
        }
        function callLeave() {
            changeCallback(lastHash);
        }
        function hashChange() {
            var href = new linkParser(window.location.hash).getContent();
            if (lastHash == href) {
                return;
            }
            lastHash = href;
            callLeave();
        }

        this.setChangeCompleted = function (fn, parameters) {
            changeCompleted = fn;
            changeCompletedParameters = parameters;
        }

        this.callChangeCompleted = function (isNewContent) {
            if (!changeCompleted) {
                return;
            }
            if (changeCompletedParameters == null) {
                changeCompleted(isNewContent);
            } else {
                changeCompletedParameters.unshift(isNewContent);
                changeCompleted.apply(null, changeCompletedParameters);
            }
            changeCompleted = null;
            changeCompletedParameters = null;
        }

        this.createHref = function (href) {
            var hash = new linkParser(href).getContent(),
                base = document.location.origin + document.location.pathname;
            if (hash == "") return base;
            return base + "#" + hash;
        }
        this.modify = function (href) {
            window.location.hash = href;
            lastHash = new linkParser(href).getContent();
        }
        this.hash = function (href) {
            if (href == undefined) {
                return window.location.hash;
            }

            window.location.hash = href;
            if (href == lastHash) {
                callLeave();
            }
            return href;
        }
        this.link = function (href) {
            if (href == undefined) {
                return _activeViewAreaManager.get().getLink();
            }
            href = linkEncode(href)
            window.location.hash = href;
            if (href == lastHash) {
                callLeave();
            }
        }
        this.add = function (href) {
            var link = new linkParser(window.location.hash);
            link.push(href);
            window.location.hash = link.getContent();
        }
        this.same = function (href) {
            var link = new linkParser(window.location.hash);
            link.pop();
            link.push(href);
            window.location.hash = link.getContent();
            if (link.getContent() == lastHash) {
                callLeave();
            }
        }
        this.refresh = function () {
            callLeave();
        }
        this._init = function (fnChangeCall) {
            if (initHashChange) {
                return;
            }
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

    function modalViewArea() {
        var links = new linkManagement(),
            loadOption;

        function setView(link, data, type, first, second) {
            var last;

            if (links.count() > 0) {
                links.last().inactiveEvent();
            } else {
                _activeViewAreaManager.get().inactiveCurrent();
                _activeViewAreaManager.push(_modalViewArea);
            }

            links.push("modal", link, { closed: loadOption.closed, closedParameters: loadOption.closedParameters, data: data, type: type });
            last = links.last();
            last.node.addClass("box-panel");
            if (loadOption.force) last.node.addClass("force");
            if (second == undefined) {
                last.setContext(first);
            } else {
                last.setContext(first, second);
            }
            _modalBox.open(last.node, function () {
                last.remove();
            });
            last.callLoadAndActive();
            $this.dealScrollToVisibleLoading();
        }

        function setOldView(first, second) {
            if (links.count() == 0) {
                return;
            }

            var last = links.last();
            last.callInactiveAndUnload();
            if (second == undefined) {
                last.setContext(first);
            } else {
                last.setContext(first, second);
            }
            last.callLoadAndActive();
            $this.dealScrollToVisibleLoading();
        }

        this.load = function (link, data, type) {
            var loading = $this.loadingTip.show(),
                loadPath;

            loadPath = getAbsolutePath(link, getPathHasAbsolutePathFromArray(links.getLinks(), links.count() - 2, _mainViewArea.getDefaultLink()), _mainViewArea.getDefaultLink());
            $.ajax({
                mimeType: 'text/html; charset=' + _setting.server_charset,
                url: loadPath,
                data: data,
                type: type,
                timeout: _setting.timeover
            }).done(function (content, textStatus, jqXHR) {
                loading.hide();
                var json = jqXHR.getResponseHeader("X-Responded-JSON"),
	                responded;

                if (json) {
                    responded = JSON.parse(json);
                    if (responded.status == 401) {
                        if (!_fnOnUnauthorized(loadPath, responded.headers.location)) {
                            if (responded.headers && responded.headers.location) {
                                document.location = responded.headers.location;
                            }
                        }
                        return;
                    }
                }

                if (isHtmlDocument(content)) {
                    alert("You try to load wrong content: " + loadPath);
                    return;
                }

                if (loadOption.refresh) setOldView(content);
                else setView(link, data, type, content);
                if (loadOption.loaded) loadOption.loaded();
            }).fail(function (jqXHR, textStatus, errorThrown) {
                var onLoadTimeoutResult,
                    response,
                    title,
                    content;
                loading.hide();
                if (jqXHR.status == 401) {
                    _fnOnUnauthorized(loadPath);
                } else if (jqXHR.status == 0 && textStatus == "timeout") {
                    onLoadTimeoutResult = _fnOnLoadTimeout();
                    if (onLoadTimeoutResult) {
                        if (loadOption.refresh) setOldView(onLoadTimeoutResult);
                        else setView(link, data, type, onLoadTimeoutResult);
                        return;
                    }
                }
                response = $(jqXHR.responseText);
                title = $("<h4></h4>").text(errorThrown);
                content = $("<pre></pre>");

                if (response[11] != null && response[11].nodeType == 8) {
                    content.text(response[11].data);
                } else {
                    content.text(response.eq(1).text());
                }

                if (loadOption.refresh) setOldView(title, content);
                else setView(link, data, type, title, content);
            });
            loadOption = { closed: null, closedParameters: null, loaded: null, refresh: false, force: false };
            return loadOption;
        }

        this.reload = function () {
            var linkOption = links.last().getOption(),
                option = this.load(links.last().link, linkOption.data, linkOption.type);
            option.refresh = true;
        }

        this.close = function () {
            var closeCompleted = null,
                parameters = Array.from(arguments);
            _modalBox.close(null, function () {
                var option = links.pop().getOption(),
                    current = links.last();
                if (current) {
                    current.activeEvent();
                } else {
                    _activeViewAreaManager.pop();
                    _activeViewAreaManager.get().activeCurrent();
                }
                if (option.closed) {
                    if (option.closedParameters != null) {
                        if (parameters.length > 0) option.closedParameters = option.closedParameters.concat(parameters);
                        option.closed.apply(null, option.closedParameters);
                    } else {
                        option.closed.apply(null, parameters);
                    }
                }
                if (closeCompleted) closeCompleted(false);
            });
            return new function () {
                this.completed = function (fn) {
                    closeCompleted = fn;
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

        this.clear = function () {
            _modalBox.clear();
            if (links.count() > 0) {
                links = new linkManagement();
                _activeViewAreaManager.pop();
            }
        }

        this.getNode = function () {
            return links.last().node;
        }

        this.getLink = function () {
            return links.last().link;
        }
    }

    function openViewArea() {
        var links = new linkManagement(),
            loadOption;

        function setView(link, data, type, first, second) {
            var last;

            if (links.count() > 0) {
                links.last().hide();
            } else {
                _activeViewAreaManager.get().inactiveCurrent();
                _activeViewAreaManager.push(_openViewArea);
            }

            links.push("open", link, { closed: loadOption.closed, closedParameters: loadOption.closedParameters, data: data, type: type });
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
            $this.dealScrollToVisibleLoading();
        }

        function setOldView(first, second) {
            if (links.count() == 0) {
                return;
            }

            var last = links.last();
            last.callInactiveAndUnload();
            if (second == undefined) {
                last.setContext(first);
            } else {
                last.setContext(first, second);
            }
            last.callLoadAndActive();
            $this.dealScrollToVisibleLoading();
        }

        this.load = function (link, data, type) {
            var loading = $this.loadingTip.show(),
                loadPath;

            _modalViewArea.clear();

            loadPath = getAbsolutePath(link, getPathHasAbsolutePathFromArray(links.getLinks(), links.count() - 2, _mainViewArea.getDefaultLink()), _mainViewArea.getDefaultLink());
            $.ajax({
                mimeType: 'text/html; charset=' + _setting.server_charset,
                url: loadPath,
                data: data,
                type: type,
                timeout: _setting.timeover
            }).done(function (content, textStatus, jqXHR) {
                loading.hide();
                var json = jqXHR.getResponseHeader("X-Responded-JSON"),
	                responded;

                if (json) {
                    responded = JSON.parse(json);
                    if (responded.status == 401) {
                        if (!_fnOnUnauthorized(loadPath, responded.headers.location)) {
                            if (responded.headers && responded.headers.location) {
                                document.location = responded.headers.location;
                            }
                        }
                        return;
                    }
                }

                if (isHtmlDocument(content)) {
                    alert("You try to load wrong content: " + loadPath);
                    return;
                }

                if (loadOption.refresh) setOldView(content);
                else setView(link, data, type, content);
                if (loadOption.loaded) loadOption.loaded();
            }).fail(function (jqXHR, textStatus, errorThrown) {
                var onLoadTimeoutResult,
                    response,
                    title,
                    content;
                loading.hide();
                if (jqXHR.status == 401) {
                    _fnOnUnauthorized(loadPath);
                } else if (jqXHR.status == 0 && textStatus == "timeout") {
                    onLoadTimeoutResult = _fnOnLoadTimeout();
                    if (onLoadTimeoutResult) {
                        if (loadOption.refresh) setOldView(onLoadTimeoutResult);
                        else setView(link, data, type, onLoadTimeoutResult);
                        return;
                    }
                }
                response = $(jqXHR.responseText);
                title = $("<h4></h4>").text(errorThrown);
                content = $("<pre></pre>");

                if (response[11] != null && response[11].nodeType == 8) {
                    content.text(response[11].data);
                } else {
                    content.text(response.eq(1).text());
                }

                if (loadOption.refresh) setOldView(title, content);
                else setView(link, data, type, title, content);
            });
            loadOption = { closed: null, closedParameters: null, loaded: null, refresh: false };
            return loadOption;
        }

        this.reload = function () {
            var linkOption = links.last().getOption(),
                option = this.load(links.last().link, linkOption.data, linkOption.type);
            option.refresh = true;
        }

        this.close = function () {
            var closeCompleted = null,
                parameters = Array.from(arguments);
            _windowBox.close(null, function () {
                var option = links.pop().getOption(),
                    current = links.last();
                if (current) {
                    current.show();
                } else {
                    _activeViewAreaManager.pop();
                    _mainViewArea.activeCurrent();
                }
                if (option.closed) {
                    if (option.closedParameters != null) {
                        if (parameters.length > 0) option.closedParameters = option.closedParameters.concat(parameters);
                        option.closed.apply(null, option.closedParameters);
                    } else {
                        option.closed.apply(null, parameters);
                    }
                }
                if (closeCompleted) closeCompleted(false);
            });
            return new function () {
                this.completed = function (fn) {
                    closeCompleted = fn;
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

        this.clear = function () {
            _windowBox.clear();
            if (links.count() > 0) {
                links = new linkManagement();
                _activeViewAreaManager.pop();
            }
        }

        this.activeCurrent = function () {
            links.last().activeEvent();
        }

        this.inactiveCurrent = function () {
            links.last().inactiveEvent();
        }

        this.getNode = function () {
            return links.last().node;
        }

        this.getLink = function () {
            return links.last().link;
        }
    }

    function mainViewArea(viewNodeSelector, link) {
        var loadId = 0,
            element = null,
            defaultLink = link,
            links = new linkManagement(),
            firstLoadContent = true;

        function setView(first, second) {
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

        function loadContent(link, baseLink, loadContentCompleted, loadCompleted) {
            var currentId = ++loadId,
                loading = null,
                loadPath = getAbsolutePath(link, baseLink, defaultLink);

            if (_hideMainViewFirstLoading) {
                _hideMainViewFirstLoading = false;
            } else {
                loading = $this.loadingTip.show();
            }

            $.ajax({
                mimeType: 'text/html; charset=' + _setting.server_charset,
                url: loadPath,
                type: 'GET',
                timeout: _setting.timeover
            }).done(function (data, textStatus, jqXHR) {
                if (currentId != loadId) {
                    return;
                }
                if (loading) loading.hide();

                var json = jqXHR.getResponseHeader("X-Responded-JSON"),
                    responded;

                if (json) {
                    responded = JSON.parse(json);
                    if (responded.status == 401) {
                        if (!_fnOnUnauthorized(loadPath, responded.headers.location)) {
                            if (responded.headers && responded.headers.location) {
                                document.location = responded.headers.location;
                            }
                        }
                        return;
                    }
                }

                if (isHtmlDocument(data)) {
                    alert("You try to load wrong content: " + loadPath);
                    return;
                }
                loadContentCompleted();
                setView(data);
                if (loadCompleted) loadCompleted(true);
            }).fail(function (jqXHR, textStatus, errorThrown) {
                var onLoadTimeoutResult,
                    response,
                    title,
                    content;
                if (currentId != loadId) {
                    return;
                }
                if (loading) loading.hide();

                if (jqXHR.status == 401) {
                    _fnOnUnauthorized(loadPath);
                } else if (jqXHR.status == 0 && textStatus == "timeout") {
                    onLoadTimeoutResult = _fnOnLoadTimeout();
                    if (onLoadTimeoutResult) {
                        loadContentCompleted();
                        setView(onLoadTimeoutResult);
                        return;
                    }
                }
                response = $(jqXHR.responseText);
                title = $("<h4></h4>").text(errorThrown);
                content = $("<pre></pre>");

                if (response[11] != null && response[11].nodeType == 8) {
                    content.text(response[11].data);
                } else {
                    content.text(response.eq(1).text());
                }
                loadContentCompleted();
                setView(title, content);
            });
        }

        this.load = function (link, loadCompleted) {
            var hrefs,
                i;
            if (element == null) element = $(viewNodeSelector);
            if (element.is("body")) throw new Error("viewNode can't be <body>");
            hrefs = new linkParser(link).getLinks();
            _modalViewArea.clear();
            _openViewArea.clear();
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

                if (firstLoadContent) {
                    element.empty();
                    firstLoadContent = false;
                }
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
                                links.replace(i, "main", hrefs[i], { baseLink: getPathHasAbsolutePathFromArray(hrefs, i - 1, defaultLink) });
                            } else {
                                links.get(i).hide();
                            }
                        } else {
                            links.push("main", hrefs[i], { baseLink: getPathHasAbsolutePathFromArray(hrefs, i - 1, defaultLink) });
                            element.append(links.last().node);
                        }
                    }
                }
            }, loadCompleted);
        }

        this.reload = function () {
            loadContent(links.last().link, links.last().getOption().baseLink, function () {
                links.last().callInactiveAndUnload();
            });
        }

        this.close = function () {
            var closeCompleted = null,
                parameters = Array.from(arguments);
            if (links.count() > 1) {
                if (links.get(links.count() - 2).valid) {
                    links.pop().remove();
                    $this.linker.modify(links.getLink());
                    links.last().show();
                    $this.resetWindowScrollbar();
                    setTimeout(function () {
                        parameters.unshift(false);
                        if (closeCompleted) closeCompleted.apply(null, parameters);
                    }, 1);
                } else {
                    setTimeout(function () {
                        $this.linker.setChangeCompleted(closeCompleted, parameters);
                        $this.linker.hash(links.getBackLink());
                    }, 1);
                }
            }
            return new function () {
                this.completed = function (fn) {
                    closeCompleted = fn;
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

        this.setElement = function (nodeSelector) {
            return element = $(nodeSelector);
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

        this.getLink = function () {
            return links.last().link;
        }
    }

    this.current = function () {
        return {
            node: _activeViewAreaManager.get().getNode(),
            link: _activeViewAreaManager.get().getLink(),
            view: _activeViewAreaManager.get()
        };
    }

    this.event = function () {
        return new function () {
            this.onLoad = function (fn) {
                _currentViewEvent.load.add(fn);
                return this;
            }

            this.onUnload = function (fn) {
                _currentViewEvent.unload.add(fn);
                return this;
            }

            this.onActive = function (fn) {
                _currentViewEvent.active.add(fn);
                return this;
            }

            this.onInactive = function (fn) {
                _currentViewEvent.inactive.add(fn);
                return this;
            }
        }
    }

    this.viewClose = function (parameter) {
        return _activeViewAreaManager.get().close(parameter);
    }

    this.viewReload = function () {
        _activeViewAreaManager.get().reload();
    }

    this.dealScrollToVisibleLoading = function (rangeNode) {
        var loading,
	        src;
        if (rangeNode && typeof (rangeNode.find) == "function") {
            loading = rangeNode.find(".webapp-loading:visible").first();
        } else {
            loading = $(".webapp-loading:visible").first();
        }

        if (loading.length > 0 && !loading.data("work") && $(window).scrollTop() + $(window).height() > loading.offset().top) {
            loading.data("work", true);
            src = loading.attr("data-src");
            if (!src) {
                return;
            }
            $.get(src, function (data) {
                loading.before(data);
                loading.remove();
                for (var i = 0; i < _visibleLoadingCompleted.length; i++) {
                    if (_visibleLoadingCompleted[i]() === false) return false;
                }
                $this.resetWindowScrollbar();
                $this.dealScrollToVisibleLoading(rangeNode);
            });
        }
    }

    function dealTouchMove(e) {
        var path,
            i,
            isCancel = true;

        if ($("body").hasClass("layout-expanded")) {
            path = e.originalEvent.path;
            if (!path) return;
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
            $this.linker.link(href);
        },
        _link: function (href) {
            $this.linker.link(href);
        },
        _add: function (href) {
            $this.linker.add(href);
        },
        _same: function (href) {
            $this.linker.same(href);
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
        return _mainViewArea.getLinks();
    }

    this.open = function (href, data) {
        var option;
        if (data) {
            option = _openViewArea.load(href, data, 'POST');
        } else {
            option = _openViewArea.load(href, data, 'GET');
        }
        return new function () {
            this.closed = function (fn) {
                option.closed = fn;
                if (arguments.length > 1) {
                    option.closedParameters = [];
                    for (var i = 1; i < arguments.length; i++) {
                        option.closedParameters.push(arguments[i]);
                    }
                }
            }
            this.loaded = function (fn) {
                option.loaded = fn;
            }
        }
    }

    this.modal = function (href, data) {
        var option;
        if (data) {
            option = _modalViewArea.load(href, data, 'POST');
        } else {
            option = _modalViewArea.load(href, data, 'GET');
        }
        return new function () {
            this.closed = function (fn) {
                option.closed = fn;
                if (arguments.length > 1) {
                    option.closedParameters = [];
                    for (var i = 1; i < arguments.length; i++) {
                        option.closedParameters.push(arguments[i]);
                    }
                }
            }
            this.loaded = function (fn) {
                option.loaded = fn;
            }
            this.force = function (b) {
                option.force = b;
            }
        }
    }

    this.find = function (selector) {
        return $this.current().node.find(selector);
    }

    this.configSetting = function (fn) {
        if (typeof fn == "function") fn(_setting);
    }

    this.configText = function (fn) {
        if (typeof fn == "function") fn(_text);
    }

    this.option = function () {
        return _initialization_option;
    }

    this.initialled = function (fn) {
        if (typeof fn != "function") return;
        _initialledEvents.push(fn);
    }

    this.initialization = function (viewNodeSelector, defaultLink) {
        function option(main) {
            this.viewNode = function (nodeSelector) {
                if (!nodeSelector) return main.getElement();
                main.setElement(nodeSelector);
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
            this.loadTimeout = function (fn) {
                _fnOnLoadTimeout = fn;
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
                _globalViewEvent.load.add(fn);
                return this;
            }
            this.viewActived = function (fn) {
                _globalViewEvent.active.add(fn);
                return this;
            }
            this.viewInactived = function (fn) {
                _globalViewEvent.inactive.add(fn);
                return this;
            }
            this.viewUnloaded = function (fn) {
                _globalViewEvent.unload.add(fn);
                return this;
            }
            this.visibleLoadingCompleted = function (fn) {
                if (typeof (fn) != "function") throw new Error("fn is not a function");
                _visibleLoadingCompleted.push(fn);
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
                if (!_isDealLinkEmptyTarget) {
                    return;
                }
                e.preventDefault();
                _dealHrefTarget._link(href);
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
        _mainViewArea = new mainViewArea(viewNodeSelector, defaultLink);
        _openViewArea = new openViewArea();
        _modalViewArea = new modalViewArea();
        _activeViewAreaManager.push(_mainViewArea);
        _initialization_option = new option(_mainViewArea);
        for (var i in _initialledEvents) {
            _initialledEvents[i]();
        }
        return _initialization_option;
    }

    this.init = function (viewNodeSelector, defaultLink, hideFirstLoading) {
        var result = $this.initialization(viewNodeSelector, defaultLink);
        if (hideFirstLoading) _hideMainViewFirstLoading = true;
        $this.linker._init(function (link) {
            _mainViewArea.load(link, $this.linker.callChangeCompleted);
        });
        return result;
    }

    this.setup = function (mainViewSelector, defaultLink, hideFirstLoading) {
        if (_canSetup) {
            _canSetup = false;
        } else {
            throw new Error("Has been setup");
        }
        var result = $this.initialization(mainViewSelector, defaultLink);
        $(function () {
            if (hideFirstLoading) _hideMainViewFirstLoading = true;
            $this.linker._init(function (link) {
                _mainViewArea.load(link, $this.linker.callChangeCompleted);
            });
        });
        return result;
    }

    this.setupLayout = function (layoutSelector, layoutLink, mainViewSelector, defaultLink) {
        if (_canSetup) {
            _canSetup = false;
        } else {
            throw new Error("Has been setup");
        }
        var result = $this.initialization(mainViewSelector, defaultLink);
        $(function () {
            $.ajax({
                mimeType: 'text/html; charset=' + _setting.server_charset,
                url: layoutLink,
                type: 'GET',
                timeout: _setting.timeover
            }).done(function (data, textStatus, jqXHR) {
                var json = jqXHR.getResponseHeader("X-Responded-JSON"),
                    responded;

                if (json) {
                    responded = JSON.parse(json);
                    if (responded.status == 401) {
                        if (!_fnOnUnauthorized(layoutLink, responded.headers.location)) {
                            if (responded.headers && responded.headers.location) {
                                document.location = responded.headers.location;
                            }
                        }
                        return;
                    }
                }

                if (isHtmlDocument(data)) {
                    alert("You try to load wrong content: " + layoutLink);
                    return;
                }
                $(layoutSelector).html(data).children().unwrap();
                _hideMainViewFirstLoading = true;
                $this.linker._init(function (link) {
                    _mainViewArea.load(link, $this.linker.callChangeCompleted);
                });
            }).fail(function (jqXHR, textStatus, errorThrown) {
                var onLoadTimeoutResult;
                if (jqXHR.status == 401) {
                    _fnOnUnauthorized(layoutLink);
                } else if (jqXHR.status == 0 && textStatus == "timeout") {
                    onLoadTimeoutResult = _fnOnLoadTimeout();
                    if (onLoadTimeoutResult) {
                        $(layoutSelector).html(onLoadTimeoutResult).children().unwrap();
                        return;
                    }
                }
                $this.dialog.alert(_text.load_layout_error, errorThrown).ok(function () {
                    document.location.reload();
                });
            });
        });
        return result;
    }

    window.$app = {
        configSetting: $this.configSetting,
        configText: $this.configText,
        alert: $this.dialog.alert,
        confirm: $this.dialog.confirm,
        message: $this.dialog.message,
        loading: $this.loadingTip.show,
        loadScript: $this.scriptLoader.load,
        href: $this.linker.createHref,
        hash: $this.linker.hash,
        link: $this.linker.link,
        add: $this.linker.add,
        same: $this.linker.same,
        open: $this.open,
        modal: $this.modal,
        event: $this.event,
        reload: $this.viewReload,
        close: $this.viewClose,
        current: $this.current,
        find: $this.find,
        option: $this.option,
        init: $this.init,
        setup: $this.setup,
        setupLayout: $this.setupLayout
    };
})();