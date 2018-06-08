// Tooltip plugin
(function ($) {
    var methods = {
        'init': function() {
            this.html('<div class="tooltip-content flow-down"></div>');
            this.addClass('hide');
        },
        
        'show': function(panel, content) {
            var html = content.join('<div class="tooltip-CBR"></div>');
            this.find('.tooltip-content').html(html);

            this.removeClass('hide');
            
            // Default position
            var position = { 'x': 0, 'y': 0 };
            
            // Client size
            var size = {
                'x': document.body.clientWidth + $(window).scrollLeft(),
                'y': document.body.clientHeight + $(window).scrollTop()
            }

            // Calculate side
            var leftWidth = $(panel).offset().left;
            var rightWidth = size.x - ($(panel).offset().left + $(panel).width());
            
            var position = {
                'x': rightWidth > this.width() ? $(panel).offset().left + $(panel).width() : $(panel).offset().left - this.width(),
                'y': $(panel).offset().top
            };
 
            // Calculate top
            var bot = $(panel).offset().top + this.height();
            if (bot > size.y)
                position.y -= bot - size.y + 4;

            this.css({ 
                left: position.x,
                top: position.y
            })
            
        },
        
        'hide': function() {
            this.addClass('hide');
        },
    }

    $.fn.ZiVTooltip = function (method) {
        // логика вызова метода
        if ( methods[method] ) {
            return methods[ method ].apply( this, Array.prototype.slice.call( arguments, 1 ));
        } else if ( typeof method === 'object' || ! method ) {
            return methods.init.apply( this, arguments );
        } else {
            $.error( 'Метод с именем ' +  method + ' не существует для jQuery.tooltip' );
        }         
    }
    
    $(document).ready(function(){
        $('.ZiVTooltip').ZiVTooltip();
    });
}( jQuery ));    
