import { Broadcast } from "./Broadcast.js";
import { Request } from "./Request.js";
import { Auth } from "./Auth.js";

const bodyData = document.createElement('template');
bodyData.innerHTML = `
    <div class="loading-container">
        <div class="flow-down">
            <div>Loading</div>
        </div>
    </div>

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

    <!-- full width navigation menu - delete nav element if using top navigation -->
    <div class="navbar-fixed">
        <nav>
            <div class="nav-wrapper blue-grey">
                <ul class="left hide-on-med-and-down" id="nav">
                </ul>
                <ul class="right hide-on-med-and-down" id="login-data">
                    <li><a id="login-btn" class="modal-trigger right" href="#login-modal">Login</a></li>
                </ul>
            </div>
        </nav>
    </div>
    <div class="main container">
        <div class="main-content">
        </div>    
    </div>
`;

const footerData = document.createElement('template');
footerData.innerHTML = `
<footer class="page-footer blue-grey lighten-1">
    <!-- <div class="container">
        <div class="row">
            <div class="col l6 s12">
                <h5 class="white-text">Footer Content</h5>
                <p class="grey-text text-lighten-4">You can use rows and columns here to organize your footer content.</p>
            </div>
            <div class="col l4 offset-l2 s12">
                <h5 class="white-text">Links</h5>
                <ul>
                    <li><a class="grey-text text-lighten-3" href="#!">Link 1</a></li>
                    <li><a class="grey-text text-lighten-3" href="#!">Link 2</a></li>
                    <li><a class="grey-text text-lighten-3" href="#!">Link 3</a></li>
                    <li><a class="grey-text text-lighten-3" href="#!">Link 4</a></li>
                </ul>
            </div>
        </div>
    </div> -->

    <div class="footer-copyright blue-grey">
        <div class="container">
            <span>Version: </span><span id="version"></span>
            <a class="grey-text text-lighten-4 right" href="#!">More Links</a>
        </div>
    </div>
</footer>
`;

class ETLApp {
    constructor() {
        document.body.appendChild(bodyData.content.cloneNode(true));
        document.body.appendChild(footerData.content.cloneNode(true));

        window.addEventListener('hashchange', (e) => this.render(decodeURI(window.location.hash)));

        this.etlContext = {};
        this.modules = {};
    }

    init() {
        console.log('Init App');

        this.broadcast = new Broadcast();
        this.broadcast.connect("ws://" + window.location.host + "/api/broadcast");

        this.auth = new Auth();

        Request.send('api/info', {
            'success': (d) => {
                this.etlContext.info = d;
                let version = this.etlContext.info.version;
                document.querySelector('footer #version').innerHTML =
                    [version.major, version.minor, version.build].join('.');
            }
        });
        
        Request.send('api/pumps/statuses',{
            'success': (d) => { this.etlContext.statuses = d; }
        });

        Request.send('api/pumps/registry', {
            'success': (d) => {
                this.etlContext.registry = d["data"].sort(function(a, b) {
                    return parseInt(a.desc.dataCode) - parseInt(b.desc.dataCode);
                });
            }
        });

        Request.send('api/pumps/updates',{
            'success': (d) => { this.etlContext.updates = d; }
        });

        M.Modal.init(document.querySelector('.modal'), {
            'onCloseStart': function () {
                let pass = document.getElementById('pass');
                pass.value = '';
                pass.focus();
                pass.blur();
            }
        });

        Request.send("api/modules",{
            'success': (d) => {
                this.modules = d;
                window.dispatchEvent(new HashChangeEvent('hashchange'));
            }
        });
    }

    render(url) {
        console.log(url);

        // Get the keyword from the url.
        let temp = url.split('/')[0];

        // Execute the needed function depending on the url keyword (stored in temp).
        if (this.modules[temp]) {
            //loadModule(modules[temp]);
        }
        // If the keyword isn't listed in the above - render the error page.
        else {
            if (document.querySelectorAll(`[href = "${temp}"]`).length === 0) {
                //loadModule(modules[Object.keys(modules)[0]]);
            }
            //renderErrorPage();
        }
    }

    get readyForDisplay() {
        return document.querySelector('.loading-container').classList.contains('visible');
    }

    set readyForDisplay(isReady) {
        let content = document.querySelector('.main-content');
        let loading = document.querySelector('.loading-container');

        if (isReady) {
            content.classList.add('visible');
            loading.classList.remove('visible');
            return;
        }

        content.classList.remove('visible');
        loading.classList.add('visible');        
    }
}

export { ETLApp };