import * as utils from "../../classes/utils.js";
import { Request } from "../../classes/Request.js";

import * as components from "../../components/components.js";

function getTable() {
    return utils.htmlToElement(`
    <div class="log z-depth-1">
        <table>
            <thead>
                <tr>
                    <th>Steps</th>
                    <th></th>
                    <th>Status</th>
                    <th>Message</th>
                    <th>Start</th>
                    <th>End</th>
                </tr>
            </thead>
            <tbody>
            </tbody>
        </table>
    </div>
    `);
};

function getKind(kind) {
    if (kind == null)
        return '';

    let iconStr = 'remove';
    let color = '';
    switch (kind) {
        case "Информация":
            iconStr = 'info_outline';
            break;
        case 'Предупреждение':
            iconStr = 'warning';
            color = 'orange-text';
            break;
        case 'Ошибка':
            iconStr = 'error';
            color = 'red-text';
            break;
        case "Критическая ошибка":
            iconStr = 'error';
            color = 'red-text text-darken-4';
            break;
    }

    return `<i class="material-icons ${color}">${iconStr}</i>`;
}

function getRow(data, num, parentNum) {
    return utils.htmlToElement(`
        <tr id="row_${num}"${parentNum == 0 ? '' : ` parent="row_${parentNum}`}">
            <td>${data.module}</td>
            <td class="arrow"><i class="material-icons clickable">expand_more</i></td>
            <td class="kind">${getKind(data.kind)}</td>
            <td><pre>${data.message}</pre></td>
            <td>${utils.formatDate(data.start)}</td>
            <td>${utils.formatDate(data.end)}</td>
        </tr>
    `);
}

function addChildren(table, info, data) {
    let parentNum = info.num;

    data.forEach((r) => {
        table.appendChild(getRow(r, ++info.num, parentNum));
        if (r.children)
            addChildren(table, info, r.children);
    });
}

function fillTable(table, sessNo) {
    table.querySelector('tbody').innerHTML = '';

    Request.send(`api/pumps/log/${sessNo}`)
        .then((data) => {
            addChildren(table.querySelector('tbody'), { 'num': 0, 'parent': 0 }, data.data);
            initExpand(table);
        });
}

function initExpand(table) {
    let body = table.querySelector('tbody');

    let rows = [...body.querySelectorAll('tr')];
    rows.forEach((r) => {
        let parent = body.querySelector(`#${r.id}`);
        let padding = parseInt(parent.querySelector('td').style.paddingLeft.toString().replace('px', '')) || 20;

        let arrow = r.querySelector('i');
        let expRows = [...body.querySelectorAll(`tr[parent="${r.id}"]`)];
        if (expRows.length == 0) {
            arrow.innerHTML = '';
            return;
        }

        r.collapse = (parentHide) => {
            let arrow = r.querySelector('i');
            if (arrow.innerHTML == '')
                return;

            let hide = arrow.innerHTML == "expand_less" || parentHide;
            arrow.innerHTML = hide ? "expand_more" : "expand_less";

            [...body.querySelectorAll(`tr[parent="${r.id}"]`)].forEach((er) => {
                er.classList.toggle('hide', hide);
                if (hide && er.collapse)
                    er.collapse(hide);
            });
        }

        expRows.forEach((er) => {
            er.classList.toggle('hide', true);
            er.querySelector('td').style.paddingLeft = `${padding + 10}px`;
        });

        arrow.addEventListener('click', () => {
            r.collapse();
        });
    });
}

class LogTable  {
    constructor(parent) {
        this.table = getTable();
        parent.appendChild(this.table);
    }

    getLog(sessNo) {
        fillTable(this.table.querySelector('.log table'), sessNo);
    }
}

export { LogTable };