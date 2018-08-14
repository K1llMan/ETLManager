$(function () {
    function runPump(config) {
        $.ajax('api/pumps/execute/' + config.id, {
            type: 'POST',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(config),
            success: function (response) {
                //...
            },
            error: function (jqXHR, exception) {
                console.log(exception);
            }
        });
    }

    function setValue(input, obj, param) {
        switch (input.attr('type')) {
            case "checkbox":
                obj[param] = input.is(":checked");
                break;
            case "radio":
                var checked = input.is(":checked");
                obj[param] = checked;
                // Update other params in radio group
                if (checked) {
                    var name = $(input).attr('name');
                    var inputs = $(input).parents('form').find('input:not(:checked)[name="' + name + '"]');
                    inputs.change();
                }
                break;
            default:
                obj[param] = input.val();
        }
    }

    function bindValue(input, obj, param) {
        switch (input.attr('type')) {
            case "checkbox":
            case "radio":
                input[0].checked = obj[param];
                break;
            default:
                input.val(obj[param]);
        }

        input.change(function () {
            setValue(input, obj, param);
        });        
    }

    function initParams(paramGroups) {
        var paramsPanel = $('#params-modal #params');
        paramsPanel.html('');

        if (Object.keys(paramGroups).length == 0)
            return;

        $.each(paramGroups, function (i, params) {
            var form = $('<form class="row"></form>');
            paramsPanel.append(form);

            $.each(Object.keys(params), function (j, key) {
                if (params[key].ui == undefined || params[key].ui == null)
                    return;

                var component = $(Templater.useTemplate(params[key].ui.type, [params[key].ui]));
                form.append(component);

                bindValue(component.find('input'), params[key], 'value');
            });            
        });
    }

    function updateModal(config) {
        var modal = $('#params-modal');
        modal.find("h4").html(config.desc.name);

        // Remove old data
        var stagesContainer = modal.find('#stages');
        stagesContainer.find('a').remove();

        stagesContainer.append($(Templater.useTemplate('common-stage-item')));
        var stagesList = $(Templater.useTemplate('stage-item',
            Object.keys(config.stages).map(function (key) {
                var stage = config.stages[key];
                stage.id = key;
                return stage;
            })));

        stagesContainer.append(stagesList);

        // Switching between params lists
        stagesContainer.find('a').each(function (i, a) {
            var stageId = $(a).attr('id');

            $(a).click(function () {
                stagesContainer.find('a').removeClass('active');
                $(a).addClass('active');
                initParams(stageId == 'common'
                    ? config.commonParams
                    : config.stages[stageId].params);
            });

            // Changing stage status
            var sw = $(a).find('input');
            if (sw.length == 0)
                return;

            bindValue(sw, config.stages[stageId], 'enabled');
        });

        var clcFunc = function() {
            runPump(config);
        }
        var runBtn = modal.find('#runPump');
        runBtn.click(clcFunc);

        // Remove run handler
        modal.modal({
            'onCloseEnd': function () {
                runBtn.off('click', clcFunc);
            }
        });

        modal.modal('open');
        stagesContainer.find('#common').click();
    }

    function updateRegistry() {
        var collapsible = $('.collapsible');
        // Clear old data
        collapsible.html('');

        // Groups by id
        var groups = [];
        $.each(pumpsRegistry, function (i, el) {
            if (groups.indexOf(el.desc.supplierCode) == -1)
                groups.push(el.desc.supplierCode);
        });

        var collapsibleItems = $(Templater.useTemplate('collapsible-item', groups.map(function (id) {
                return { "id": id, "name": id };
            })
        ));

        collapsible.append(collapsibleItems);

        $.each(pumpsRegistry, function (i, el) {
            var body = $(Templater.useTemplate('collapsible-body-item', [el]));

            el.desc.id = el.id;
            el.desc.version = el.version;

            body.prepend($(Templater.useTemplate('pump-desc', [el.desc])));
            var startBtn = body.find('#buttons #start');
            startBtn.click(function () {
                updateModal(el);
            });

            // Append to group
            var item = collapsible.find('#' + el.desc.supplierCode);
            item.find('.collapsible-body').append(body);
        });

        $('.collapsible').collapsible();
    }

    var module = {
        'init': function() {
            var page = $('<div class="main-page"></div>');

            var fullPage = $(Templater.useTemplate('scenarios-page', [{
                'assetsPath': ''
            }]));
            
            page.append(fullPage);
            
            $('.main-content').append(page);

            updateRegistry();

            // Update to correct height
            $('html').resize();

            $('.modal').modal();

            context.readyForDisplay(true);
        },
        
        'destroy': function() {
        }
    }
    
    context.currentModule = module;
}());