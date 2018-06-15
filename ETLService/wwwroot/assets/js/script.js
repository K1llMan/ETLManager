var context = {
    'currentModule': null,
    'readyForDisplay': function( isReady ){
        var content = $('.main-content');
        var loading = $('.loading-container');
        
        if (isReady){
            content.addClass('visible');
            loading.removeClass('visible');
            return;
        }
        
        content.removeClass('visible');
        loading.addClass('visible');
    }, 
    'isLogged': false
}

// Format string
String.prototype.format = String.prototype.f = function () {
    var args = arguments;
    return this.replace(/\{\{|\}\}|\{(\d+)\}/g, function (m, n) {
        if (m === "{{") { return "{"; }
        if (m === "}}") { return "}"; }
        return args[n];
    });
};

var pumpsRegistry = null;

// These are called on page load
$(function () {
    $.ajaxSetup({
        beforeSend: function (xhr) {
            if (localStorage.getItem("token") !== null) {
                xhr.setRequestHeader('Authorization', 'Bearer ' + localStorage.getItem("token"));
            }
        }
    });

    function Authorization(user, pass) {
        $.post("api/token", $.param({ username: user, password: pass }))
            .done(function (token) {
                //save the token in local storage
                localStorage.setItem("token", token);
                //...
                //getModules();
            })
            .fail(function() {
                
            });        
    }

    Authorization("Admin", "Admin");

    var modules = {};
    var modulesPath = 'assets/js/modules/';
    var templatesPath = 'assets/templates/';

    // Load module script
    function loadModuleScript( script, params ) {
        $.getScript( modulesPath + script, function( data, textStatus, jqxhr ) {
            // Init module after loading
            context.currentModule.init(params);

            /*console.log( data ); // Data returned
            console.log( textStatus ); // Success
            console.log( jqxhr.status ); // 200
            console.log( "Load was performed." );
            */
        });
    }

	//	Event handlers for frontend navigation
    function loadModule( module, params ) {
        if (context.currentModule)
            if (context.currentModule.destroy)
                context.currentModule.destroy();
    
		// Hide whatever page is currently shown.
        var content = $('.main-content');
		content.html('');

        context.readyForDisplay(false);
        
        if (module.template)
            // Load templates first
            Templater.downloadTemplates(templatesPath + module.template, function(){
                loadModuleScript(module.script, params);
            });
        else
            loadModuleScript(module.script, params);
    }

    // Global error handler    
    $( document ).ajaxError(function(event, request, settings) {
        console.log(event);
        renderErrorPage();
    });
    
    $(window).on('error', function() {
        renderErrorPage();     
    });    
    
    // An event handler with calls the render function on every hashchange.
    // The render function will show the appropriate content of out page.
    $(window).on('hashchange', function(){
        render(decodeURI(window.location.hash));
    });

    context.readyForDisplay(false);

    function getRegistry() {
        // Send the data using post 
        $.get("api/pumps/registry")
            .done(function (data) {
                pumpsRegistry = data["data"];

                // Manually trigger a hashchange to start the app.
                $(window).trigger('hashchange');
            });
    }

    function getModules() {
        // Send the data using post 
        $.get("api/modules")
            .done(function (data) {
                modules = data;

                // Generate navigation
                $.each(modules, function (key, value) {
                    var nav = $('#nav');

                    var displayName = value.displayName;
                    var displayImage = value.displayImage;

                    if (displayName === undefined && displayImage === undefined)
                        return;

                    var li = $('<li class="waves-effect waves-light"></li>');
                    if (!!displayImage) {
                        var img = $('<img src={0}>'.format('assets/images/' + displayImage));
                        li.append(img);
                    }

                    if (!!displayName) {
                        var name = $('<a>{0}</a>'.format(displayName));
                        li.append(name);
                    }

                    li.click(function () {
                        window.location.hash = key;
                    });

                    nav.append(li);
                });

                getRegistry();
            });
    };

    getModules();

	// Navigation
	function render(url) {
		// Get the keyword from the url.
		var temp = url.split('/')[0];

		// Execute the needed function depending on the url keyword (stored in temp).
		if(modules[temp]){
            loadModule(modules[temp]);
		}
		// If the keyword isn't listed in the above - render the error page.
		else {
            if ($('[href = "{0}"]'.format(temp)).length === 0)
                loadModule(modules[Object.keys(modules)[0]]);
                //renderErrorPage();
		}
	}
    
	// Shows the error page.
	function renderErrorPage(){
        context.readyForDisplay(true);
        $('.main-content').html('<h3>Sorry, something went wrong :(</h3>');
	}    
});