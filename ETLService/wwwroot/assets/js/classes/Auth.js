import { Request } from "./Request.js"

function getCurrentTime() {
    return Math.round((new Date()).getTime() / 1000);
}

class Auth {
    constructor() {
        Request.addInfoHandler((info) => {
            if (localStorage.getItem("token") !== null)
                info.headers['Authorization'] = 'Bearer ' + localStorage.getItem("token");
        });

        this.payload = null;

        if (localStorage.getItem("token") !== null) {
            this.payload = JSON.parse(window.atob(localStorage.getItem("token").split('.')[1]));
            // Remove expired token
            if (parseInt(this.payload.exp, 10) < getCurrentTime())
                localStorage.removeItem("token");
        }
    }

    login(user, pass, success) {
        Request.send('api/token', {
               'info': {
                   'method': 'POST', 
                   'headers': {
                       'Accept': 'application/json',
                       'Content-Type': 'application/json'
                   },
                   'body': JSON.stringify({ username: user, password: pass })
                }
            })
            .then((d) => {
                // Save the token in local storage
                localStorage.setItem("token", d);

                this.payload = JSON.parse(window.atob(d.split('.')[1]));

                if (success != null)
                    success(d);
            });
    }

    logout() {
        localStorage.removeItem("token");
        this.payload = null;
    }

    get user() {
        return {
            'Name': this.payload ? this.payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] : undefined,
            'Role': this.payload ? this.payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] : undefined
        }
    }

    get isLogged() {
        return this.payload != null;
    }
}

export { Auth };