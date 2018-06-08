$(function () {
    var imgUrl = updatePath('assets/images/heroes/');
    var imgUrlAbil = updatePath('assets/images/spellicons/');

    function getTooltipContent( ability ) {
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
    
    function addAbility( parent, ability ) {
        var abil = {
            'name': ability.name,
            'img': imgUrlAbil + ability.img + '.png',
            'borderImg': imgUrlAbil + 'ability_border_outer.png'
        };
        
        var abilityPanel = $(Templater.useTemplate('ability', [abil]));
        
        abilityPanel.hover( 
            function() { $('.ZiVTooltip').ZiVTooltip('show', this, getTooltipContent(ability)); }, 
            function() { $('.ZiVTooltip').ZiVTooltip('hide'); }
        );
        
        parent.append(abilityPanel);
    }

    function generateAbilitiesList( index, hero ){
        $('#heroCard .abilities').html('');

        var layout = hero.abilities_layout.length;
        var hero_abils = hero.abilities.slice();
        
        for(var i = 0; i < layout; i++)
        {
            $('#heroCard .abilities').append('<div class="choice-container" id="ability_' + i + '"></div>');
            var count = hero.abilities_layout[i];
            var parent = $('#ability_' + i);
            
            // Add abilities to container
            for(var a = 0; a < count; a++)
            {
                var abil = zivDB.abilities[hero_abils.shift()];
                if (abil)
                    addAbility( parent, abil );
            }
        }
    }
    
    // Update hero card data
    function updateHeroCard( index, hero ) {
        $('#heroCard .card-title .title').html(hero.u);
        $('.attributes #str .desc').html(hero.str);
        $('.attributes #agi .desc').html(hero.agi);
        $('.attributes #int .desc').html(hero.int);
        $('.attributes #dmg .desc').html(hero.dmg[0] + ' - ' + hero.dmg[1]);
        $('.attributes #ms .desc').html(hero.ms);
        
        $('.attributes .attrib').removeClass('primary-attrib');
        $('.attributes #' + hero.pa).addClass('primary-attrib');
        
        
        $('.card-body .bio').html(hero.bio);
        
        generateAbilitiesList( index, hero );    
    }
    
    // Add hero panel
    function addHero( parent, index, hero ) {
        var icon = $('<img class="hero-icon" src="' + imgUrl + 'small/' + index + '.png" id="icon_' + index +'" />');
        icon.click( function(){ 
            if ( $('#heroCard').hasClass('hide') )
                $('#heroCard').removeClass('hide');
            updateHeroCard( index, hero ); 
        })
        
        parent.find('.' + hero.playstyle).append(icon);
    }    

    // Generate heroes list
    function generateHeroes(){
        var heroes = $(Templater.useTemplate('heroes-page', [{'path': updatePath('')} ]));
        $.each(zivDB.heroes, function(index, hero){
            addHero(heroes, index, hero);
        });

        return heroes;
    }

    var module = {
        'init': function() {
            var page = $('<div class="heroes-page"></div>');

            var heroes = generateHeroes();
            page.append(heroes);
            context.readyForDisplay(true);

            $('.main-content').append(page);
        }
    }
    
    context.currentModule = module;
});