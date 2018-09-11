import { ETLApp } from "./ETLApp.js";

// These are called on page load
document.addEventListener('DOMContentLoaded', function () {
    let app = new ETLApp();
    app.init();

    document.app = app;
});