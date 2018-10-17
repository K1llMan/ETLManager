import { htmlToElement } from "../classes/utils.js";
import { Component } from "./Component.js";

import * as components from "./components.js";


/*-----------------------------------------------------------------------------
                             (jQuery 3.3.1)
-----------------------------------------------------------------------------*/
/*  Options example:
    {
        'tableHeader': 'Testing',
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
                'afterEdit': function(row) {
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
        'hideSelection': false,
        'getData': function(pageInfo) {
            var response = {
                'total': 100,
                'page': page,
                'pageSize': pageSize,
                'pageCount': 10,
                'orderBy': 'id',
                'orderDir': 'desc',
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
        },
        'beforeDelete': function(rows) {

        }
    };
*/
/*
(function ($) {
    function getTable() {
        table.getSelected = function() {
            return this.find('tbody tr:visible').has('input:checked');
        }

        table.removeSelected = function() {
            var rowsData = Array.from(this.getSelected().map(function (i, row) {
                return opt.data.rows[parseInt($(row).attr('num'), 10)];
            }));

            this.find('thead input').prop('checked', false);
            this.find('thead input').prop('indeterminate', false);

            if (rowsData.length == 0)
                return;

            if (opt.beforeDelete)
                opt.beforeDelete(rowsData);

            Materialize.toast(rowsData.length + ' rows removed.', 3000);
            this.getData();
        }

        datatable.append(table);
        datatable.table = table;        
    }
*/

function getDialog() {
    let dialog = htmlToElement(`
        <div class="z-depth-1 row" id="editDialog">
            <div class="input-field col s12">
                <textarea id="editArea" class="materialize-textarea" length="120"></textarea>
                <label for="editArea"></label>
            </div>
            <a id="btnSave" class="right waves-effect teal-text btn-flat">Save</a>
            <a id="btnCancel" class="right waves-effect teal-text btn-flat">Cancel</a>
            <a id="btnClear" class="right waves-effect teal-text btn-flat">Clear</a>
        </div>
    `);

    dialog.hide = (hide) => dialog.classList.toggle('hide', hide);

    dialog.querySelector('#btnClear').addEventListener('click', () => {
        dialog.querySelector('#editArea').value = '';
        dialog.querySelector('#editArea').blur();
    });

    dialog.querySelector('#btnCancel').addEventListener('click', () => {
        dialog.hide(true);
    });

    dialog.hide(true);

    return dialog;
}

function getHtml(params) {
    let element = htmlToElement(`
    <div class="datatable z-depth-1">
        <div class="header"></div>
        <div class="progress hide">
            <div class="indeterminate"></div>
        </div>
        <div class="data">
            <table>
                <thead>
                    <tr>
                    </tr>
                </thead>
                <tbody>
                </tbody>
            </table>
        </div>
        <div class="footer">
            <label style="margin-left: auto;">Rows per page:</label>
            <label class="counter"></label>
            <div class="pagination">
                <a id="page-left" class="disabled"><i class="material-icons waves-effect">chevron_left</i></a>
                <a id="page-right" class="disabled"><i class="material-icons waves-effect">chevron_rights</i></a>
            </div>
        </div>
    </div>
    `);

    if (params.id != null)
        element.id = id;

    return element;
}

function getOptions(options) {
    let defaultOptions = {
        'tableHeader': '',
        'fields': {
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
        'hideSelection': false,
        'getData': function (pageInfo) {

        },
        'beforeDelete': function (rows) {

        }
    };

    // Update options
    if (options)
        Object.keys(options).forEach((key) => {
            if (Object.keys(defaultOptions).indexOf(key) != -1)
                defaultOptions[key] = options[key];
        });

    return defaultOptions;
}

function initHeader(element) {
    let header = element.querySelector('.header');
    header.setCaption = (caption) => header.innerHTML = caption;
    header.hide = (hide) => header.classList.toggle('hide', hide);

    element.header = header;
}

function initProgress(element) {
    let progress = element.querySelector('.progress');

    progress.hide = (hide) => progress.classList.toggle('hide', hide);
    element.progress = progress;
}

function addCheckBox(id, table) {
    let td = document.createElement('td');
    td.classList.add('select');
    let check = new components.Checkbox(td, { 'id': id, 'desc': '' });
    check.addEventListener('change', () => {
        let headerCheck = table.querySelector('#headerCheck input');

        let count = table.querySelectorAll('tbody input').length;
        let selectedCount = table.querySelectorAll('tbody input:checked').length;

        headerCheck.indeterminate = false;
        if (selectedCount == count)
            headerCheck.checked = true;
        else if (selectedCount == 0)
            headerCheck.checked = false;
        else
            headerCheck.indeterminate = true;
    });

    return td;
}

function getColumnHeader(id, field) {
    let cell = htmlToElement(`
        <th id="${id}" ${field.sortable ? 'class="clickable"' : ''}>
            <div class="valign-wrapper"><i class="material-icons tiny"></i>${field.header}</div>
        </th>`);

    return cell;
}

function setHeaderSorting(header, dir) {
    if (header == null)
        return;

    switch (dir) {
        case 'asc':
            header.innerHTML = 'arrow_downward';
            break;
        case 'desc':
            header.innerHTML = 'arrow_upward';
            break;
        default:
            header.innerHTML = '';
            break;
    }    
}

function updateHeader(head, key, data) {
    let orderBy = key;

    if (orderBy != data.orderBy) {
        [...head.querySelectorAll('i')].forEach((i) => setHeaderSorting(i, ''));
        setHeaderSorting(head.querySelector(`#${key} i`), 'asc');

        data.orderBy = orderBy;
        data.orderDir = 'asc';
    }
    else{
        switch (data.orderDir) {
            case 'asc':
                data.orderDir = 'desc';
                break;
            case 'desc':
                data.orderBy = '';
                data.orderDir = '';
                break;
        }

        setHeaderSorting(head.querySelector(`#${key} i`), data.orderDir);
    }
}

function initTable(element) {
    let table = element.querySelector('.data');
    table.setHeaders = () => {
        let head = table.querySelector('thead tr');
        let checkParent = document.createElement('th');
        checkParent.classList.add('select');
        head.appendChild(checkParent);

        let check = new components.Checkbox(checkParent, { 'id': 'headerCheck', 'desc': '' });
        check.addEventListener('change', () => {
            table.querySelectorAll('tbody input').forEach((i) => i.checked = check.checked);
        });

        Object.keys(element.options.fields).forEach((key) => {
            let field = element.options.fields[key];
            let cell = getColumnHeader(key, field);
            head.appendChild(cell);

            if (!field.sortable)
                return;

            cell.addEventListener('click', () => {
                updateHeader(head, key, element.options.data);

                element.update();
            });
        });

        if (element.options.data.orderBy != '')
            setHeaderSorting(head.querySelector(`#${element.options.data.orderBy} i`), element.options.data.orderDir);
    }

    table.setRows = () => {
        let opt = element.options;

        let body = table.querySelector('tbody');
        body.innerHTML = '';
        for (let i = 0; i < opt.data.pageSize; i++) {
            let tableRow = document.createElement('tr');
            tableRow.classList.toggle('hide', true);
            tableRow.setAttribute('num', i);
            tableRow.appendChild(addCheckBox(`check_${i}`, table));

            Object.keys(opt.fields).forEach((key) => {
                let cell = document.createElement('td');
                cell.id = key;

                let field = opt.fields[key];
                if (field && field.editable) {
                    cell.classList.toggle('editable');
                    cell.addEventListener('click', () => {
                        let dialog = element.editDialog;

                        // Set position and header
                        dialog.querySelector('label').innerHTML = field.header;
                        dialog.style.left = `${cell.offsetLeft + cell.offsetParent.offsetLeft}px`;
                        dialog.style.top = `${cell.offsetTop + cell.offsetParent.offsetTop}px`;

                        // Init input area
                        let area = dialog.querySelector('#editArea');
                        area.val = cell.innerHTML;
                        area.dispatchEvent(new Event('autoresize'));

                        // Init character counter, size must be always set
                        area.setAttribute('length', field.size);
                        //M.CharacterCounter(area);

                        // Set save handler
                        let btnSave = dialog.querySelector('#btnSave');
                        let btnSaveClone = btnSave.cloneNode(true);

                        btnSave.parentNode.replaceChild(btnSaveClone, btnSave);
                        
                        btnSaveClone.addEventListener('click', () => {
                            let value = area.value;
                            if (value.length > field.size) {
                                M.Toast('Value is too long', 3000);
                                return;
                            }

                            let rowNum = parseInt(cell.closest('tr').getAttribute('num'), 10);
                            let rowData = opt.data.rows[rowNum];
                            if (rowData) {
                                rowData[key] = area.value;
                                if (field.afterEdit)
                                    field.afterEdit(rowData);
                            }

                            cell.innerHTML = area.value;

                            dialog.hide();
                        });

                        dialog.hide(false);
                        area.focus();
                    });
                }

                tableRow.appendChild(cell);
            });

            body.appendChild(tableRow);
        }

        table.hideSelection(opt.hideSelection);
        table.hideColumns(Object.keys(opt.fields).filter(key => opt.fields[key].hidden));
    }

    table.fill = (data) => {
        let opt = element.options;
        opt.data.rows = data.rows;

        let body = table.querySelector('tbody');
        opt.data.rows.forEach((row, i) => {
            let tableRow = body.querySelector(`tr:nth-child(${i + 1})`);
            if (tableRow == null)
                return;

            tableRow.style.borderBottom = '';
            tableRow.classList.toggle('hide', false);

            Object.keys(opt.fields).forEach((key) => {
                let cell = tableRow.querySelector(`#${key}`);
                let field = opt.fields[key];
                if (field.init && !field.editable)
                    opt.fields[key].init(cell, row);
                else
                    if (row[key])
                        cell.innerHTML = row[key];
            });
        });
        
    }

    table.hideSelection = (hide) => {
        [...table.querySelectorAll('.select')]
            .forEach((s) => s.classList.toggle('hide', hide));
    };

    table.hideColumns = (columns) => {
        [...table.querySelectorAll('th:not(.select),td:not(.select)')]
            .forEach((c) => c.classList.toggle('hide', false));

        columns.forEach((key) => {
            [...table.querySelectorAll(`#${key}`)]
                .forEach((c) => c.classList.toggle('hide', true));
        });
    }

    element.table = table;
}

function initFooter(element) {
    let footer = element.querySelector('.footer');
    let select = new components.Select(footer, {
        'options': [
            {
                'value': 10,
                'text': '10'
            },
            {
                'value': 25,
                'text': '25'
            },
            {
                'value': 50,
                'text': '50'
            }
        ]
    });

    select.bind(element.options.data, 'pageSize');
    select.addEventListener('change', () => element.update());

    footer.insertBefore(footer.querySelector('select'), footer.querySelector('label.counter'));

    // Counter
    let counter = footer.querySelector('label.counter');
    counter.update = (data) => {
        let end = (data.page - 1) * data.pageSize + data.rows.length;
        let start = end - data.rows.length + (data.rows.length > 0 ? 1 : 0);

        counter.innerHTML = `${start}-${end} of ${data.total}`;
    }

    footer.counter = counter;

    // Navigation
    footer.querySelector('#page-left').addEventListener('click', () => {
        if (element.options.data.page > 1) {
            element.options.data.page--;
            element.update();
        }
    });

    footer.querySelector('#page-right').addEventListener('click', () => {
        if (element.options.data.page < element.options.data.pageCount) {
            element.options.data.page++;
            element.update();
        }
    });

    footer.hide = (hide) => footer.classList.toggle('hide', hide);

    element.footer = footer;
}


function update(element) {
    let opt = element.options;

    // Fill rows set
    element.table.setRows();

    if (opt.getData) {
        element.progress.hide(false);

        // Clear rows data before request new
        opt.data.rows = [];
        opt.getData(opt.data)
            .then((data) => {
                element.table.fill(data);
                element.footer.counter.update(data);

                element.footer.querySelector('#page-left').classList.toggle('disabled', data.page == 1);
                element.footer.querySelector('#page-right').classList.toggle('disabled', data.page == data.pageCount);
                element.table.querySelector(`tr:nth-child(n+${data.rows.length})`).style.borderBottom = 'none';

                element.progress.hide(true);
            });
    }
}

class Datatable extends Comment {
    constructor(parent, params) {
        super(parent, params);

        this.element = parent.appendChild(getHtml(params));
        this.element.options = getOptions(params.options);

        this.element.update = () => update(this.element);

        this.element.editDialog = this.element.appendChild(getDialog());

        this.init();

        this.element.header.setCaption(this.element.options.tableHeader);
        this.element.header.hide(this.element.options.hideHeader);

        this.element.table.setHeaders();

        this.element.footer.hide(this.element.options.hideFooter);

        this.element.update();
    }

    init() {
        initHeader(this.element);
        initProgress(this.element);
        initTable(this.element);
        initFooter(this.element);
    }

    update() {
        this.element.update();
    }
}

export { Datatable };