class Component {
    constructor(parent, params) {
        this.parent = parent;
        this.id = null;
        if (params != null)
            this.id = params.id;
    }

    addEventListener(event, funct) {
        if (this.element == null)
            return;

        this.element.addEventListener(event, funct);
    }

    removeEventListener(event, funct) {
        if (this.element == null)
            return;

        this.element.removeEventListener(event, funct);        
    }
}

export { Component }