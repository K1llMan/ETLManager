import { htmlToElement } from "./utils.js";

function getHtml() {
    return htmlToElement(`
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
    `);
}

function getChip(tag) {
    return htmlToElement(`<div class="chip">${tag}</div>`);
}

function getUpdateRecord(update, config) {
    let record = htmlToElement(`
        <li class="collection-item flow-right update-record">
            <label>
                <input type="checkbox" />
                <span></span>
            </label>
            <div>
                <div>${config == null ? '0000' : config.desc.dataCode} ${config == null ? 'New pump' : config.desc.name}</div>
                <div>
                PumpIdentifier: ${update.programID}
                </div>
                <div class="chips-container"></div>
            </div>
        </li>
    `);

    ['new', 'module', 'config'].map((tag) => getChip(tag))
        .forEach((t) => record.querySelector('.chips-container').appendChild(t));

    return record;
}

class UpdatesModal {
    constructor(parent) {
        this.modal = getHtml();
        parent.appendChild(this.modal);

        M.Modal.init(this.modal, { });
    }

    open(context) {
        let collection = this.modal.querySelector('.collection');
        collection.innerHTML = '';

        Object.keys(context.updates).forEach((key) => {
            let update = context.updates[key];
            let config = context.registry.find((c) => c.id == update.programID);
            collection.appendChild(getUpdateRecord(update, config));
        });

        M.Modal.getInstance(this.modal).open();
    }
}

export { UpdatesModal };