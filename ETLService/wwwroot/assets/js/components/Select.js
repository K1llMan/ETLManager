import { htmlToElement } from "../classes/utils.js";
import { Component } from "./Component.js";


function setValue(select, obj, field) {
    obj[field] = select.options[select.selectedIndex].value;
}

function getHtml(params) {
    let element = htmlToElement(`
        <select class="browser-default">
            ${params.options.map((o) => `<option value="${o.value}">${o.text}</option>`)}
        </select>
    `);

    if (params.id != null)
        element.id = id;

    return element;
}

class Select extends Component {
    constructor(parent, params) {
        super(parent, params);

        this.element = parent.appendChild(getHtml(params));
    }

    bind(obj, field) {
        [...this.element.options].some((option, index) => {
            if (option.value == obj[field]) {
                this.element.selectedIndex = index;
                return true;
            }

            return false;
        });

        this.element.addEventListener('change', () => setValue(this.element, obj, field));
    }
}

export { Select };