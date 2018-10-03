import { htmlToElement } from "../../classes/utils.js";
import { Broadcast } from "../../classes/Broadcast.js";
import { PageCommon } from "../PageCommon.js";
import { ParamsModal } from "./ParamsModal.js";

function getPage() {
    return htmlToElement(`
      <div class="scenarios-page">
        <ul class="collapsible">
        </ul>    
      </div>
    `);
};

function getCollapsibleItem(id) {
    return htmlToElement(`  
        <li id="${id}">
            <div class="collapsible-header">
                ${id}
                <div class="badges-container">
                <span class="new badge red" data-badge-caption="errors">4</span>
                <span class="new badge orange" data-badge-caption="warnings">4</span>
                <span class="new badge green" data-badge-caption="running">4</span>
                </div>
            </div>
            <div class="collapsible-body">
            </div>
        </li>`);
}

const broadcastHandlers = {
    'startPump': (data) => {
        let icon = document.querySelector(`#${data.id} #info .material-icons`);
        setStatus(icon, "Running");

        updateBadges(icon.closest('li'));
    },
    'endPump': (data) => {
        let icon = document.querySelector(`#${data.id} #info .material-icons`);
        setStatus(icon, data.status);

        updateBadges(icon.closest('li'));
    },
    'receiveUpdate': (data) => {
    },
    'update': (data) => {
        // Update pumps list
        updateRegistry(document.app.module);
    }
}

function setStatus(icon, status) {
    // Remove colors
    icon.className = icon.className.replace(/[\s](.+)?-text/g, '');

    let iconStr = 'remove';
    let color = '';
    switch (status) {
        case "Running":
            iconStr = 'trending_flat';
            break;
        case 'Successful':
            iconStr = 'check';
            color = 'green-text';
            break;
        case 'Warnings':
            iconStr = 'error_outline';
            color = 'orange-text';
            break;
        case 'Errors':
            iconStr = 'error';
            color = 'red-text';
            break;
        case "Terminated":
            iconStr = 'clear';
            break;
        case 'None':
            iconStr = 'remove';
            break;
    }

    icon.innerHTML = iconStr;
    if (color != '')
        icon.classList.add(color);
}

function updateBadges(header) {
    let statuses = [...header.querySelectorAll("#status")];

    let running = statuses.filter((s) => s.innerHTML == 'trending_flat').length;
    let warnings = statuses.filter((s) => s.innerHTML == 'error_outline').length;
    let errors = statuses.filter((s) => s.innerHTML == 'error').length;

    let green = header.querySelector(".badge.green");
    green.classList.toggle('hide', running < 1);
    green.innerHTML = running;

    let orange = header.querySelector(".badge.orange");
    orange.classList.toggle('hide', warnings < 1);
    orange.innerHTML = warnings;

    let red = header.querySelector(".badge.red");
    red.classList.toggle('hide', errors < 1);
    red.innerHTML = errors;
}

function getCollapsibleBody(pump, status) {
    let body = htmlToElement(`
        <div id="${pump.id}" class="row pumpRow">
            <div id="info" class="col s10">
                <div class="valign-wrapper" >
                    <i id="status" class="material-icons">${status}</i> ${pump.desc.dataCode} ${pump.desc.name}
                </div>
                <div>${pump.desc.comment}</div>
                <div>
                    <span>PumpIdentifier: ${pump.id}</span>
                    <span>Version: ${pump.version}</span>
                </div>
            </div>
            <div id="buttons" class="col s2">
                <i id="history" class="right medium material-icons">view_headline</i>
                <i id="start" class="right medium material-icons green-text">play_arrow</i>
            </div>
        </div>`);

    setStatus(body.querySelector('#status'), status);

    return body;
}

function updateRegistry(page) {
    let collapsible = page.container.querySelector('.collapsible');
    // Clear old data
    collapsible.innerHTML = '';

    // Groups by id
    let groups = [];
    page.app.etlContext.registry.forEach((p) => {
        if (groups.indexOf(p.desc.supplierCode) == -1)
            groups.push(p.desc.supplierCode);
    });

    let collapsibleItems = groups.map((id) => getCollapsibleItem(id));
    collapsibleItems.forEach((n) => collapsible.appendChild(n));

    page.app.etlContext.registry.forEach((p) => {
        let body = getCollapsibleBody(p, page.app.etlContext.statuses[p.id]);

        let startBtn = body.querySelector('#buttons #start');
        startBtn.addEventListener('click', () => {
            page.paramsModal.open(p);
        });

        let item = collapsible.querySelector(`#${p.desc.supplierCode}`);
        item.querySelector('.collapsible-body').appendChild(body);
    });

    collapsible.querySelectorAll('li').forEach((n) => updateBadges(n));
    M.Collapsible.init(collapsible);
}

class Scenarios extends PageCommon {
    constructor(app, container) {
        super(app, container, 'Scenarios');

        let page = getPage();

        // Params
        this.paramsModal = new ParamsModal(page);

        this.container.append(page);

        updateRegistry(this);

        document.dispatchEvent(new Event('resize'));

        Broadcast.addHandlers(broadcastHandlers);
    }

    destroy() {
        super.destroy();

        Broadcast.removeHandlers(broadcastHandlers);
    }
}

export { Scenarios as module };