/**
 * Cloning contents from a &lt;template&gt; element is more performant
 * than using innerHTML because it avoids addtional HTML parse costs.
 */
const template = document.createElement('template');
template.innerHTML = `
    <link type="text/css" rel="stylesheet" href="../css/materialize.min.css" media="screen,projection">
    <link type="text/css" rel="stylesheet" href="css/styles.css" media="screen,projection">
    <p>
      <label>
          <input type="checkbox" />
          <span>{{desc}}</span>
      </label>
    </p>
  `;

// HIDE
// ShadyCSS will rename classes as needed to ensure style scoping.
//ShadyCSS.prepareTemplate(template, 'howto-checkbox');

class Checkbox extends HTMLElement {
    static get observedAttributes() {
        return ['checked', 'disabled'];
    }

    /**
     * The element's constructor is run anytime a new instance is created.
     * Instances are created either by parsing HTML, calling
     * document.createElement('howto-checkbox'), or calling new HowToCheckbox();
     * The construtor is a good place to create shadow DOM, though you should
     * avoid touching any attributes or light DOM children as they may not
     * be available yet.
     */
    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.appendChild(template.content.cloneNode(true));
    }

    /**
     * `connectedCallback()` fires when the element is inserted into the DOM.
     * It's a good place to set the initial `role`, `tabindex`, internal state,
     * and install event listeners.
     */
    connectedCallback() {
        // HIDE
        // Shim Shadow DOM styles. This needs to be run in `connectedCallback()`
        // because if you shim Custom Properties (CSS variables) the element
        // will need access to its parent node.
        //ShadyCSS.styleElement(this);
        // /HIDE

        if (!this.hasAttribute('role'))
            this.setAttribute('role', 'checkbox');
        if (!this.hasAttribute('tabindex'))
            this.setAttribute('tabindex', 0);

        // A user may set a property on an _instance_ of an element,
        // before its prototype has been connected to this class.
        // The `_upgradeProperty()` method will check for any instance properties
        // and run them through the proper class setters.
        // See the [lazy properites](/web/fundamentals/architecture/building-components/best-practices#lazy-properties)
        // section for more details.
        this._upgradeProperty('checked');
        this._upgradeProperty('disabled');

        //this.addEventListener('keyup', this._onKeyUp);
        this.addEventListener('click', this._onClick);
    }

    _upgradeProperty(prop) {
        if (this.hasOwnProperty(prop)) {
            let value = this[prop];
            delete this[prop];
            this[prop] = value;
        }
    }

    /**
     * `disconnectedCallback()` fires when the element is removed from the DOM.
     * It's a good place to do clean up work like releasing references and
     * removing event listeners.
     */
    disconnectedCallback() {
        this.removeEventListener('keyup', this._onKeyUp);
        this.removeEventListener('click', this._onClick);
    }

    /**
     * Properties and their corresponding attributes should mirror one another.
     * The property setter for `checked` handles truthy/falsy values and
     * reflects those to the state of the attribute. See the [avoid
     * reentrancy](/web/fundamentals/architecture/building-components/best-practices#avoid-reentrancy)
     * section for more details.
     */
    set checked(value) {
        const isChecked = Boolean(value);
        if (isChecked)
            this.setAttribute('checked', '');
        else
            this.removeAttribute('checked');
    }

    get checked() {
        return this.hasAttribute('checked');
    }

    set disabled(value) {
        const isDisabled = Boolean(value);
        if (isDisabled)
            this.setAttribute('disabled', '');
        else
            this.removeAttribute('disabled');
    }

    get disabled() {
        return this.hasAttribute('disabled');
    }

    /**
     * `attributeChangedCallback()` is called when any of the attributes in the
     * `observedAttributes` array are changed. It's a good place to handle
     * side effects, like setting ARIA attributes.
     */
    attributeChangedCallback(name, oldValue, newValue) {
        const hasValue = newValue !== null;
        switch (name) {
            case 'checked':
                this.setAttribute('aria-checked', hasValue);
                break;
            case 'disabled':
                this.setAttribute('aria-disabled', hasValue);
                // The `tabindex` attribute does not provide a way to fully remove
                // focusability from an element.
                // Elements with `tabindex=-1` can still be focused with
                // a mouse or by calling `focus()`.
                // To make sure an element is disabled and not focusable, remove the
                // `tabindex` attribute.
                if (hasValue) {
                    this.removeAttribute('tabindex');
                    // If the focus is currently on this element, unfocus it by
                    // calling the `HTMLElement.blur()` method.
                    this.blur();
                } else {
                    this.setAttribute('tabindex', '0');
                }
                break;
        }
    }

    _onKeyUp(event) {
        // Don’t handle modifier shortcuts typically used by assistive technology.
        if (event.altKey)
            return;

        switch (event.keyCode) {
            case KEYCODE.SPACE:
                event.preventDefault();
                this._toggleChecked();
                break;
            // Any other key press is ignored and passed back to the browser.
            default:
                return;
        }
    }

    _onClick(event) {
        this._toggleChecked();
    }

    /**
     * `_toggleChecked()` calls the `checked` setter and flips its state.
     * Because `_toggleChecked()` is only caused by a user action, it will
     * also dispatch a change event. This event bubbles in order to mimic
     * the native behavior of `<input type=checkbox>`.
     */
    _toggleChecked() {
        if (this.disabled)
            return;
        this.checked = !this.checked;
        this.dispatchEvent(new CustomEvent('change', {
            detail: {
                checked: this.checked,
            },
            bubbles: true,
        }));
    }
}

export { Checkbox };