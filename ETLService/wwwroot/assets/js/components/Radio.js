import { htmlToElement } from "../classes/utils.js";
import { Component } from "./Component.js";

function setValue(input, obj, field) {
    let checked = input.checked;
    obj[field] = checked;
    // Update other params in radio group
    if (checked) {
        let name = input.getAttribute('name');
        let inputs = input.closest('form').querySelectorAll(`input:not(:checked)[name="${name}"]`);
        inputs.forEach((i) => i.dispatchEvent(new Event('input')));
    }
}

function getHtml(params) {
    let element = htmlToElement(`
      <p>
        <label>
          <input class="with-gap" name="group" type="radio"/>
          <span>${params.desc}</span>
        </label>
      </p>
    `);

    if (params.id != null)
        element.id = id;

    return element;
}

class Radio extends Component {
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

export { Radio };