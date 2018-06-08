$(function () {
    var imgUrl = updatePath('assets/images/items/');

    function getTooltipContent( item ) {
        var content = ['<div class="item-container" style="margin-left: auto; margin-right: auto;"><img id="item_icon" src="' + imgUrl + (item.img ? item.img : 'default') + '.png"></div>'];
        if (item.dname)
            content.push('<div id="item_name" class="tooltip-label">' + item.dname +'</div>');
        
        if (item.slot != null && item.slot != '')
            content.push('<div id="item_slot" class="flow-right"><a class="tooltip-label">Slot:</a><a class="value">' + item.slot + '</a></div>')

        if (item.desc)
            content.push('<div class="tooltip-label" id="item_desc">' + item.desc + '</div>');
        if (item.attrib != '')
            content.push('<div class="attribs tooltip-label">' + item.attrib + '</div>');

        return content;
    }
    
    // Add item panel
    function addItem( parent, index, item ) {
        var itemData = {
            'name': item.name,
            'img': imgUrl + (item.img ? item.img : 'default') + '.png'
        };

        var itemPanel = $(Templater.useTemplate('item-container', [itemData]));
        parent.append(itemPanel);

        itemPanel.hover(         
            function() { $('.ZiVTooltip').ZiVTooltip('show', this, getTooltipContent(item)); }, 
            function() { $('.ZiVTooltip').ZiVTooltip('hide'); }
        );
    }
    
    // Generate heroes list
    function generateItems(){
        var items = $(Templater.useTemplate('items-page'));
        $.each(zivDB.items, function(index, item){
            addItem(items, index, item);
        });

        return items;
    }

    var module = {
        'init': function() {
            var page = $('<div class="items-page"></div>');

            var items = generateItems();
            page.append(items);
            context.readyForDisplay(true);

            $('.main-content').append(page);
        }
    }
    
    context.currentModule = module;
});