let socketHandlers = [];

class Broadcast {
    constructor() {
        this.socket = null;
    }

    connect(url) {
        if (this.socket != null)
            this.socket.close();

        let socket = new WebSocket(url);

        socket.onopen = function () {
            console.log("Соединение установлено.");
        };

        socket.onclose = function (event) {
            if (event.wasClean) {
                console.log('Соединение закрыто чисто');
            } else {
                console.log('Обрыв соединения'); // например, "убит" процесс сервера
            }
            console.log('Код: ' + event.code + ' причина: ' + event.reason);
        };

        /*
        {
            'action': 'callingFunc',
            'data': {
                'd1': 'my very useful data'
            }
        }
        */
        socket.onmessage = function (event) {
            console.log("Получены данные " + decodeURI(event.data));
            let msg = JSON.parse(decodeURI(event.data));

            if (socketHandlers == null)
                return;

            socketHandlers.forEach((h) => {
                if (Object.keys(h).indexOf(msg.action) != -1)
                    if (h[msg.action] != null)
                        h[msg.action](msg.data);                
            });
        };

        socket.onerror = function (error) {
            console.log("Ошибка " + error.message);
        };

        this.socket = socket;
    }

    send(data) {
        if (this.socket == null || this.socket.readyState === socket.CLOSED)
            return;

        this.socket.send(encodeURI(JSON.stringify(data)));        
    }

    static addHandlers(handlers) {
        socketHandlers.push(handlers);
    }
}

export { Broadcast };

/*
var broadcastHandlers = {
    'startPump': function (data) {
        var icon = $('#' + data.id + ' #info .material-icons');
        setStatus(icon, "Running");

        updateBadges(icon.parents('li'));
    },
    'endPump': function (data) {
        var icon = $('#' + data.id + ' #info .material-icons');
        setStatus(icon, data.status);

        updateBadges(icon.parents('li'));
    },
    'receiveUpdate': function (data) {
        etlContext.updates[data.ProgramID] = data;

        $('#updatesBtn').toggleClass('hide', etlContext.updates.length < 1);
    },
    'update': function(data) {
        
    }
}
*/