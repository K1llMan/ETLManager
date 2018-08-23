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
    'isLogged': function () { return localStorage.getItem("token") !== null; }
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

var etlContext = {};

Broadcast.setHandlers(broadcastHandlers);
Broadcast.connect("ws://" + window.location.host + "/api/broadcast");

// These are called on page load
$(function () {
    var modules = {};
    var modulesPath = 'assets/js/modules/';
    var templatesPath = 'assets/templates/';

    function addLogout() {
        var logout = $("<li id=\"login-info\"><a>{0}</a></li>".format(Auth.User().Name + " (Logout)"));
        logout.click(function () {
            Auth.Logout();
            logout.remove();
            $("#login-btn").css({ "display": "list-item" });
            getModules();
        });

        $("#login-data").prepend(logout);
        $("#login-btn").css({ "display": "none" });
    }

    if (context.isLogged())
        addLogout();    
    
    // Authorization
    $('#login').click(function () {
        var name = $("#name").val();
        var pass = $("#pass").val();

        Auth.Login(name, pass, function () {
            addLogout();
            getModules();
        });
    });

    $('.modal').modal({
        complete: function () {
            $("#pass").val('');
            $("#pass").blur();
        }
    });

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

    function getInfo() {
        // Send the data using post 
        $.get("api/info")
            .done(function (data) {
                etlContext.info = data;
                var version = etlContext.info.version;

                $('footer #version').html([version.major, version.minor, version.build].join('.'));
            });
    }

    function getStatuses() {
        // Send the data using post 
        $.get("api/pumps/statuses")
            .done(function (data) {
                etlContext.statuses = data.data;
            });
    }

    function getUpdates() {
        // Send the data using post 
        $.get("api/pumps/updates")
            .done(function (data) {
                etlContext.updates = data.data;
            });
    }

    function getRegistry() {
        // Send the data using post 
        $.get("api/pumps/registry")
            .done(function (data) {
                //broadcast.send(JSON.stringify(data));

                etlContext.pumpsRegistry = data["data"].sort(function (a, b) { return parseInt(a.desc.dataCode) - parseInt(b.desc.dataCode) });

                // Manually trigger a hashchange to start the app.
                $(window).trigger('hashchange');
            });
    }

    function getModules() {
        // Send the data using post 
        $.get("api/modules")
            .done(function (data) {
                $('#nav').html('');
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

                    nav.prepend(li);
                });

                getInfo();
                getStatuses();
                getUpdates();
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