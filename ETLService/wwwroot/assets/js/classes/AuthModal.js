import { htmlToElement } from "./utils.js";

import { Auth } from "./Auth.js";

const modal = htmlToElement(`
<!-- Modal Structure -->
<div id="login-modal" class="modal">
    <div class="modal-content">
        <h4 class="center">Login form</h4>
        <div class="row">
            <div class="input-field col s6 offset-s3">
                <input id="name" type="text" class="validate">
                <label for="name">Login</label>
            </div>
            <div class="input-field col s6 offset-s3">
                <input id="pass" type="password" class="validate">
                <label for="pass">Password</label>
            </div>
        </div>
    </div>
    <div class="modal-footer">
        <a class="modal-close waves-effect waves-red btn-flat">Close</a>
        <a id="login" class="modal-close waves-effect waves-green btn-flat">Login</a>
    </div>
</div>
`);

class AuthModal {
    constructor(parent) {
        this.auth = new Auth();
        this.nav = parent;

        var login = htmlToElement(`<li><a id="login-btn" class="modal-trigger right" href="#login-modal">Login</a></li>`);

        function addLogout(auth, nav) {
            var logout = htmlToElement(`<li id=\"login-info\"><a>${auth.user.Name} (Logout)</a></li>`);
            logout.addEventListener('click', () => {
                auth.logout();
                nav.querySelector('#login-info').remove();
                document.dispatchEvent(new Event('logout'));

                nav.querySelector('#login-btn').style.display = 'list-item';
            });

            nav.querySelector('#login-btn').style.display = 'none';
            nav.prepend(logout);
        }

        this.nav.appendChild(login);

        if (this.isLogged)
            addLogout(this.auth, this.nav);

        modal.querySelector('#login').addEventListener('click', () => {
            let name = document.querySelector('#name').value;
            let pass = document.querySelector('#pass').value;

            this.auth.login(name, pass, () => {
                addLogout(this.auth, this.nav);
                document.dispatchEvent(new Event('login'));
            });
        });

        document.body.appendChild(modal);

        M.Modal.init(document.querySelector('.modal'), {
            'onCloseStart': function () {
                let pass = document.getElementById('pass');
                pass.value = '';
                pass.focus();
                pass.blur();
            }
        });
    }

    get isLogged() {
        return this.auth.isLogged;
    }

    get user() {
        return this.auth.user;
    }
}

export { AuthModal };