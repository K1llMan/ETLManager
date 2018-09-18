import { htmlToElement } from "../../classes/utils.js";
import * as components from "../../components/components.js";

const modal = htmlToElement(`
<!-- Params modal -->
<div id="params-modal" class="modal">
    <div class="modal-content">
        <h4>Scenario name</h4>
        <div class="row">
        <h5>Parameters</h5>
        <div class="col s3">
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
            let stageID = a.id;
            a.addEventListener('click', () => {
                stagesContainer.querySelectorAll('a').forEach((s) => s.classList.remove('active'));
                a.classList.add('active');

                //initParams
            });
        });

        M.Modal.getInstance(this.modal).open();

        /*
            $(Templater.useTemplate('stage-item',
            Object.keys(config.stages).map(function (key) {
                var stage = config.stages[key];
                stage.id = key;
                return stage;
            })));
        

        stagesContainer.append(stagesList);
        
        // Switching between params lists
        stagesContainer.find('a').each(function (i, a) {
            var stageId = $(a).attr('id');

            $(a).click(function () {
                stagesContainer.find('a').removeClass('active');
                $(a).addClass('active');
                initParams(stageId == 'common'
                    ? config.commonParams
                    : config.stages[stageId].params);
            });

            // Changing stage status
            var sw = $(a).find('input');
            if (sw.length == 0)
                return;

            bindValue(sw, config.stages[stageId], 'enabled');
        });

        var clcFunc = function () {
            runPump(config);
        }
        var runBtn = modal.find('#runPump');
        runBtn.click(clcFunc);

        // Remove run handler
        modal.modal({
            'onCloseEnd': function () {
                runBtn.off('click', clcFunc);
            }
        });

        modal.modal('open');
        stagesContainer.find('#common').click();  
        */
    }
}

export { ParamsModal };