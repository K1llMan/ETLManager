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

    function updatePumpModal(config) {
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
        $.each(etlContext.pumpsRegistry, function (i, el) {
            if (groups.indexOf(el.desc.supplierCode) == -1)
                groups.push(el.desc.supplierCode);
        });

        var collapsibleItems = $(Templater.useTemplate('collapsible-item', groups.map(function (id) {
                return { "id": id, "name": id };
            })
        ));

        collapsible.append(collapsibleItems);

        $.each(etlContext.pumpsRegistry, function (i, el) {
            var body = $(Templater.useTemplate('collapsible-body-item', [el]));

            var desc = $.extend(true, {}, el.desc);
            desc.id = el.id;
            desc.version = el.version;

            var pumpDesc = $(Templater.useTemplate('pump-desc', [desc]));
            setStatus($(pumpDesc).find('#status'), etlContext.statuses[el.id]);

            body.prepend(pumpDesc);
            var startBtn = body.find('#buttons #start');
            startBtn.click(function () {
                updatePumpModal(el);
            });

            // Append to group
            var item = collapsible.find('#' + el.desc.supplierCode);
            item.find('.collapsible-body').append(body);
        });

        $.each(collapsibleItems, function (i, el) {
            updateBadges(el);
        });

        $('.collapsible').collapsible();
    }

    function updateUpdatesModal() {
        var modal = $('#update-modal');
        var collection = modal.find('.collection');
        collection.html('');

        $.each(etlContext.updates, function (i, el) {
            var upRec = $.extend(true, {}, el);
            var name = 'New pump';
            var dataCode = '0000';

            // Search pump config
            var config = etlContext.pumpsRegistry.filter(function(c) {
                return c.id == upRec.programID;
            });

            if (config[0] != null) {
                name = config[0].desc.name;
                dataCode = config[0].desc.dataCode;
            }
            upRec.dataCode = dataCode;
            upRec.name = name;

            var record = $(Templater.useTemplate('update-record', [upRec]));

            record.find('.chips-container').append((Templater.useTemplate('chip', [{ 'tag': 'new' }, { 'tag': 'module' }, { 'tag': 'config' }])));
            collection.append(record);
        });

        modal.modal('open');
    }

    function updateUpdates() {
        var btn = $('#updatesBtn');
        btn.toggleClass('hide', etlContext.updates.length < 1);

        btn.click(function() {
            updateUpdatesModal();
        });
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
            updateUpdates();

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