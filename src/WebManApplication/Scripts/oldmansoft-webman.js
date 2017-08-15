/*
* v0.9.65
* Copyright 2016 Oldmansoft, Inc; http://www.apache.org/licenses/LICENSE-2.0
*/
if (!window.oldmansoft) window.oldmansoft = {};
window.oldmansoft.webman = new (function () {
    var $this = this,
        menu,
        text,
        dealMethod,
        linkBehave;

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
                if ($(this).parent().hasClass("expand")) {
                    $(this).parent().removeClass("expand");
                    $(this).find(".arrow").removeClass("fa-minus-circle").addClass("fa-plus-circle");
                } else {
                    $(this).parent().addClass("expand");
                    $(this).find(".arrow").removeClass("fa-plus-circle").addClass("fa-minus-circle");
                }
                resetMainHeight();
            });
            if ($(this).children("ul").length > 0) {
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
                        text = store[j].node.children("span").text();
                        icon = store[j].node.children("i").clone();
                        break;
                    }
                }

                currentLinks = links.slice(0, i + 1);
                currentLinks.splice(0, 0, "");
                a = $("<a></a>");
                if (i < links.length - 1) {
                    a.attr("href", currentLinks.join("#"));
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

    function dealSubmitResultAction(data, method) {
        function refresh(isNewContent) {
            var operator;
            if (data.NewData) {
                operator = $(".main-view").last().data("operator");
                if (operator) {
                    operator.draw(false);
                } else if (!isNewContent) {
                    $app.reload();
                }
            }
        }

        function action() {
            var loading;
            if (!data.NewData && data.Path != null) {
                if (data.Behave == linkBehave.link) {
                    $app.sameHash(data.Path);
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
        }

        if (data.CloseOpen && method == dealMethod.form) {
            $app.close(null, refresh);
            action();
        } else {
            refresh(false);
            action();
        }
    }

    function dealSubmitResult(data, method) {
        if (data.Message) {
            $app.alert(data.Message, data.Success ? text.success : text.tips).ok(function () {
                dealSubmitResultAction(data, method);
            });
        } else {
            dealSubmitResultAction(data, method);
        }
    }

    function getDataTableSelectedIds(a) {
        var ids = [];
        a.parent().next().find("tbody tr td:first-child input[type='checkbox']").each(function () {
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
                dealSubmitResult(data, dealMethod.form);
            }).fail(function (error) {
                loading.hide();
                $app.alert($(error.responseText).eq(1).text(), error.statusText);
            });
        } else if (target == "_open") {
            $app.open(action, form.serialize());
        } else if (!target) {
            loading = $app.loading();
            form.ajaxSubmit().data("jqxhr").done(function (data) {
                loading.hide();
                $app.current().view.replace(action, data);
            }).fail(function (error) {
                loading.hide();
                $app.alert($(error.responseText).eq(1).text(), error.statusText);
            });
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
        if (text.hasClass("mark")) {
            return false;
        }
        text.addClass("mark");
        text.wrap("<del></del>");
        container.find(".del-file-input").val("1");
        return true;
    }

    function unmarkFileDelete(container) {
        var text = container.find(".icon-fa-text");
        if (!text.hasClass("mark")) {
            return false;
        }
        text.removeClass("mark");
        text.unwrap("<del></del>");
        container.find(".del-file-input").val("0");
        return true;
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
        function computeElementWidth(item) {
            var width = 0;
            item.children().each(function () {
                width += $(this).outerWidth(true)
            });
            return width;
        }
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
        });
    }

    this.init = function (main, defaultLink) {
        oldmansoft.webapp.configTarget(function (target) {
            target["_call"] = function (href) {
                var loading = $app.loading();
                $.get(href).done(function (data) {
                    dealSubmitResult(data, dealMethod.call);
                }).fail(dealAjaxError).always(function () { loading.hide(); });
            }
        });
        menu = new define_menu();
        $app.init(main, defaultLink).viewActived(function (view) {
            if (view.name == "open") return;
            menu.active(oldmansoft.webapp.hashes(), view, defaultLink);
        }).replacePCScrollBar(true);

        resetMainHeight();
        $(window).on("resize", resetMainHeight);
        $(".webman-bar").on("click", function () {
            var body = $("body"), className = "mini-nav";
            if (body.hasClass(className)) {
                body.removeClass(className);
            } else {
                body.addClass(className);
            }
        });
        $(".side-menu>li>a").on("click", function (e) {
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
        });
        $(document).on("click", ".webman-datatables-checkbox", function () {
            $(this).parents("table.dataTable").find("input[type='checkbox']").prop("checked", $(this).prop("checked"));
        });
        $(document).on("click", ".dataTable-action a", function (e) {
            var other_nothing = 0,
                other_supportParameter = 1,
                other_needSelected = 2,
                action,
                selectedIds = getDataTableSelectedIds($(this)),
                node = $(e.target).parents(".webman-body").find("table.dataTable");
            action = node.data("table-actions")[Number($(this).attr("data-index"))];

            function execute() {
                var loading;
                if (action.behave == linkBehave.open) {
                    if ((action.other & other_supportParameter) == other_supportParameter) {
                        $app.open(action.path, { SelectedId: selectedIds });
                    } else {
                        $app.open(action.path);
                    }
                } else if (action.behave == linkBehave.link) {
                    if ((action.other & other_supportParameter) == other_nothing || selectedIds.length == 0) {
                        $app.addHash(action.path);
                    } else {
                        for (var i = 0; i < selectedIds.length; i++) {
                            selectedIds[i] = encodeURIComponent(selectedIds[i]);
                        }
                        $app.addHash(bindUrlParamter(action.path, "SelectedId=" + selectedIds.join("&SelectedId=")));
                    }
                } else if (action.behave == linkBehave.call) {
                    loading = $app.loading();
                    if ((action.other & other_supportParameter) == other_supportParameter) {
                        $.post(action.path, {
                            SelectedId: selectedIds
                        }).done(function (data) {
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
                        for (var i = 0; i < selectedIds.length; i++) {
                            selectedIds[i] = encodeURIComponent(selectedIds[i]);
                        }
                        document.location = bindUrlParamter(action.path, "SelectedId=" + selectedIds.join("&SelectedId="));
                    }
                } else if (action.behave == linkBehave.blank) {
                    if ((action.other & other_supportParameter) == other_nothing || selectedIds.length == 0) {
                        window.open(action.path);
                    } else {
                        for (var i = 0; i < selectedIds.length; i++) {
                            selectedIds[i] = encodeURIComponent(selectedIds[i]);
                        }
                        window.open(bindUrlParamter(action.path, "SelectedId=" + selectedIds.join("&SelectedId=")));
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
            node.parents(".main-view").data("operator", node.data("datatable"));
        });
        $(document).on("click", ".dataTable-item-action a", function (e) {
            var action,
                itemId = getDataTableItemId($(this)),
                node = $(e.target).parents("table.dataTable");
            action = node.data("item-actions")[Number($(this).attr("data-index"))];

            function execute() {
                var loading;
                if (action.behave == linkBehave.open) {
                    $app.open(action.path, { SelectedId: itemId });
                } else if (action.behave == linkBehave.link) {
                    $app.addHash(bindUrlParamter(action.path, "SelectedId=" + itemId));
                } else if (action.behave == linkBehave.call) {
                    loading = $app.loading();
                    $.post(action.path, {
                        SelectedId: itemId
                    }).done(function (data) {
                        dealSubmitResult(data, dealMethod.call);
                    }).fail(dealAjaxError).always(function () { loading.hide(); });
                } else if (action.behave == linkBehave.self) {
                    document.location = bindUrlParamter(action.path, "SelectedId=" + itemId);
                } else if (action.behave == linkBehave.blank) {
                    window.open(bindUrlParamter(action.path, "SelectedId=" + itemId));
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
            node.parents(".main-view").data("operator", node.data("datatable"));
        });
        $(document).on("click", ".input-group-addon.del-file", function () {
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
        });
        $(document).on("click", ".input-group-addon.del-files", function () {
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
        });
        $(document).on("click", ".input-group-addon.del-input-files", function () {
            var container = $(this).parent(),
                group = container.parent();
            container.remove();
            group.find(".template-mulit-file-input").eq(0).trigger("change");
        });
        $(document).on("click", ".input-group .virtual-file-input", function () {
            $(this).prev().click();
        });
        $(document).on("change", ".input-group .single-file-input", function () {
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
        });
        $(document).on("click", ".input-group .virtual-mulit-file-input", function () {
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
        });
        $(document).on("submit", "form:not(.bv-form)", function (e) {
            if (!submitForm($(this))) {
                e.preventDefault();
            }
        });
        $(".webman-main-panel header form i.fa-search").on("click", function () {
            submitForm($(this).parents("form"));
        });
        $(document).on("click", ".container-remove", function () {
            var caller = $(this).parent();
            caller.fadeOut(function () {
                caller.remove();
            });
        });
        $(document).on("click", ".container-parent-remove", function () {
            var caller = $(this).parent().parent();
            caller.fadeOut(function () {
                var input = caller.parent().children(".input");
                caller.remove();
                input.trigger("input");
            });
        });
    }

    window.$man = {
        init: $this.init,
        configText: $this.configText
    }
})();