import { ETLApp } from "./classes/ETLApp.js";

// These are called on page load
document.addEventListener('DOMContentLoaded', function () {
    let app = new ETLApp();
    app.init();
    app.readyForDisplay = false;

    setTimeout(() => { app.readyForDisplay = true; }, 1000);

    document.app = app;
});