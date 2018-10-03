import { htmlToElement } from "./utils.js";

import { Broadcast } from "./Broadcast.js";
import { Request } from "./Request.js";
import { AuthModal } from "./AuthModal.js";
import { UpdatesModal } from "./UpdatesModal.js";

const bodyData = htmlToElement(`
    <div class="loading-container">
        <div class="flow-down">
            <div>Loading</div>
        </div>
    </div>

    <!-- full width navigation menu - delete nav element if using top navigation -->
    <div class="navbar-fixed">
        <nav>
            <div class="nav-wrapper blue-grey">
                <ul class="left hide-on-med-and-down" id="nav">
                </ul>
                <ul class="right hide-on-med-and-down" id="login-data">
                </ul>
            </div>
        </nav>
    </div>
    <div class="main container">
        <div class="main-content">
        </div>
    </div>

  <div class="fixed-action-btn">
    <a id="updatesBtn" class="btn-floating btn-large pulse hide">
      <i class="large material-icons">autorenew</i>
    </a>
  </div>
`);

const footerData = htmlToElement(`
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
`);

const broadcastHandlers = {
    'receiveUpdate': (data) => {
        if (document.app.etlContext.updates == null)
            document.app.etlContext.updates = {};

        document.app.etlContext.updates[data.programID] = data;
        document.querySelector('#updatesBtn').classList.toggle('hide',
            Object.keys(document.app.etlContext.updates).length == 0);
    },
    'update': (data) => {
        let registry = document.app.etlContext.registry;
        let programID = data.info.programID;

        // Remove update record
        delete document.app.etlContext.updates[programID];

        let config = registry.find((c) => c.programID = programID);
        if (config != null)
            registry[registry.indexOf(config)] = data.config;
        else {
            registry.push(data.config);

            // Sort data after adding new records
            document.app.etlContext.registry = registry.sort(function (a, b) {
                return parseInt(a.desc.dataCode) - parseInt(b.desc.dataCode);
            });
        }

        document.querySelector('#updatesBtn').classList.toggle('hide',
            Object.keys(document.app.etlContext.updates).length == 0);

        M.toast({ html: `"${programID}" updated!` });
    }
}

class ETLApp {
    constructor() {
        document.body.appendChild(bodyData);
        document.body.appendChild(footerData);

        window.addEventListener('hashchange', (e) => this.render(decodeURI(window.location.hash)));
        document.addEventListener('login', () => {
            console.log('login');
            this.getModules();
        });

        document.addEventListener('logout', () => {
            console.log('logout');
            this.getModules();
        });

        this.etlContext = {};
        this.modules = {};
    }

    getModules() {
        Request.send("api/modules", {
            'success': (d) => {
                this.modules = d;

                document.querySelector('#nav').innerHTML = '';
                Object.keys(this.modules).forEach((k) => {
                    let value = this.modules[k];
                    let nav = document.querySelector('#nav');

                    let displayName = value.displayName;
                    let displayImage = value.displayImage;

                    if (displayName === undefined && displayImage === undefined)
                        return;

                    let li = htmlToElement('<li class="waves-effect waves-light"></li>');
                    if (!!displayImage) {
                        let img = htmlToElement(`<img src=${'assets/images/' + displayImage}>`);
                        li.append(img);
                    }

                    if (!!displayName) {
                        let name = htmlToElement(`<a>${displayName}</a>`);
                        li.append(name);
                    }

                    li.addEventListener('click', () => window.location.hash = k);

                    nav.append(li);
                });

                window.dispatchEvent(new HashChangeEvent('hashchange'));
            }
        });        
    }

    init() {
        console.log('Init App');

        this.broadcast = new Broadcast();
        this.broadcast.connect("ws://" + window.location.host + "/api/broadcast");

        this.auth = new AuthModal(document.querySelector('nav .nav-wrapper #login-data'));
        this.updates = new UpdatesModal(document.body);

        document.querySelector('#updatesBtn').addEventListener('click', () => {
            this.updates.open(this.etlContext);
        });

        Request.send('api/info', {
            'success': (d) => {
                this.etlContext.info = d;
                let version = this.etlContext.info.version;
                document.querySelector('footer #version').innerHTML =
                    [version.major, version.minor, version.build].join('.');
            }
        });
        
        Request.send('api/pumps/statuses',{
            'success': (d) => { this.etlContext.statuses = d.data; }
        });

        Request.send('api/pumps/registry', {
            'success': (d) => {
                this.etlContext.registry = d.data.sort(function(a, b) {
                    return parseInt(a.desc.dataCode) - parseInt(b.desc.dataCode);
                });

                this.getModules();
            }
        });

        Request.send('api/updates',{
            'success': (d) => {
                this.etlContext.updates = d.data;
                document.querySelector('#updatesBtn').classList.toggle('hide', Object.keys(this.etlContext.updates).length == 0);
            }
        });

        Broadcast.addHandlers(broadcastHandlers);
    }

    loadPage(moduleName) {
        this.readyForDisplay = false;

        if (this.module != null)
            if (this.module.destroy != null)
                this.module.destroy();

        import(`../modules/${moduleName}/${moduleName}.js`)
            .then((module) => {
                this.module = new module.module(this, document.querySelector('.main-content'));
                this.readyForDisplay = true;
            });
    }

    renderErrorPage() {
        document.querySelector('.main-content').innerHTML = '<h3>Sorry, something went wrong :(</h3>';
        this.readyForDisplay = true;
    }

    render(url) {
        // Get the keyword from the url.
        let temp = url.split('/')[0];

        // Execute the needed function depending on the url keyword (stored in temp).
        if (this.modules[temp]) {
            this.loadPage(this.modules[temp].script);
        }
        // If the keyword isn't listed in the above - render the error page.
        else {
            if (document.querySelectorAll(`[href = "${temp}"]`).length === 0) {
                //loadModule(modules[Object.keys(modules)[0]]);
            }
            this.renderErrorPage();
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