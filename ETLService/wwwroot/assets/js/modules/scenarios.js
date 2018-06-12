$(function () {
    var module = {
        'init': function() {
            var page = $('<div class="main-page"></div>');

            var fullPage = $(Templater.useTemplate('scenarios-page', [{
                'assetsPath': ''
            }]));
            
            page.append(fullPage);
            
            $('.main-content').append(page);
            
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