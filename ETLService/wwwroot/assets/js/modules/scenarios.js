$(function () {
    var module = {
        'init': function() {
            var page = $('<div class="main-page"></div>');

            var fullPage = $(Templater.useTemplate('scenarios-page', [{
                'assetsPath': ''
            }]));
            
            page.append(fullPage);
            
            $('.main-content').append(page);

            var collapsible = $('.collapsible');
            // Groups by id
            var groups = [];
            $.each(pumpsRegistry, function(i, el) {
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
                var startBtn = body.find('#buttons').first();
                startBtn.click(function () {
                    var modal = $('#params-modal');
                    modal.find("h4").html(el.desc.name);

                    modal.modal('open');
                });

                // Append to group
                var item = collapsible.find('#' + el.desc.supplierCode);
                item.find('.collapsible-body').append(body);
            });

            // Update to correct height
            $('html').resize();

            $('.collapsible').collapsible();
            $('.modal').modal();

            context.readyForDisplay(true);
        },
        
        'destroy': function() {
        }
    }
    
    context.currentModule = module;
}());