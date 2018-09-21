import { htmlToElement } from "../classes/utils.js";
import { Component } from "./Component.js";

function setValue(input, obj, field) {
    obj[field] = input.checked;
}

function getHtml(params) {
    let element = htmlToElement(`
        <div class="switch">
          <label>
            <input type="checkbox">
              <span class="lever"></span>
          </label>
        </div>
    `);

    if (params != null && params.id != null)
        element.id = id;

    return element;
}

class Switch extends Component {
    constructor(parent, params) {
        super(parent, params);

        this.element = parent.appendChild(getHtml(params));
    }

    bind(obj, field) {
        let input = this.element.querySelector('input');
        input.checked = obj[field];
        input.addEventListener('input', () => setValue(input, obj, field));          
    }
}

export { Switch };