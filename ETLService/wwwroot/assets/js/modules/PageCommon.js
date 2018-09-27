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

        this.cssLink = document.createElement('link');
        this.cssLink.rel = 'stylesheet';
        this.cssLink.type = 'text/css';
        this.cssLink.href = `assets/css/modules/${this.name}.css`;
        this.cssLink.media = 'screen,projection';

        document.head.appendChild(this.cssLink);
    }

    destroy() {
        if (this.cssLink != null)
            document.head.removeChild(this.cssLink);
    }
}

export { PageCommon };