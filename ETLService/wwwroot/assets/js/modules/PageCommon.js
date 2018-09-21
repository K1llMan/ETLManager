class PageCommon {
    constructor(app, container) {
        this.app = app;
        this.container = container;

        container.innerHTML = '';
    }

    destroy() {
        
    }
}

export { PageCommon };