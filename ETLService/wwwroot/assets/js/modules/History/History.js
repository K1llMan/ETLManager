import { htmlToElement } from "../../classes/utils.js";
import { Broadcast } from "../../classes/Broadcast.js";
import { PageCommon } from "../PageCommon.js";

function getPage() {
    return htmlToElement(`
      <div class="history-page">
      </div>
    `);
};

const broadcastHandlers = {
    'startPump': (data) => {

    },
    'endPump': (data) => {

    },
    'receiveUpdate': (data) => {
    },
    'update': (data) => {

    }
}

class History extends PageCommon {
    constructor(app, container) {
        super(app, container, 'History');

        let page = getPage();
        this.container.append(page);

        document.dispatchEvent(new Event('resize'));

        Broadcast.addHandlers(broadcastHandlers);
    }

    destroy() {
        super.destroy();

        Broadcast.removeHandlers(broadcastHandlers);
    }
}

export { History as module };