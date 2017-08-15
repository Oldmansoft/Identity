// jQuery Form Plugin 4.2.1
(function ($) {
    /**
    * Feature detection
    */
    var feature = {};

    feature.fileapi = $('<input type="file">').get(0).files !== undefined;
    feature.formdata = (typeof window.FormData !== 'undefined');
    $.fn.formToArray = function (semantic, elements, filtering) {
        var a = [];

        if (this.length === 0) {
            return a;
        }

        var form = this[0];
        var formId = this.attr('id');
        var els = (semantic || typeof form.elements === 'undefined') ? form.getElementsByTagName('*') : form.elements;
        var els2;

        if (els) {
            els = $.makeArray(els); // convert to standard array
        }

        // #386; account for inputs outside the form which use the 'form' attribute
        // FinesseRus: in non-IE browsers outside fields are already included in form.elements.
        if (formId && (semantic || /(Edge|Trident)\//.test(navigator.userAgent))) {
            els2 = $(':input[form="' + formId + '"]').get(); // hat tip @thet
            if (els2.length) {
                els = (els || []).concat(els2);
            }
        }

        if (!els || !els.length) {
            return a;
        }

        if ($.isFunction(filtering)) {
            els = $.map(els, filtering);
        }

        var i, j, n, v, el, max, jmax;

        for (i = 0, max = els.length; i < max; i++) {
            el = els[i];
            n = el.name;
            if (!n || el.disabled) {
                continue;
            }
            // Oldman: ignore temporary
            if ($(el).attr("data-temporary") == "temporary") {
                continue;
            }

            if (semantic && form.clk && el.type === 'image') {
                // handle image inputs on the fly when semantic == true
                if (form.clk === el) {
                    a.push({ name: n, value: $(el).val(), type: el.type });
                    a.push({ name: n + '.x', value: form.clk_x }, { name: n + '.y', value: form.clk_y });
                }
                continue;
            }

            v = $.fieldValue(el, true);
            if (v && v.constructor === Array) {
                if (elements) {
                    elements.push(el);
                }
                for (j = 0, jmax = v.length; j < jmax; j++) {
                    a.push({ name: n, value: v[j] });
                }

            } else if (feature.fileapi && el.type === 'file') {
                if (elements) {
                    elements.push(el);
                }

                var files = el.files;

                if (files.length) {
                    for (j = 0; j < files.length; j++) {
                        a.push({ name: n, value: files[j], type: el.type });
                    }
                } else {
                    // #180
                    a.push({ name: n, value: '', type: el.type });
                }

            } else if (v !== null && typeof v !== 'undefined') {
                if (elements) {
                    elements.push(el);
                }
                a.push({ name: n, value: v, type: el.type, required: el.required });
            }
        }

        if (!semantic && form.clk) {
            // input type=='image' are not found in elements array! handle it here
            var $input = $(form.clk), input = $input[0];

            n = input.name;

            if (n && !input.disabled && input.type === 'image') {
                a.push({ name: n, value: $input.val() });
                a.push({ name: n + '.x', value: form.clk_x }, { name: n + '.y', value: form.clk_y });
            }
        }

        return a;
    };
}(window.jQuery));

// BootstrapValidator v0.5.3
(function ($) {
    function findTargetField($field) {
        var form = $field.parentsUntil("body", "form"),
            tempFor = $field.attr("data-temporary-for");
        if (!tempFor) return $field;

        return form.find("input[name=" + tempFor + "]").filter(function (index) {
            return !$(this).attr("data-temporary-for");
        });
    }

    $.fn.bootstrapValidator.validators.regexp = {
        html5Attributes: {
            message: 'message',
            regexp: 'regexp'
        },

        enableByHtml5: function ($field) {
            var pattern = $field.attr('pattern');
            if (pattern) {
                return {
                    regexp: pattern
                };
            }

            return false;
        },

        /**
         * Check if the element value matches given regular expression
         *
         * @param {BootstrapValidator} validator The validator plugin instance
         * @param {jQuery} $field Field element
         * @param {Object} options Consists of the following key:
         * - regexp: The regular expression you need to check
         * @returns {Boolean}
         */
        validate: function (validator, $field, options) {
            var values = [],
                fields = findTargetField($field),
                field,
                files,
                value,
                regexp,
                i,
                j;
            for (i = 0; i < fields.length; i++) {
                field = fields.eq(i);
                if (field.attr('type') == "file") {
                    files = field.get(0).files;
                    for (j = 0; j < files.length; j++) {
                        values.push(files[j].name);
                    }
                } else {
                    value = field.val();
                    if (value === '') {
                        return true;
                    }
                    values.push(value);
                }
            }

            regexp = ('string' === typeof options.regexp) ? new RegExp(options.regexp) : options.regexp;
            for (i = 0; i < values.length; i++) {
                if (!regexp.test(values[i])) {
                    return false;
                }
            }
            return true;
        }
    };

    $.fn.bootstrapValidator.validators.notEmpty = {
        enableByHtml5: function ($field) {
            var required = $field.attr('required') + '';
            return ('required' === required || 'true' === required);
        },

        /**
         * Check if input value is empty or not
         *
         * @param {BootstrapValidator} validator The validator plugin instance
         * @param {jQuery} $field Field element
         * @param {Object} options
         * @returns {Boolean}
         */
        validate: function (validator, $field, options) {
            var type = $field.attr('type'),
                fields,
                i;
            if ('radio' === type || 'checkbox' === type) {
                return validator
                            .getFieldElements($field.attr('data-bv-field'))
                            .filter(':checked')
                            .length > 0;
            }

            if ('number' === type && $field.get(0).validity && $field.get(0).validity.badInput === true) {
                return true;
            }

            if ('file' === type) {
                if ($field.hasClass("template-mulit-file-input")) {
                    var delInputs = $field.parent().parent().find(".del-file-input");
                    for (i = 0; i < delInputs.length; i++) {
                        if ($.trim(delInputs.eq(i).val()) === '0') return true;
                    }

                    fields = findTargetField($field);
                    for (i = 0; i < fields.length; i++) {
                        if ($.trim(fields.eq(i).val()) !== '') return true;
                    }
                    return false;
                } else if ($field.hasClass("single-file-input")) {
                    if ($.trim($field.val()) === '') {
                        if ($field.parent().find(".del-file").length == 0 || $field.parent().find(".del-file").hasClass("on")) {
                            return false;
                        }
                    }
                    return true;
                }
            }

            //.tagsinput input 
            if ($field.hasClass("input")) {
                fields = findTargetField($field);
                for (i = 0; i < fields.length; i++) {
                    if ($.trim(fields.eq(i).val()) !== '') return true;
                }
                return false;
            }

            return $.trim($field.val()) !== '';
        }
    };
}(window.jQuery));