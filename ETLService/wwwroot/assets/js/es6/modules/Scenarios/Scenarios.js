import { htmlToElement } from "../../classes/utils.js"
import { PageCommon } from "../PageCommon.js";
import { ParamsModal } from "./ParamsModal.js";

const page = htmlToElement(`
  <!-- Updates modal -->
  <div id="update-modal" class="modal">
    <div class="modal-content">
      <h4>Updates list</h4>
      <div class="row">
        <ul class="collection">
          <li class="collection-item">Alvin</li>
          <li class="collection-item">Alvin</li>
          <li class="collection-item">Alvin</li>
          <li class="collection-item">Alvin</li>
        </ul>
      </div>
    </div>
    <div class="modal-footer">
      <a class="modal-close waves-effect waves-red btn-flat">Close</a>
      <a id="runUpdate" class="modal-close waves-effect waves-green btn-flat">Update</a>
    </div>
  </div>
  
  <div class="scenarios-page">
    <ul class="collapsible">
    </ul>    
  </div>
  <div class="fixed-action-btn">
    <a id="updatesBtn" class="btn-floating btn-large pulse <!--hide-->">
      <i class="large material-icons">autorenew</i>
    </a>
  </div>`);

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

function setStatus(icon, status) {
    // Remove colors
    icon.className.replace(/[\s](.+)?-text/g, '');

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

function updatePumpModal(pump) {
    let modal = document.querySelector('#params-modal');
    M.Modal.getInstance(modal).open();
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
        super(app, container);

        // Params
        this.paramsModal = new ParamsModal(page);

        this.container.append(page);

        updateRegistry(this);

        document.dispatchEvent(new Event('resize'));
        M.Modal.init(document.querySelector('.modal'));
    }
}

export { Scenarios as module };