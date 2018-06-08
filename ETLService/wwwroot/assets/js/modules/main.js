$(function () {
    // Hero slides
    function generateHeroSlides(){
        var heroesData = Object.keys(zivDB.heroes).map(function(key){
            return {
                'assetsPath': '',
                'hero': key,
                'name': zivDB.heroes[key].u,
                'bio': zivDB.heroes[key].bio,
            };
        });
        
        var slides = $(Templater.useTemplate('hero-slide', heroesData));
        return slides;
    }

    function heroSlideLoad( panel ) {
        if ($(panel)[0].id !== 'section-heroes' && $(panel).parents('#section-heroes').length === 0)
            return;
    
        var curPanel = $(panel)
        if ($(panel)[0].id === 'section-heroes')
            curPanel = curPanel.find('.active');
            
        curPanel.find('.horizontal-line').css({ width: '0%' });
        curPanel.find('.horizontal-line').delay(500).animate({ width: '80%' }, 500);

        curPanel.find('.hero-intro img').css({ left: '-100%', opacity: '0' })
        curPanel.find('.hero-intro img').delay(500)
            .animate({ left: '0%', opacity: '1' }, { 
                duration: 500, 
                specialEasing: { left: 'easeOutExpo', opacity: 'linear' } 
            });
    }
    
    function heroSlideLeave( panel ) {
        if ($(panel)[0].id !== 'section-heroes' && $(panel).parents('#section-heroes').length === 0)
            return;

        $(panel).find('.horizontal-line').css({ width: '0%' });
        $(panel).find('.hero-intro img').css({ left: '-100%', opacity: '0' });
    }
    
    // Map slides
    function generateMapSlides(){
        var mapsData = Object.keys(zivDB.maps).map(function(key){
            return {
                'assetsPath': '',
                'map': key,
                'caption': zivDB.maps[key].story.match('<b>.*</b>')[0].replace(/<.+?>/g, ''),
                'story': zivDB.maps[key].story.match('</font>.*')[0].replace(/(<.+\s-\s)?/, ''),
            };
        });
        
        var slides = $(Templater.useTemplate('map-slide', mapsData));
        return slides;
    }
    
    function mapSlideLoad( panel ) {
        if ($(panel)[0].id !== 'section-maps' && $(panel).parents('#section-maps').length === 0)
            return;
    
        var curPanel = $(panel)
        if ($(panel)[0].id === 'section-maps')
            curPanel = curPanel.find('.active');
            
        curPanel.find('.horizontal-line').css({ width: '0%' });
        curPanel.find('.horizontal-line').delay(500).animate({ width: '80%' }, 500);
    }
    
    function mapSlideLeave( panel ) {
        if ($(panel)[0].id !== 'section-maps' && $(panel).parents('#section-maps').length === 0)
            return;

        $(panel).find('.horizontal-line').css({ width: '0%' });
    }    
    
    var module = {
        'init': function() {
            var page = $('<div class="main-page"></div>');

            var fullPage = $(Templater.useTemplate('main-page', [{
                'assetsPath': ''
            }]));
            
            //fullPage.find('#section-heroes').append(generateHeroSlides());
            //fullPage.find('#section-maps').append(generateMapSlides());
            page.append(fullPage);
            
            $('.main-content').append(page);

            if ($('html').hasClass('fp-enabled'))
                $.fn.fullpage.destroy('all');

            $('#fullpage').fullpage({
                //anchors: ['firstPage', 'secondPage', '3rdPage'],
                //sectionsColor: ['#C63D0F', '#1BBC9B', '#7E8F7C'],
                
                navigation: true,
                navigationPosition: 'right',
                
                easingcss3: 'cubic-bezier(0.645, 0.045, 0.355, 1)',
                scrollingSpeed: 1000,

                afterLoad: function(anchorLink, index) {
                    // Hide scrollers
                    $('.scroll-up').css({ visibility: $(this)[0].id === 'section-intro' ? 'collapse' : 'visible' });
                    $('.scroll-down').css({ visibility: $(this)[0].id === 'section-maps' ? 'collapse' : 'visible' });

                    // Call afterSlideLoad for heroes
                    if ($(this).find('.slide').length > 0){
                        heroSlideLoad(this);
                        mapSlideLoad(this);
                    }
                    else {
                        $(this).find('.horizontal-line').css({ width: '0%' });
                        $(this).find('.horizontal-line').delay(500).animate({ width: '80%' }, 500);
                    }
                },
                
                onLeave: function(index, nextIndex, direction){
                    $(this).find('.horizontal-line').css({ width: '0%' });
                    
                    // Call afterSlideLoad for heroes
                    if ($(this).find('.slide').length > 0){
                        heroSlideLeave(this);
                        mapSlideLeave(this);
                    }
                },
                
                afterSlideLoad (anchorLink, index, slideAnchor, slideIndex) {
                    heroSlideLoad(this);
                    mapSlideLoad(this);
                },
                
                onSlideLeave (anchorLink, index, slideIndex, direction, nextSlideIndex) {
                    heroSlideLeave(this);
                    mapSlideLeave(this);
                }
            });
            
            // Update to correct height
            $('html').resize();
            
            $('.arrow-up').click(function() {
                $.fn.fullpage.moveSectionUp();
            });

            $('.arrow-down').click(function() {
                $.fn.fullpage.moveSectionDown();
            });

            $('.arrow-left').click(function() {
                $.fn.fullpage.moveSlideLeft();
            });

            $('.arrow-right').click(function() {
                $.fn.fullpage.moveSlideRight();
            });
            
            context.readyForDisplay(true);
        },
        
        'destroy': function() {
            $.fn.fullpage.destroy('all');
        }
    }
    
    context.currentModule = module;
});