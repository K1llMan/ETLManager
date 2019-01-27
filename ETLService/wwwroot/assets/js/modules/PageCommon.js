import { addLink } from "../classes/utils.js"

class PageCommon {
    constructor(app, container, name) {
        console.log(`Module "${name}" loaded.`);

        this.app = app;
        this.container = container;
        this.container.innerHTML = '';

        if (name == null) {
            console.log('Empty module name.');
            return;
        }

        this.name = name;

        this.cssLink = addLink(`assets/css/modules/${this.name}.css`);
    }

    destroy() {
        if (this.cssLink != null)
            document.head.removeChild(this.cssLink);
    }
}

export { PageCommon };