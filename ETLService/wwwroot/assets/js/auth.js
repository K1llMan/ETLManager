/*-----------------------------------------------------------------------------
                             JWT Auth module (jQuery 3.3.1)
-----------------------------------------------------------------------------*/
Auth = (function() {
    // Adding JWT header
    $.ajaxSetup({
        beforeSend: function (xhr) {
            if (localStorage.getItem("token") !== null) {
                xhr.setRequestHeader('Authorization', 'Bearer ' + localStorage.getItem("token"));
            }
        }
    });

    function getCurrentTime() {
        return Math.round((new Date()).getTime() / 1000);
    }

    var payload;
    if (localStorage.getItem("token") !== null) {
        payload = JSON.parse(window.atob(localStorage.getItem("token").split('.')[1]));
        // Remove expired token
        if (parseInt(payload.exp, 10) < getCurrentTime())
            localStorage.removeItem("token");
    }

    return {
        'Login': function(user, pass, success) {
            $.post("api/token", $.param({ username: user, password: pass }))
                .done(function (token) {
                    //save the token in local storage
                    localStorage.setItem("token", token);

                    payload = JSON.parse(window.atob(token.split('.')[1]));

                    if (success)
                        success();
                })
                .fail(function () {

                });
        },

        'Logout': function() {
            localStorage.removeItem("token");
        },

        'User': function () {
            return {
                'Name': payload ? payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] : undefined,
                'Role': payload ? payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] : undefined
            }
        }
    };
})();