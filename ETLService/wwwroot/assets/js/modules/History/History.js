import * as utils from "../../classes/utils.js";
import { Broadcast } from "../../classes/Broadcast.js";
import { Request } from "../../classes/Request.js";
import { PageCommon } from "../PageCommon.js";

import * as components from "../../components/components.js";

let broadcastHandlers = {};

function getDuration(startData, endDate) {
    let start = new Date(startData);
    let end = new Date(endDate);
    let duration = end - start;

    var milliseconds = parseInt((duration % 1000) / 100),
        seconds = ('' + parseInt((duration / 1000) % 60)).padStart(2, '0'),
        minutes = ('' + parseInt((duration / (1000 * 60)) % 60)).padStart(2, '0'),
        hours = ('' + parseInt((duration / (1000 * 60 * 60)) % 24)).padStart(2, '0');

    return [hours, minutes, seconds].join(':') + '.' + milliseconds;
}

function formatDate(date) {
    let d = new Date(date);
    let month = ('' + (d.getMonth() + 1)).padStart(2, '0');
    let day = ('' + d.getDate()).padStart(2, '0');
    let year = d.getFullYear();

    let hours = ('' + d.getHours()).padStart(2, '0');
    let minutes = ('' + d.getMinutes()).padStart(2, '0');
    let second = ('' + d.getSeconds()).padStart(2, '0');

    return [year, month, day].join('-') + ' ' + [hours, minutes, second].join(':');
}

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
                            cell.innerHTML = formatDate(data.pumpstartdate);
                        }
                    },
                    'pumpfinishdate': {
                        'header': 'Ended at',
                        'tooltip': 'Ended at',
                        'hidden': false,
                        'editable': false,
                        'sortable': true,
                        'init': function (cell, data) {
                            cell.innerHTML = formatDate(data.pumpfinishdate);
                        }
                    },
                    'duration': {
                        'header': 'Duration',
                        'tooltip': 'Duration',
                        'hidden': false,
                        'editable': false,
                        'sortable': false,
                        'init': function (cell, data) {
                            cell.innerHTML = getDuration(data.pumpstartdate, data.pumpfinishdate);
                        }
                    },
                    'operations': {
                        'header': 'Operations',
                        'tooltip': 'Operations',
                        'hidden': false,
                        'editable': true,
                        'sortable': false,
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