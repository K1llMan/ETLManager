import * as utils from "../../classes/utils.js";
import { Broadcast } from "../../classes/Broadcast.js";
import { Request } from "../../classes/Request.js";
import { PageCommon } from "../PageCommon.js";

import * as components from "../../components/components.js";

let broadcastHandlers = {};

function getPage() {
    return utils.htmlToElement(`
      <div class="history-page">
      </div>
    `);
};

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
                        'sortable': true,
                        'init': function (cell, data) {

                            let iconStr = 'remove';
                            let color = '';
                            switch (data.status) {
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

                            cell.innerHTML = `<i class="material-icons ${color}">${iconStr}</i>`;
                        }
                    },
                    'id': {
                        'header': 'SessionID',
                        'tooltip': 'SessionID',
                        'hidden': false,
                        'editable': false,
                        'sortable': true
                    },
                    'programid': {
                        'header': 'Scenario Name',
                        'tooltip': 'Scenario Name',
                        'hidden': false,
                        'editable': false,
                        'sortable': true,
                        'init': function (cell, data) {
                            let pump = document.app.etlContext.registry.find((p) => p.id == data.programid);

                            cell.innerHTML = `${pump.desc.name}`;
                        }
                    },
                    'programversion': {
                        'header': 'Version',
                        'tooltip': 'Version',
                        'hidden': false,
                        'editable': false,
                        'sortable': true
                    },
                    'username': {
                        'header': 'Executed by',
                        'tooltip': 'Executed by',
                        'hidden': false,
                        'editable': false,
                        'sortable': true
                    },
                    'pumpstartdate': {
                        'header': 'Started at',
                        'tooltip': 'Started at',
                        'hidden': false,
                        'editable': false,
                        'sortable': true,
                        'init': function (cell, data) {
                            cell.innerHTML = utils.formatDate(data.pumpstartdate);
                        }
                    },
                    'pumpfinishdate': {
                        'header': 'Ended at',
                        'tooltip': 'Ended at',
                        'hidden': false,
                        'editable': false,
                        'sortable': true,
                        'init': function (cell, data) {
                            cell.innerHTML = utils.formatDate(data.pumpfinishdate);
                        }
                    },
                    'duration': {
                        'header': 'Duration',
                        'tooltip': 'Duration',
                        'hidden': false,
                        'editable': false,
                        'sortable': false,
                        'init': function (cell, data) {
                            cell.innerHTML = utils.getDuration(data.pumpstartdate, data.pumpfinishdate);
                        }
                    },
                    'operations': {
                        'header': 'Operations',
                        'tooltip': 'Operations',
                        'hidden': false,
                        'editable': false,
                        'sortable': false,
                        'init': function (cell, data) {
                            cell.innerHTML =`
                                <div>
                                    <i id="history" class="clickable material-icons">view_headline</i>
                                    <i id="restart" class="clickable material-icons green-text">replay</i>
                                </div>
                            `;

                            cell.querySelector('#restart').addEventListener('click', () => {
                                Request.send(`api/pumps/${data.programid}/restart/${data.id}`);
                            });
                        }
                    }
                },
                'data': {
                    'total': 100,
                    'page': 1,
                    'pageSize': 10,
                    'pageCount': 1,
                    'orderBy': 'id',
                    'orderDir': 'desc',
                    'rows': [],
                },
                'hideHeader': false,
                'hideFooter': false,
                'hideSelection': true,
                'getData': function (pageInfo) {
                    return Request.send('api/pumps/history', {
                        'info': {
                            'method': 'POST',
                            'headers': {
                                'Accept': 'application/json',
                                'Content-Type': 'application/json'
                            },
                            'body': JSON.stringify(pageInfo)
                        }
                    });
                },
                'beforeDelete': function (rows) {

                }
            }
        });

        document.dispatchEvent(new Event('resize'));

        broadcastHandlers = {
            'endPump': () => {
                this.datatable.update();
            }
        };

        Broadcast.addHandlers(broadcastHandlers);
    }

    destroy() {
        super.destroy();

        Broadcast.removeHandlers(broadcastHandlers);
    }
}

export { History as module };