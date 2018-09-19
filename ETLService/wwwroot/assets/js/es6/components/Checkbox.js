import { htmlToElement } from "../classes/utils.js";
import { Component } from "./Component.js";

function setValue(input, obj, field) {
    obj[field] = input.checked;
}

function getHtml(params) {
    let element = htmlToElement(`
      <p>
        <label>
          <input type="checkbox" />
          <span>${params.desc}</span>
        </label>
      </p>
    `);

    if (params.id != null)
        element.id = id;

    return element;
}

class Checkbox extends Component {
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

export { Checkbox };