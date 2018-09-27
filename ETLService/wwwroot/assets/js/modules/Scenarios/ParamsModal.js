import { htmlToElement } from "../../classes/utils.js";
import { Request } from "../../classes/Request.js";

import * as components from "../../components/components.js";

const modal = htmlToElement(`
<!-- Params modal -->
<div id="params-modal" class="modal">
    <div class="modal-content">
        <h4>Scenario name</h4>
        <div class="row">
        <h5>Parameters</h5>
        <div id="stages-container" class="col s3">
            <div id="stages" class="collection">            
            </div>
        </div>
        <div id="params" class="col s9">
        </div>
        </div>
    </div>
    <div class="modal-footer">
        <a class="modal-close waves-effect waves-red btn-flat">Close</a>
        <a id="runPump" class="modal-close waves-effect waves-green btn-flat">Run</a>
    </div>
</div>
`);

function getCommonItem() {
    return htmlToElement(`
        <a id="common" class="collection-item clickable">Common</a>
    `);
}

function getStageItem(stage) {
    return htmlToElement(`
      <a id="${stage.id}" class="collection-item clickable">
        ${stage.name}
      </a>
    `);
}

function runPump(config) {
    Request.send(`api/pumps/${config.id}/execute`, {
        'info': {
            'method': 'POST',
            'headers': {
                'Content-type': 'application/json; charset=utf-8'
            },
            'body': JSON.stringify(config)
        }
    });
}

function initParams(modal, paramGroups) {
    let paramsPanel = modal.querySelector('#params');
    paramsPanel.innerHTML = '';

    if (Object.keys(paramGroups).length == 0)
        return;

    paramGroups.forEach((params) => {
        let form = htmlToElement('<form class="row"></form>');
        paramsPanel.appendChild(form);

        Object.keys(params).forEach((key) => {
            if (params[key].ui == undefined || params[key].ui == null)
                return;

            let component = null;
            switch (params[key].ui.type) {
                case 'check':
                    component = new components.Checkbox(form, params[key].ui);
                    break;
                case 'radio':
                    component = new components.Radio(form, params[key].ui);
                    break;
            }

            if (component != null)
                component.bind(params[key], 'value');
        });
    });
}

class ParamsModal {
    constructor(parent) {
        parent.appendChild(modal);

        this.modal = parent.querySelector('#params-modal');

        M.Modal.init(this.modal, {
            'onCloseStart': function () {

            }
        });
    }

    open(config) {
        this.modal.querySelector("h4").innerHTML = config.desc.name;

        // Remove old data
        let stagesContainer = this.modal.querySelector('#stages');
        stagesContainer.querySelectorAll('a').forEach((s) => s.remove());

        stagesContainer.appendChild(getCommonItem());
        Object.keys(config.stages).forEach((key) => {
            let stage = Object.assign({ 'id': key }, config.stages[key]);
            let stageItem = getStageItem(stage);

            // Changing stage status
            let sw = new components.Switch(stageItem);
            sw.bind(config.stages[key], 'enabled');

            stagesContainer.appendChild(stageItem);
        });

        stagesContainer.querySelectorAll('a').forEach((a) => {
            let stageId = a.id;
            a.addEventListener('click', () => {
                stagesContainer.querySelectorAll('a').forEach((s) => s.classList.remove('active'));
                a.classList.add('active');

                initParams(this.modal, stageId == 'common'
                    ? config.commonParams
                    : config.stages[stageId].params);
            });
        });

        let clcFunc = () => {
            runPump(config);
        }

        modal.querySelector('#runPump').addEventListener('click', clcFunc);

        let instance = M.Modal.init(this.modal, {
            'onCloseStart': function () {
                modal.querySelector('#runPump').removeEventListener('click', clcFunc);
            }
        });

        instance.open();
        stagesContainer.querySelector('#common').dispatchEvent(new Event('click'));
    }
}

export { ParamsModal };