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
            'rows': [],
        },
        'hideHeader': false,
        'hideFooter': false,
        'hideSelection': false,
        'getData': function(page, pageSize, sort, sortDir, updateData) {
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

            updateData(response);
        },
        'beforeDelete': function(rows) {

        }
    };
*/
/*
(function ($) {
    var datatable;
    var opt = {
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
        'getData': function(page, pageSize, sort, sortDir, updateData) {

        },
        'beforeDelete': function(rows) {
            
        }
    };

    function getTable() {
        var table = $('<div class="data"><table><thead><tr></tr></thead><tbody></tbody></table></div>');
        table.setHeaders = function() {
            var head = this.find('thead tr');
            head.append(addHeaderCheckBox());
            $.each(Object.keys(opt.fields), function (i, el) {
                var cell = $('<th></th>');
                cell.attr('id', el);
                cell.html(opt.fields[el].header);

                head.append(cell);
            });
        }

        table.setRows = function() {
            var body = this.find('tbody');
            body.html('');
            for (var i = 0; i < opt.data.pageSize; i++) {
                var tableRow = $('<tr></tr>');
                tableRow.attr('num', i);
                tableRow.append(addCheckBox('check_' + i));

                $.each(Object.keys(opt.fields), function (i, key) {
                    var cell = $('<td></td>');
                    cell.attr('id', key);
                    cell.html();

                    var field = opt.fields[key];

                    if (field && field.editable) {
                        cell.toggleClass('editable');

                        cell.click(function() {
                            var dialog = datatable.editDialog;
                            // Set position and header
                            dialog.find('label').html(field.header);
                            dialog.css(cell.offset());

                            // Init input area
                            var area = dialog.find('#editArea');
                            area.val(cell.html());
                            area.trigger('autoresize');

                            // Init character counter, size must be always set
                            area.attr('length', field.size);
                            area.characterCounter();

                            dialog.show();
                            area.focus();

                            // Set save handler
                            var btnSave = dialog.find('#btnSave');
                            btnSave.off('click');
                            btnSave.click(function () {
                                var value = area.val();
                                if (value.length > field.size) {
                                    Materialize.toast('Value is too long', 3000);
                                    return;
                                }

                                var rowNum = parseInt(cell.closest('tr').attr('num'), 10);
                                var rowData = opt.data.rows[rowNum];
                                rowData[key] = area.val();
                                cell.html(rowData[key]);

                                if (field.afterEdit)
                                    field.afterEdit(rowData);

                                dialog.hide();
                            });
                        });
                    }

                    tableRow.append(cell);
                });

                body.append(tableRow);
            }

            table.hideColumns();
            table.hideSelection();
        }

        table.updateData = function (data) {
            opt.data = data;

            // Update counters
            datatable.footer.counter.update();

            datatable.footer.find("#page-left").toggleClass('disabled', opt.data.page == 1);
            datatable.footer.find("#page-right").toggleClass('disabled', opt.data.page == opt.data.pageCount);

            // Fill rows set
            table.setRows();

            // Fill rows
            var body = table.find('tbody');
            $.each(opt.data.rows, function (i, row) {
                var tableRow = body.find('tr:nth-child(' + (i + 1) + ')');
                if (tableRow.length == 0)
                    return;

                tableRow.css({ 'borderBottom': '' });
                tableRow.show();

                $.each(Object.keys(opt.fields), function (i, key) {
                    var field = opt.fields[key];
                    var cell = tableRow.find('#' + key);
                    if (field.init && !field.editable)
                        opt.fields[key].init(cell, row);
                    else
                        cell.html(row[key]);
                });
            });

            //body.find('tr:nth-child(' + (opt.data.rows.length) + ')').css({ 'borderBottom': 'none' });
            body.find('tr:nth-child(n+' + (opt.data.rows.length + 1) + ')').hide();
        }

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

        table.getData = function() {
            if (!opt.getData)
                return;

            opt.getData(opt.data.page, opt.data.pageSize, null, 'asc', table.updateData);
        }

        table.hideColumns = function() {
            // Show all columns
            this.find('th:not(.select),td:not(.select)').show();

            // Hide selected
            $.each(Object.keys(opt.fields), function (i, key) {
                if (!opt.fields[key].hidden)
                    return;

                var cols = datatable.find('table #' + key);
                cols.hide();
            });            
        }

        table.hideSelection = function() {
            var column = this.find('table .select');
            if (opt.hideSelection)
                column.hide();
            else
                column.show();            
        }

        datatable.append(table);
        datatable.table = table;        
    }

    function getFooter() {
        var footer = $('<div class="footer"><label style="margin-left: auto;">Rows per page:</label></div>');

        var pageSizeControl = $('<select class="browser-default">' +
                '<option value="10" selected>10</option>' + 
                '<option value="25">25</option>' + 
                '<option value="50">50</option>' + 
            '</select>');
        footer.append(pageSizeControl);
        footer.find('select').change(function() {
            opt.data.pageSize = parseInt(this.value, 10);
            datatable.table.getData();
        });

        var counter = $('<label></label>');
        counter.update = function () {
            var end = (opt.data.page - 1) * opt.data.pageSize + opt.data.rows.length;
            var start = end - opt.data.rows.length + (opt.data.rows.length > 0 ? 1 : 0);

            counter.html(start + '-' + end + ' of ' + opt.data.total);
        };

        footer.append(counter);
        footer.counter = counter;

        var pageControl = $('<div class="pagination"></div>');
        var left = $('<a id="page-left" class="disabled"><i class="material-icons waves-effect">chevron_left</i></a>');
        left.click(function () {
            if (opt.data.page > 1) {
                opt.data.page--;
                datatable.table.getData();
            }
        });

        var right = $('<a id="page-right" class="disabled"><i class="material-icons waves-effect">chevron_rights</i></a>');
        right.click(function () {
            if (opt.data.page < opt.data.pageCount) {
                opt.data.page++;
                datatable.table.getData();
            }
        });

        pageControl.append(left, right);
        footer.append(pageControl);

        footer.hide = function(hide) {
            this.toggleClass('hide', hide);
        }

        datatable.append(footer);
        datatable.footer = footer;
    }

    function init() {
        getTable();
        getFooter();
    }

    $.fn.datatable = function (options) {
        datatable = this;

        init();

        datatable.table.setHeaders();
        datatable.table.hideColumns();

        datatable.table.hideSelection();

        datatable.footer.hide(opt.hideFooter);

        datatable.table.getData();

        return datatable;
    };
})(jQuery);
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

    return dialog;
}

function getHtml(params) {
    let element = htmlToElement(`
    <div class="datatable z-depth-1">
        <div class="header"></div>
        <div class="data">
            <table>
                <thead>
                    <tr>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td class="select">
                            <label>
                                <input type="checkbox" />
                                <span></span>
                            </label>
                        </td>                        
                        <td>data 1</td>
                        <td>data 2</td>
                        <td>data 3</td>
                        <td>data 4 hjg h gyyf fg lfuyf gfkuyf uyfyufuyr6r hgftk eydtd te tkfuf 6r uf tukrf</td>
                    </tr>
                </tbody>
            </table>
        </div>
        <div class="footer">
          <label style="margin-left: auto;">Rows per page:</label>
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
        'getData': function (page, pageSize, sort, sortDir, updateData) {

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
            let cell = document.createElement('th');
            cell.id = key;
            cell.innerHTML = element.options.fields[key].header;

            head.appendChild(cell);
        });
    }

    table.setRows = () => {
        let opt = element.options;

        let body = table.querySelector('tbody');
        body.innerHTML = '';
        for (let i = 0; i < opt.data.pageSize; i++) {
            let tableRow = document.createElement('tr');
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
    }

    element.table = table;
}

function initFooter(element) {
    let footer = element.querySelector('.footer');
    let select = new components.Select(footer,{
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
}


function update(element) {
    element.table.setRows();
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

        this.element.update();
    }

    init() {
        // Clear html struct
        initHeader(this.element);
        initTable(this.element);
        initFooter(this.element);

        /*
        datatable.html('');
        getHeader();
        getTable();
        getFooter();
        getEditDialog();
        */
    }
}

export { Datatable };