class Component {
    constructor(parent, params) {
        this.parent = parent;
        this.id = null;
        if (params != null)
            this.id = params.id;
    }
}

export { Component }