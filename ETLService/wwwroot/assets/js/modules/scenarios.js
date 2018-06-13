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

            // Update to correct height
            $('html').resize();

            $('.collapsible').collapsible();

            context.readyForDisplay(true);
        },
        
        'destroy': function() {
        }
    }
    
    context.currentModule = module;
}());