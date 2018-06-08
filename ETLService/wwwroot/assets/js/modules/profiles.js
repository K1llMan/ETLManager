$(function () {
    var imgUrl = updatePath('assets/images/heroes/');
    var imgUrlAbil = updatePath('assets/images/spellicons/');
    var imgUrlItem = updatePath('assets/images/items/');

    // Item tooltip
    function getItemTooltipContent( item ) {
        if (!item)
            return [];
            
        var content = ['<div class="item-container" style="margin-left: auto; margin-right: auto;"><img id="item_icon" src="' + imgUrlItem + (item.img ? item.img : 'default') + '.png"></div>'];
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
    
    // Item generation
    function generateItem( itemContent ) {
        var itemData = zivDB.items[itemContent.item];
        var item = {
            'img': imgUrlItem + (itemData && itemData.img ? itemData.img : 'default') + '.png'
        }        
        
        var itemPanel = $(Templater.useTemplate('item', [item]));
        itemPanel.hover( 
            function() { $('.ZiVTooltip').ZiVTooltip('show', this, getItemTooltipContent(itemData)); }, 
            function() { $('.ZiVTooltip').ZiVTooltip('hide'); }
        );        
        
        return itemPanel;        
    }
    
    // Ability tooltip
    function getAbilityTooltipContent( ability ) {
        if (!ability)
            return [];
    
        var content = ['<img id="ability_icon" src="' + imgUrlAbil + ability.img + '.png">'];
        content.push('<div id="ability_name" class="tooltip-label">' + ability.dname +'</div>');

        var resources = '';

        var cd = ability.cmb.split('|')[0].replace(/ /g, '/');
        var mana = ability.cmb.split('|')[1].replace(/ /g, '/');

        if (mana != '')
            resources += '<div class="flow-right" id="mana"><a class="tooltip-label">Energy cost:</a><a class="value">' +  mana + '</a></div>';
        if (cd != '')
            resources += '<div class="flow-right" id="cd"><a class="tooltip-label">Cooldown:</a><a class="value">' +  cd + '</a></div>';
            
        if (ability.ability_cast_point != null && ability.ability_cast_point == '')
            resources += '<div class="flow-right" id="castPoint"><a class="tooltip-label">Cooldown:</a><a class="value">' +  ability.ability_cast_point + '</a></div>';
            
        if (resources != '')
            content.push(resources);
            
        content.push('<div class="attribs tooltip-label">' + ability.attrib + '</div>');
        
        return content;
    }
    
    // Ability generation
    function generateAbility( abilityName ) {
        var abilityData = zivDB.abilities[abilityName];
        
        var ability = {
            'img': imgUrlAbil + (abilityData && abilityData.img ? abilityData.img : 'ziv_empty') + '.png',
            'borderImg': updatePath('assets/images/spellicons/ability_border_outer.png')
        }

        var abilityPanel = $(Templater.useTemplate('ability', [ability]));
        abilityPanel.hover( 
            function() { $('.ZiVTooltip').ZiVTooltip('show', this, getAbilityTooltipContent(abilityData)); }, 
            function() { $('.ZiVTooltip').ZiVTooltip('hide'); }
        );        
        
        return abilityPanel;
    }
    
    // Return status struct
    function getStatus( status ) {
        switch(status) {
            case 'dev': return { 'class': 'status-dev', 'name': 'Developer' };
            case 'tester': return { 'class': 'status-tester', 'name': 'Tester' };
            case 'friend': return { 'class': 'status-friend', 'name': 'Friend' };
            case 'sup': return { 'class': 'status-sup', 'name': 'Support' };
        }
        
        return null;
    }
    
    // Generate main profile panel
    function generateProfileCard( data ) {
        var borderImg = 'wood';
        switch(parseInt(data.exp / 10000)){
            case 1: 
                borderImg = 'bronze';
                break;
            case 2: 
                borderImg = 'silver';
                break;
            case 3: 
                borderImg = 'gold';
                break;
            case 4: 
                borderImg = 'platinum';
                break;
        }
    
        data.avatar = updatePath('assets/images/spellicons/holdout_gods_strength.png');
        data.borderImg = updatePath('assets/images/account/{0}.png'.format(borderImg));

        var card = $(Templater.useTemplate('profiles-page', [data]));

        // Add separator
        if (data.status && data.status.length > 0)
            $('<div class="CBR"></div>').insertBefore(card.find('.statuses'));
        
        // Append statuses
        $.each(data.status, function(k, v){
            var params = getStatus(v);
            if (!params)
                return;

            var status = $(Templater.useTemplate('status', [params]));
            card.find('.statuses').append(status);
        });
        
        return card;
    }

    // Slider controls
    
    function checkArrowsVisibility( panel ) {
        var step = parseFloat(panel.attr('step'));
        var pos = Math.abs(parseInt(panel.attr('pos')));
        var count = step == 0 ? 0 : Math.floor(100 / step);

        panel.find('.arrow-left').css({ 'visibility': (pos == 0 ? 'collapse' : 'visible') });
        panel.find('.arrow-right').css({ 'visibility': (pos == count - 1 || count == 0 ? 'collapse' : 'visible') });
    }
    
    function sliderLeft() {
        var panel = $(this).parents('.heroes-slider');
        var pos = parseInt(panel.attr('pos')) + 1;
        panel.attr('pos', pos);
        var step = parseFloat(panel.attr('step'));
        panel.find('.heroes-card').css({ 'transform': 'translateX({0}%)'.format(pos * step) });
        
        checkArrowsVisibility(panel);
    }
    
    function sliderRight() {
        var panel = $(this).parents('.heroes-slider');
        var pos = parseInt(panel.attr('pos')) - 1;
        panel.attr('pos', pos);
        var step = parseFloat(panel.attr('step'));
        panel.find('.heroes-card').css({ 'transform': 'translateX({0}%)'.format(pos * step) });
        
        checkArrowsVisibility(panel);
    }
    
    // Main profile function
    function generateProfile( data ) {
        var card = generateProfileCard( data );
        
        var heroes = data.heroes.map(function(v){
            return v;
        });

        $.each(heroes, function(k, v){
            var name = v.HeroName.replace('npc_dota_hero_', '');
            v.img = updatePath('assets/images/heroes/small/{0}.png'.format(name));
            
            var heroCard = $(Templater.useTemplate('hero-card', [v]));
            card.find('.heroes-card').append(heroCard);
            card.find('.equip-bg').attr('src', updatePath('assets/images/equipment/equipment_bg.png'));
            
            $.each(v.Abilities, function(k, v){
                var ability = generateAbility(v);
                heroCard.find('.abilities').append(ability);
            });
            
            $.each(zivDB.heroes[name].equipment_slots, function(k, v){
                var slot = heroCard.find('.slot' + k);
                slot.addClass(v.replace(/[\d]/g, ''));
                slot.find('img').attr('src', updatePath('assets/images/equipment/{0}.png').format(v));
            });
            
            
            $.each(v.Equipment, function(k, v){
                var item = generateItem(v);
                var slot = zivDB.items[v.item].slot.toLowerCase();
                
                var slotPanels = heroCard.find('.' + slot + '.empty');
                if (slotPanels.length > 0) {
                    $(slotPanels[0]).html(item);
                    $(slotPanels[0]).removeClass('empty');
                }
            });
        });
        
        // Set slider container width
        card.find('.heroes-card').css({ 'width': heroes.length * 310 });
        card.find('.heroes-slider').attr('pos', 0);
        card.find('.heroes-slider').attr('step', heroes.length > 0 ? 100 / heroes.length : 0);
        
        card.find('.arrow-left').click(sliderLeft);
        card.find('.arrow-right').click(sliderRight);
        
        checkArrowsVisibility(card.find('.heroes-slider'));
        
        return card;
    }

    // Error page
    function showErrorInfo() {
    	return $('<h3>Sorry, player not found.</h3>');
    }

    var module = {
        'init': function( steamID64 ) {
            var page = $('<div class="profiles-page"></div>');

            //var steamID64 = '76561197996730911';
            var steamID32 = String(Number(steamID64.substring(3)) - 61197960265728);

            // Send the data using post 
            var posting = $.post( ajaxUrl, {
                action: 'get_player_info',
                steamID64: steamID64,
                steamID32: steamID32
            })
            .done(function( data ) {
            	if (data.length == 0){
            		page.append(showErrorInfo());
            		context.readyForDisplay(true);
            		return;
            	}

            	var playerData = JSON.parse(data);

                var card = generateProfile(playerData);
	            page.append(card);
	            context.readyForDisplay(true);
            });
            
            $('.main-content').append(page);
        }
    }
    
    context.currentModule = module;
});