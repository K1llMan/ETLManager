import * as utils from "../../classes/utils.js";
import { Broadcast } from "../../classes/Broadcast.js";
import { Request } from "../../classes/Request.js";
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
                'tableHeader': 'Pumps history',
                'fields': {
                    'status': {
                        'header': 'Status',
                        'tooltip': 'Status',
                        'hidden': false,
                        'editable': false,
                        'init': function (cell, data) {

                        }
                    },
                    'id': {
                        'header': 'SessionID',
                        'tooltip': 'SessionID',
                        'hidden': false,
                        'editable': false
                    },
                    'programid': {
                        'header': 'Scenario Name',
                        'tooltip': 'Scenario Name',
                        'hidden': false,
                        'editable': false
                    },
                    'programversion': {
                        'header': 'Version',
                        'tooltip': 'Version',
                        'hidden': false,
                        'editable': false
                    },
                    'username': {
                        'header': 'Executed by',
                        'tooltip': 'Executed by',
                        'hidden': false,
                        'editable': false
                    },
                    'pumpstartdate': {
                        'header': 'Started at',
                        'tooltip': 'Started at',
                        'hidden': false,
                        'editable': false
                    },
                    'pumpfinishdate': {
                        'header': 'Ended at',
                        'tooltip': 'Ended at',
                        'hidden': false,
                        'editable': false
                    },
                    'duration': {
                        'header': 'Duration',
                        'tooltip': 'Duration',
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
                },
                'data': {
                    'total': 100,
                    'page': 1,
                    'pageSize': 10,
                    'pageCount': 1,
                    'rows': [],
                },
                'hideHeader': false,
                'hideFooter': false,
                'hideSelection': true,
                'getData': function (page, pageSize, sort, sortDir) {
                    return Request.send('api/pumps/history', {
                        'info': {
                            'method': 'POST'
                        }
                    });

                    var response = {
                        'total': 100,
                        'page': page,
                        'pageSize': pageSize,
                        'pageCount': 10,
                        'rows': [
                            { 'col1': 'data41', 'col2': 'data41', 'col3': 'data41', 'col4': 'data51', 'col5': 'ololo', 'col6': '434525', 'col7': 'Column 7', 'col8': 'Column 8', 'col9': 'Column 9', 'col10': 'Column 10 dfgsdg sdg sdg  sdgsdgwegsd segsd gsdg serg sdgwsegsdfg sergsdg segsdfgdLorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.' },
                            { 'col1': 'data1', 'col2': 'data2', 'col3': 'data3' },
                            { 'col1': 'data41', 'col2': 'data41', 'col3': 'data41' },
                            { 'col1': 'data11', 'col2': 'data21', 'col3': 'data31' },
                            { 'col1': 'data41', 'col2': 'data41', 'col3': 'data41' },
                            { 'col1': 'data12', 'col2': 'data22', 'col3': 'data32' },
                            { 'col1': 'data41', 'col2': 'data41', 'col3': 'data41' }
                        ]
                    }

                    return response;
                },
                'beforeDelete': function (rows) {

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