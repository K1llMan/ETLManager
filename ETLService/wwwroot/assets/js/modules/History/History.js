import * as utils from "../../classes/utils.js";
import { Broadcast } from "../../classes/Broadcast.js";
import { PageCommon } from "../PageCommon.js";

import * as components from "../../components/components.js";

function getPage() {
    return utils.htmlToElement(`
      <div class="history-page">
      </div>
    `);
};

const broadcastHandlers = {
    'startPump': (data) => {

    },
    'endPump': (data) => {

    },
    'receiveUpdate': (data) => {
    },
    'update': (data) => {

    }
}

class History extends PageCommon {
    constructor(app, container) {
        super(app, container, 'History');

        let page = getPage();
        this.container.append(page);
        this.datatable = new components.Datatable(page, {
            'options': {
                'tableHeader': 'Test header',
                'hideHeader': false,
                'fields': {
                    'col1': {
                        'header': 'Column 1',
                        'tooltip': 'Column 1',
                        'hidden': false,
                        'editable': false
                    },
                    'col2': {
                        'header': 'Column 2',
                        'tooltip': 'Column 2',
                        'hidden': false,
                        'editable': false
                    },
                    'col3': {
                        'header': 'Column 3',
                        'tooltip': 'Column 3',
                        'hidden': false,
                        'editable': false,
                        'init': function (cell, data) {

                        }
                    },
                    'col4': {
                        'header': 'Column 4',
                        'tooltip': 'Column 4',
                        'hidden': false,
                        'editable': true,
                        'size': 1000,
                        'afterEdit': function (row) {
                            alert(row);
                        }
                    }                    
                }
            }
        });

        document.dispatchEvent(new Event('resize'));

        Broadcast.addHandlers(broadcastHandlers);
    }

    destroy() {
        super.destroy();

        Broadcast.removeHandlers(broadcastHandlers);
    }
}

export { History as module };