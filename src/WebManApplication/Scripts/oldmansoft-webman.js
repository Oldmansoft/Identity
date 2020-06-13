/*
* v0.27.111
* Copyright 2016 Oldmansoft, Inc; http://www.apache.org/licenses/LICENSE-2.0
*/
(function ($) {
    $.fn.bootstrapValidator.validators.listCount = {
        validate: function (validator, $field, options) {
            var type = $field.attr('type'),
                fields,
                i,
                delInputs,
                count = 0;
            if ('file' === type) {
                if ($field.hasClass("template-mulit-file-input")) {
                    delInputs = $field.parent().parent().find(".del-file-input");
                    for (i = 0; i < delInputs.length; i++) {
                        if ($.trim(delInputs.eq(i).val()) === '0') count++;
                    }

                    fields = findTemporaryTargetField($field);
                    for (i = 0; i < fields.length; i++) {
                        if ($.trim(fields.eq(i).val()) !== '') count += fields.get(i).files.length;
                    }
                }
            } else if ($field.hasClass("input") && $field.parent().hasClass("tagsinput")) {
                count = findTemporaryTargetField($field).length;
            } else if ('checkbox' === type) {
                fields = $("input[name=" + $field.attr("name") + "]");
                for (i = 0; i < fields.length; i++) {
                    if (fields.eq(i).prop("checked")) count++;
                }
            } else if ($field.is("select")) {
                fields = $field.children();
                for (var i = 0; i < fields.length; i++) {
                    if (fields.eq(i).prop("selected")) count++;
                }
            } else {
                count = $("input[name=" + $field.attr("name") + "]").length;
            }
            if (count == 0) return true;
            if (options.fixed) return count == options.fixed;
            if (options.min && options.max) {
                if (options.inclusive) {
                    return count >= options.min && count <= options.max;
                } else {
                    return count > options.min && count < options.max;
                }
            }
            if (options.min) {
                if (options.inclusive) {
                    return count >= options.min;
                } else {
                    return count > options.min;
                }
            }
            if (options.max) {
                if (options.inclusive) {
                    return count <= options.max;
                } else {
                    return count < options.max;
                }
            }
            return true;
        }
    };

    $.fn.bootstrapValidator.validators.fileLimitContentLength = {
        validate: function (validator, $field, options) {
            var type = $field.attr('type'),
                fields,
                i,
                j,
                count = 0;
            if ('file' === type) {
                if ($field.hasClass("template-mulit-file-input")) {
                    fields = findTemporaryTargetField($field);
                    for (i = 0; i < fields.length; i++) {
                        if ($.trim(fields.eq(i).val()) !== '') {
                            for (j = 0; j < fields.get(i).files.length; j++) {
                                if (fields.get(i).files[j].size > options.length) return false;
                            }
                        }
                    }
                }
                return true;
            }

            return false;
        }
    };
}(window.jQuery));

if (!window.oldmansoft) window.oldmansoft = {};
window.oldmansoft.webman = new (function () {
    var $this = this,
        menu,
        text,
        dealMethod,
        linkBehave,
        badgeRefreshParentExecuteCount = 0;

    text = {
        dataTable: {
            info: "Showing _START_ to _END_ of _TOTAL_ entries",
            infoEmpty: "Empty",
            emptyTable: "No data available in table",
            processing: "Processing...",
            paginate: {
                first: "First",
                last: "Last",
                next: "Next",
                previous: "Previous"
            }
        },
        error: "Error",
        success: "Success",
        tips: "Tips",
        denied: "Permission denied",
        please_select_item: "Please select item.",
    }

    dealMethod = {
        form: 0,
        call: 1
    }

    linkBehave = {
        link: 0,
        open: 1,
        call: 2,
        self: 3,
        blank: 4,
        script: 5
    }

    function bindUrlParamter(path, parameter) {
        if (!parameter) return path;
        if (!path) return "?" + parameter;
        if (path.indexOf("?") > -1) return path + "&" + parameter;
        return path + "?" + parameter;
    }

    function define_menu() {
        var store = [];
        $(".webman-left-panel ul.side-menu li").each(function () {
            var item = { level: 0, node: $(this).children("a") };
            store.push(item);
            item.node.click(function () {
                var arrow = $(this).find(".arrow"),
                    li = $(this).parent();
                if (arrow.length == 0) return;
                if (li.hasClass("expand")) {
                    li.removeClass("expand");
                    arrow.removeClass("fa-minus-circle").addClass("fa-plus-circle");
                } else {
                    li.addClass("expand");
                    arrow.removeClass("fa-plus-circle").addClass("fa-minus-circle");
                }
                resetMainHeight();
            });
            if ($(this).find("ul>li>a[href]").length > 0) {
                item.node.append($("<i></i>").addClass("arrow").addClass("fa").addClass("fa-plus-circle"));
            } else if (item.node.attr("href") == undefined) {
                $(this).hide();
            }
        });

        function setBreadcrumb(links, view, defaultLink) {
            var i, j, link, node, text, currentLinks, a, icon;
            node = $(".webman-breadcrumb ul");
            node.empty();
            for (i = 0; i < links.length; i++) {
                link = links[i];
                if (link == "") link = defaultLink;
                text = null;
                icon = null;
                for (j = 0; j < store.length; j++) {
                    if (link == store[j].node.attr("href")) {
                        text = store[j].node.children("span").first().text();
                        icon = store[j].node.children("i").clone();
                        break;
                    }
                }

                currentLinks = links.slice(0, i + 1);
                a = $("<a></a>");
                if (i < links.length - 1) {
                    a.attr("href", "#" + oldmansoft.webapp.parser(currentLinks).getContent());
                }
                if (!text) {
                    if (view.node.data("link") == links[i]) {
                        text = view.node.find(".webman-panel header h2").text();
                        icon = view.node.find(".webman-panel header>i");
                    } else {
                        text = "..";
                    }
                }
                if (i < links.length - 1 && icon) a.append(icon);
                a.append($("<span></span>").text(text));
                node.append($("<li></li>").append(a));
            }
        }

        function findNode(link, defaultLink) {
            var i;
            if (link == "") link = defaultLink;
            for (i = 0; i < store.length; i++) {
                if (link == store[i].node.attr("href")) {
                    return store[i].node.parent();
                }
            }
        }

        this.active = function (links, view, defaultLink) {
            var node, result;
            setBreadcrumb(links, view, defaultLink);
            $(".webman-left-panel ul.side-menu li").removeClass("active");
            node = findNode(links[links.length - 1], defaultLink);
            if (node) {
                node.addClass("active").parentsUntil(".side-menu", "li").addClass("expand").children("a").children(".arrow").removeClass("fa-plus-circle").addClass("fa-minus-circle");
                result = true;
            } else {
                result = false;
            }
            resetMainHeight();
            return result;
        }
    }

    function dealSubmitResultAction(data, method, form) {
        function refresh(isNewContent) {
            var operator;
            if (!data.NewData) {
                if (method == dealMethod.form && form) form.bootstrapValidator("disableSubmitButtons", false);
                action();
                return;
            }

            operator = $app.current().node.data("operator");
            if (operator) {
                operator.draw(false);
            } else if (!isNewContent) {
                $app.reload();
            }
            action();
        }

        function action() {
            var loading;
            if (data.NewData || data.Path == null) return;

            if (data.Behave == linkBehave.link) {
                $app.same(data.Path);
            } else if (data.Behave == linkBehave.open) {
                $app.open(data.Path);
            } else if (data.Behave == linkBehave.call) {
                loading = $app.loading();
                $.get(data.Path).done(function (data) {
                    dealSubmitResult(data, dealMethod.call);
                }).fail(dealAjaxError).always(function () { loading.hide(); });
            } else if (data.Behave == linkBehave.self) {
                document.location = data.Path;
            } else if (data.Behave == linkBehave.blank) {
                window.open(data.Path);
            }
        }

        if (data.CloseOpen && (!$app.current().node.hasClass("main-view") || (method == dealMethod.form && $app.current().view.getLinks().length > 1))) {
            $app.close().completed(refresh);
        } else {
            refresh(false);
        }
    }

    function dealSubmitResult(data, method, form) {
        if (data.Message) {
            $app.alert(data.Message, data.Success ? text.success : text.tips).ok(function () {
                dealSubmitResultAction(data, method, form);
            });
        } else {
            dealSubmitResultAction(data, method, form);
        }
    }

    function getDataTableFromActionLink(element) {
        var group = element.parents(".dataTable-action"),
            body = group.parents(".webman-body"),
            table = body.find("table.dataTable"),
            target = group.attr("data-target");
        if (target) {
            table = body.find("." + target);
        }
        return table;
    }

    function getDataTableSelectedIds(a) {
        var ids = [],
            table = getDataTableFromActionLink(a);

        table.find("tbody tr td:first-child input[type='checkbox']").each(function () {
            if ($(this).prop("checked")) {
                ids.push($(this).val());
            }
        });
        return ids;
    }

    function getDataTableItemId(a) {
        return a.parent().attr("data-id");
    }

    function dealAjaxError(jqXHR, textStatus, errorThrown) {
        if (jqXHR.status == 401) {
            $app.alert(text.denied, text.error);
        } else {
            $app.alert(errorThrown, text.error);
        }
    }

    function submitForm(form) {
        var loading,
            action = form.attr("action"),
            target = form.attr("target");

        if (target == "_call") {
            loading = $app.loading();
            form.ajaxSubmit().data("jqxhr").done(function (data) {
                loading.hide();
                dealSubmitResult(data, dealMethod.form, form);
            }).fail(function (error) {
                loading.hide();
                $app.alert($(error.responseText).eq(1).text(), error.statusText);
            });
        } else if (target == "_open") {
            $app.open(action, form.serialize());
        } else if (!target) {
            $app.link(action + "?" + form.serialize());
        } else {
            return true;
        }
        return false;
    }

    function resetMainHeight() {
        var leftHeight = $(".webman-left-panel").height(),
            windowHeight = $(window).height();
        $(".webman-main-panel").css("min-height", leftHeight > windowHeight ? leftHeight : windowHeight);
    }

    function markFileDelete(container) {
        var text = container.find(".icon-fa-text");
        if (text.hasClass("del-mark")) {
            return false;
        }
        text.addClass("del-mark");
        text.wrap("<del></del>");
        container.find(".del-file-input").val("1");
        return true;
    }

    function unmarkFileDelete(container) {
        var text = container.find(".icon-fa-text");
        if (!text.hasClass("del-mark")) {
            return false;
        }
        text.removeClass("del-mark");
        text.unwrap("<del></del>");
        container.find(".del-file-input").val("0");
        return true;
    }

    function computeElementWidth(item) {
        var width = 0;
        item.children().each(function () {
            width += $(this).outerWidth(true)
        });
        return width;
    }

    function fixedTableFirstAndLastColumnWidth(view, className) {
        var table = view.node.find("." + className);

        var maxWidth = 0;
        table.find("tbody tr td:first-child").each(function () {
            var width = computeElementWidth($(this));
            if (width > maxWidth) maxWidth = width;
        })
        if (maxWidth > 0) {
            table.find("thead tr th:first-child").width(maxWidth);
        }

        maxWidth = 0;
        table.find("tbody tr td:last-child").each(function () {
            var width = computeElementWidth($(this).children());
            if (width > maxWidth) maxWidth = width;
        })
        if (maxWidth > 0) {
            table.find("thead tr th:last-child").width(maxWidth);
        }
    }

    this.configText = function (fn) {
        if (typeof fn == "function") fn(text);
    }

    this.setLoginSubmit = function (loginForm, seedPath, accountInput, passwordInput) {
        $(accountInput).on("keypress", function () {
            $(this).parent().parent().removeClass("has-error");
        });
        $(accountInput).on("change", function () {
            $(this).parent().parent().removeClass("has-error");
        });
        $(passwordInput).on("keypress", function () {
            $(this).parent().parent().removeClass("has-error");
        });
        $(passwordInput).on("change", function () {
            $(this).parent().parent().removeClass("has-error");
        });
        $(loginForm).submit(function () {
            var loading,
                account,
                password,
                seedResponse,
                passwordHash,
                doubleHash;

            account = $.trim($(accountInput).val());
            password = $(passwordInput).val();

            if (account == "") {
                $(accountInput).parent().parent().addClass("has-error");
            }
            if (password == "") {
                $(passwordInput).parent().parent().addClass("has-error");
            }
            if (account == "" || password == "") {
                return false;
            }

            loading = $app.loading()
            seedResponse = $.ajax({ url: seedPath + "?" + new Date().getTime(), async: false });
            if (seedResponse.status != 200) {
                loading.hide();
                $app.alert(seedResponse.statusText);
                return false;
            }
            passwordHash = sha256(account.toLowerCase() + password);
            doubleHash = sha256(passwordHash.toUpperCase() + seedResponse.responseText);

            $.post($(this).attr("action"), {
                Account: account,
                Hash: doubleHash
            }).done(function (data) {
                loading.hide();
                data.NewData = false;
                data.CloseOpen = false;
                data.Behave = linkBehave.self;
                dealSubmitResult(data, dealMethod.form);
            }).fail(function (error) {
                loading.hide();
                $app.alert($(error.responseText).eq(1).text(), error.statusText);
            });

            return false;
        });
    }

    this.setFormValidate = function (view, className, fields) {
        if (!fields) return;
        var form = view.node.find("." + className);
        form.bootstrapValidator({
            fields: fields
        }).on('success.form.bv', function (e) {
            form.bootstrapValidator("disableSubmitButtons", true);
            if (!submitForm($(e.target))) {
                e.preventDefault();
            }
        });
    }

    this.setDataTableColumnCheckbox = function (data) {
        var input = $("<input type='checkbox'/>");
        input.val(data);
        return input.wrap('<div></div>').parent().html();
    }

    this.setDataTableColumnIndex = function (data, type, row, meta) {
        return meta.settings._iDisplayStart + meta.row + 1;
    }

    this.setDataTableColumnOperate = function (items) {
        return function (data, type, row, meta) {
            var div = $("<div></div>");
            div.addClass("dataTable-item-action");
            div.attr("data-id", data);
            for (var i = 0; i < items.length; i++) {
                if (items[i].hide && items[i].hide(row)) {
                    continue;
                }

                var a = $("<a></a>");
                a.text(items[i].text);
                a.attr("data-index", i);
                a.attr("data-path", items[i].path);
                a.attr("data-behave", items[i].behave);
                a.attr("data-tips", items[i].tips);

                if (items[i].disabled && items[i].disabled(row)) {
                    a.addClass("disabled");
                }
                div.append(a);
            }
            return div.wrap('<div></div>').parent().html();
        }
    }

    this.setDataTable = function (view, className, source, option) {
        var tableOption = {
            processing: true,
            serverSide: true,
            ajax: {
                url: source,
                type: 'POST'
            },
            columns: option.columns,
            retrieve: true,
            searching: false,
            lengthChange: false,
            autoWidth: false,
            ordering: false,
            language: text.dataTable,
            dom: "<'table-content'<'col-sm-6'f><'col-sm-6 text-right'l>>rt<'table-content'<'col-sm-6'i><'col-sm-6 text-right'p>>",
            drawCallback: function () {
                fixedTableFirstAndLastColumnWidth(view, className);
            }
        },
            node;
        if (option.size) {
            tableOption.displayLength = option.size;
        }
        if (option.createdRow) {
            tableOption.createdRow = option.createdRow;
        }
        node = view.node.find("." + className);
        node.data("datatable", node.DataTable(tableOption));
        node.data("table-actions", option.tableActions);
        node.data("item-actions", option.itemActions);
    }

    this.setStaticTable = function (view, className, option) {
        this.draw = function () {
            $app.reload();
        }
        fixedTableFirstAndLastColumnWidth(view, className);
        var node;
        node = view.node.find("." + className);
        node.data("datatable", this);
        node.data("table-actions", option.tableActions);
        node.data("item-actions", option.itemActions);
    }

    this.setTagsInput = function (view, selector) {
        var target = view.node.find(selector);

        target.on("click", function () {
            $(this).find(".input").focus();
        });

        function AddValue(caller) {
            var value,
                find = false,
                input,
                div,
                hidden,
                span;

            input = caller.find(".input");
            value = $.trim(input.val());
            if (value == "") return;
            
            caller.find("input[type=hidden]").each(function () {
                if ($(this).val() == value) {
                    find = true;
                }
            });
            if (!find) {
                input.val("");
                input.width("");
                div = $("<div></div>");
                hidden = $("<input type='hidden'/>");
                hidden.attr("name", input.attr("name"));
                hidden.val(value);
                hidden.appendTo(div);
                span = $("<span></span>");
                span.text(value);
                span.appendTo(div);
                span.append("<i class='fa fa-times container-parent-remove'></i>");

                input.before(div);
                input.trigger("input");
            }
        }

        target.on("blur", ".input", function (e) {
            AddValue($(e.delegateTarget));
        });

        target.on("keypress", ".input", function (e) {
            if (e.keyCode == 13) {
                AddValue($(e.delegateTarget));
                return false;
            }
            if (this.scrollWidth > $(this).parent().width()) return true;
            $(this).width(this.scrollWidth);
        });
    }

    function setBadge(container, text) {
        var node = container.children(".badge");
        if (node.length == 0) {
            container.append($("<span></span>").addClass("badge"));
            node = container.children(".badge");
        }
        if (text == " ") {
            node.html("&nbsp");
        } else {
            node.text(text);
        }
    }

    function badgeSetContent() {
        var hasValue = false;

        $(this).children(".branch").each(badgeSetContent);
        $(this).children("ul").children("li").find(".badge").each(function () {
            if ($(this).text() != "") {
                hasValue = true;
                return false;
            }
        });
        if (hasValue) {
            setBadge($(this).children("a"), " ");
        } else {
            setBadge($(this).children("a"), "");
        }
    }

    function badgeRefreshParent() {
        badgeRefreshParentExecuteCount--;
        if (badgeRefreshParentExecuteCount > 0) return;
        $(".side-menu").children(".branch").each(badgeSetContent);
        $(".dropdown").each(badgeSetContent);
    }

    this.badge = function (href, text) {
        var container = $(".badge-container[href='" + href + "']");
        if (container.length == 0) return;

        setBadge(container, text);
        badgeRefreshParentExecuteCount++;
        setTimeout(badgeRefreshParent, 1);
    }

    var tableScroller = new (function () {
        var x,
            table,
            disabled = false;
        function disabledSelectStart() {
            return false;
        }
        function mousemove(e) {
            table.scrollLeft(table.scrollLeft() + x - e.clientX);
            x = e.clientX;
        }
        function mouseup() {
            table.off("selectstart", disabledSelectStart);
            table.off("mousemove", mousemove);
            table.off("mouseup", mouseup);
            table.off("mouseleave ", mouseup);
        }
        function mousedown(e) {
            if (disabled) return;
            table = $(this);
            if (table.get(0).scrollWidth == table.width()) return;
            x = e.clientX;
            table.addClass("mouse-down");
            table.on("selectstart", disabledSelectStart);
            table.on("mousemove", mousemove);
            table.on("mouseup", mouseup);
            table.on("mouseleave ", mouseup);
        }
        this.init = function () {
            $(document).on("mousedown", ".dataTables_wrapper", mousedown);
            $(document).on("keydown", function (key) { if (key.keyCode == 17) disabled = true; });
            $(document).on("keyup", function (key) { if (key.keyCode == 17) disabled = false; });
        }
    })();

    this.search = new (function () {
        var form,
            input,
            born,
            values,
            onExecuted = false;
        function init() {
            if (form) {
                return;
            }

            form = $(".webman-main-panel>header>form");
            input = form.children("input");
            born = {
                action: form.attr("action"),
                target: form.attr("target"),
                name: input.attr("name"),
                placeholder: input.attr("placeholder"),
                hidden: form.hasClass("hidden")
            };
            values = [];
            values[born.action] = "";
        }

        this.on = function (option) {
            init();
            values[form.attr("action")] = input.val();
            input.val(values[option.action]);
            form.attr("action", option.action);
            if (option.target) form.attr("target", option.target);
            input.attr("name", option.name);
            input.attr("placeholder", option.placeholder);
            if (born.hidden) {
                form.removeClass("hidden");
            }
            onExecuted = true;
        }
        this.off = function () {
            init();
            if (onExecuted) {
                onExecuted = false;
                return;
            }
            if (born.hidden) {
                form.addClass("hidden");
            }
            values[form.attr("action")] = input.val();
            input.val(values[born.action]);
            form.attr("action", born.action);
            if (born.target) form.attr("target", born.target);
            input.attr("name", born.name);
            input.attr("placeholder", born.placeholder);
        }
    })();

    function web_bar_click() {
        var body = $("body"),
            className = "mini-nav";
        if (body.hasClass(className)) {
            body.removeClass(className);
        } else {
            body.addClass(className);
        }
    }

    function side_menu_li_a_click(e) {
        if ("ontouchmove" in document && $(window).width() <= 768) {
            var menu = $(".side-menu"),
                index = $(".side-menu>li>a").index($(this));
            if (menu.data("click") != index) {
                menu.data("click", index);
                return false;
            } else {
                $(document).click();
            }
        }
    }

    function webman_datatables_checkbox_click() {
        $(this).parents("table.dataTable").find("input[type='checkbox']").prop("checked", $(this).prop("checked"));
    }

    function dataTable_action_a_click(e) {
        var other_nothing = 0,
            other_supportParameter = 1,
            other_needSelected = 2,
            selectedIds = getDataTableSelectedIds($(this)),
            node = getDataTableFromActionLink($(e.target)),
            action = node.data("table-actions")[Number($(this).attr("data-index"))],
            parameter_name = node.attr("data-parameter");
        if (!parameter_name) parameter_name = "selectedId";

        function getJsonValue() {
            var result = {};
            result[parameter_name] = selectedIds;
            return result;
        }

        function getFormValue() {
            for (var i = 0; i < selectedIds.length; i++) {
                selectedIds[i] = encodeURIComponent(selectedIds[i]);
            }
            return parameter_name + "=" + selectedIds.join("&" + parameter_name + "=");
        }

        function execute() {
            var loading;
            if (action.behave == linkBehave.open) {
                if ((action.other & other_supportParameter) == other_supportParameter) {
                    $app.open(action.path, getJsonValue());
                } else {
                    $app.open(action.path);
                }
            } else if (action.behave == linkBehave.link) {
                if ((action.other & other_supportParameter) == other_nothing || selectedIds.length == 0) {
                    $app.add(action.path);
                } else {
                    $app.add(bindUrlParamter(action.path, getFormValue()));
                }
            } else if (action.behave == linkBehave.call) {
                loading = $app.loading();
                if ((action.other & other_supportParameter) == other_supportParameter) {
                    $.post(action.path, getJsonValue()).done(function (data) {
                        dealSubmitResult(data, dealMethod.call);
                    }).fail(dealAjaxError).always(function () { loading.hide(); });
                } else {
                    $.get(action.path).done(function (data) {
                        dealSubmitResult(data, dealMethod.call);
                    }).fail(dealAjaxError).always(function () { loading.hide(); });
                }
            } else if (action.behave == linkBehave.self) {
                if ((action.other & other_supportParameter) == other_nothing || selectedIds.length == 0) {
                    document.location = action.path;
                } else {
                    document.location = bindUrlParamter(action.path, getFormValue());
                }
            } else if (action.behave == linkBehave.blank) {
                if ((action.other & other_supportParameter) == other_nothing || selectedIds.length == 0) {
                    window.open(action.path);
                } else {
                    window.open(bindUrlParamter(action.path, getFormValue()));
                }
            } else if (action.behave == linkBehave.script) {
                action.script(selectedIds);
            }
        }

        if ((action.other & other_needSelected) == other_needSelected && selectedIds.length == 0) {
            $app.alert(text.please_select_item);
            return;
        }
        if (action.tips) {
            $app.confirm(action.tips).yes(execute);
        } else {
            execute();
        }
        $app.current().node.data("operator", node.data("datatable"));
    }

    function dataTable_item_action_a_click(e) {
        var itemId = getDataTableItemId($(this)),
            node = $(e.target).parents("table.dataTable"),
            action = node.data("item-actions")[Number($(this).attr("data-index"))],
            parameter_name = node.attr("data-parameter");
        if (!parameter_name) parameter_name = "selectedId";

        function getJsonValue() {
            var result = {};
            result[parameter_name] = itemId;
            return result;
        }

        function execute() {
            var loading;
            if (action.behave == linkBehave.open) {
                $app.open(action.path, getJsonValue());
            } else if (action.behave == linkBehave.link) {
                $app.add(bindUrlParamter(action.path, parameter_name + "=" + itemId));
            } else if (action.behave == linkBehave.call) {
                loading = $app.loading();
                $.post(action.path, getJsonValue()).done(function (data) {
                    dealSubmitResult(data, dealMethod.call);
                }).fail(dealAjaxError).always(function () { loading.hide(); });
            } else if (action.behave == linkBehave.self) {
                document.location = bindUrlParamter(action.path, parameter_name + "=" + itemId);
            } else if (action.behave == linkBehave.blank) {
                window.open(bindUrlParamter(action.path, parameter_name + "=" + itemId));
            } else if (action.behave == linkBehave.script) {
                action.script(itemId);
            }
        }

        if ($(this).hasClass("disabled")) return;
        if (action.tips) {
            $app.confirm(action.tips).yes(execute);
        } else {
            execute();
        }
        $app.current().node.data("operator", node.data("datatable"));
    }

    function input_group_addon_del_file_click() {
        var container = $(this).parent();
        if ($(this).hasClass("on")) {
            if (container.find("input[type=file]").val() == "") {
                unmarkFileDelete(container);
            }
            $(this).removeClass("on");
        } else {
            markFileDelete(container);
            $(this).addClass("on");
        }
        container.find(".single-file-input").trigger("change");
    }

    function input_group_addon_del_files_click() {
        var container = $(this).parent(),
            group = container.parent();
        if ($(this).hasClass("on")) {
            unmarkFileDelete(container);
            $(this).removeClass("on");
        } else {
            markFileDelete(container);
            $(this).addClass("on");
        }
        group.find(".template-mulit-file-input").eq(0).trigger("change");
    }

    function input_group_addon_del_input_files_click() {
        var container = $(this).parent(),
            group = container.parent();
        container.remove();
        group.find(".template-mulit-file-input").eq(0).trigger("change");
    }

    function input_group_single_file_input_change() {
        var container = $(this).parent(),
            files;

        if (!$(this).data("next-text")) $(this).data("next-text", $(this).next().text());
        files = $(this).get(0).files;
        if (files != null && files.length > 0) {
            $(this).next().text(files[0].name);
        } else {
            $(this).next().text($(this).data("next-text"));
        }

        if (container.find(".del-file-input").length == 0) return;
        if ($(this).val() == "") {
            if (!container.find(".del-file").hasClass("on")) {
                unmarkFileDelete(container);
            }
        } else {
            markFileDelete(container);
        }
    }

    function input_group_virtual_mulit_file_input_click() {
        var template = $(this).prev(),
            input = $("<input type='file' multiple='multiple' />"),
            caller = $(this);
        if ($(this).attr("readonly") == "readonly" || $(this).attr("disabled") == "disabled") {
            return;
        }
        if (template.hasClass("mulit-file-input")) {
            template.click();
            return;
        }

        input.attr("name", template.attr("name"));
        input.attr("accept", template.attr("accept"));
        input.attr("class", "mulit-file-input");
        input.attr("data-bv-field", template.attr("data-bv-field"));
        input.attr("data-temporary", "temporary");
        input.on("change", function () {
            caller.prev().prev().trigger("change");

            var container = $(this).parent(),
                groups = $(this).parentsUntil("form", ".mulit-file-group"),
                group,
                control,
                files,
                ul,
                i,
                li,
                span,
                fa;

            group = $("<div></div>");
            group.addClass("input-group");
            group.addClass("control-line");

            $(this).removeAttr("data-temporary");
            group.append($(this));

            control = $("<div></div>");
            control.addClass("form-control");
            control.css("height", "initial");
            control.appendTo(group);

            files = $(this).get(0).files;
            ul = $("<ul><ul>");
            ul.addClass("control-value");
            ul.appendTo(control);
            for (i = 0; i < files.length; i++) {
                li = $("<li></li>");
                li.appendTo(ul);
                li.text(files[i].name);
            }

            span = $("<span></span>");
            span.addClass("input-group-addon");
            span.addClass("del-input-files");
            span.appendTo(group);

            fa = $("<i></i>");
            fa.addClass("fa");
            fa.addClass("fa-times");
            fa.appendTo(span);

            group.appendTo(groups);
        });

        template.after(input);
        input.click();
    }

    function form_not_bv_form_submit(e) {
        var form = $(this);
        if (form.attr("action") == undefined) {
            form.attr("action", $app.link());
        }
        if (form.attr("action") == document.location.pathname) {
            return;
        }
        if (form.attr("onsubmit") != undefined) {
            return;
        }
        if (!submitForm(form)) {
            e.preventDefault();
        }
    }

    function container_remove_click() {
        var caller = $(this).parent();
        caller.fadeOut(function () {
            caller.remove();
        });
    }

    function container_parent_remove_click() {
        var caller = $(this).parent().parent();
        caller.fadeOut(function () {
            var input = caller.parent().children(".input");
            caller.remove();
            input.trigger("input");
        });
    }

    function header_form_search_click() {
        var form = $(this).parents("form"),
            input = form.find("input[type=text]");
        if ($.trim(input.val()) == "")
        {
            input.get(0).focus();
            return;
        }
        submitForm(form);
    }

    this.init = function (main, defaultLink) {
        menu = new define_menu();
        oldmansoft.webapp.configTarget(function (target) {
            target["_call"] = function (href) {
                var loading = $app.loading();
                $.get(href).done(function (data) {
                    dealSubmitResult(data, dealMethod.call);
                }).fail(dealAjaxError).always(function () { loading.hide(); });
            }
        });
        $app.init(main, defaultLink).viewActived(function (view) {
            if (view.name == "open") return;
            menu.active(oldmansoft.webapp.hashes(), view, defaultLink);
            $this.search.off();
        }).replacePCScrollBar(true);

        resetMainHeight();
        $(window).on("resize", resetMainHeight);
        $(".webman-bar").on("click", web_bar_click);
        $(".side-menu>li>a").on("click", side_menu_li_a_click);
        $(document).on("click", ".webman-datatables-checkbox", webman_datatables_checkbox_click);
        $(document).on("click", ".dataTable-action a", dataTable_action_a_click);
        $(document).on("click", ".dataTable-item-action a", dataTable_item_action_a_click);
        $(document).on("click", ".input-group-addon.del-file", input_group_addon_del_file_click);
        $(document).on("click", ".input-group-addon.del-files", input_group_addon_del_files_click);
        $(document).on("click", ".input-group-addon.del-input-files", input_group_addon_del_input_files_click);
        $(document).on("click", ".input-group .virtual-file-input", function () { $(this).prev().click(); });
        $(document).on("change", ".input-group .single-file-input", input_group_single_file_input_change);
        $(document).on("click", ".input-group .virtual-mulit-file-input", input_group_virtual_mulit_file_input_click);
        $(document).on("submit", "form:not(.bv-form)", form_not_bv_form_submit);
        $(document).on("click", ".container-remove", container_remove_click);
        $(document).on("click", ".container-parent-remove", container_parent_remove_click);
        $(".webman-main-panel header form i.fa-search").on("click", header_form_search_click);
        tableScroller.init();
   }

    window.$man = {
        init: $this.init,
        configText: $this.configText,
        badge: $this.badge
    }
})();