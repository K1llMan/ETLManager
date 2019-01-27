import * as utils from "../../classes/utils.js";
import { Broadcast } from "../../classes/Broadcast.js";
import { Request } from "../../classes/Request.js";
import { PageCommon } from "../PageCommon.js";
import { LogTable } from "./LogTable.js";

import * as components from "../../components/components.js";

let broadcastHandlers = {};

function getPage() {
    return utils.htmlToElement(`
      <div class="test-page">
      </div>
    `);
};

class Test extends PageCommon {
    constructor(app, container) {
        super(app, container, 'Test');

        let page = getPage();
        this.container.append(page);

        this.table = new LogTable(page);
        this.table.getLog("24");

        broadcastHandlers = {
        };

        Broadcast.addHandlers(broadcastHandlers);
    }

    destroy() {
        super.destroy();

        Broadcast.removeHandlers(broadcastHandlers);
    }
}

export { Test as module };