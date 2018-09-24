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

class UpdatesModal {
    constructor(parent) {
        this.modal = getHtml();
        parent.appendChild(this.modal);

        M.Modal.init(this.modal, { });
    }

    open() {
        M.Modal.getInstance(this.modal).open();
    }
}

export { UpdatesModal };