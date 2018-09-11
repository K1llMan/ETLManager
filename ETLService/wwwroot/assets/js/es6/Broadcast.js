class Broadcast {
    constructor() {
        this.socket = null;
        this.socketHandlers = null;
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
            'func': 'callingFunc',
            'data': {
                'd1': 'my very useful data'
            }
        }
        */
        socket.onmessage = function (event) {
            console.log("Получены данные " + decodeURI(event.data));
            var data = JSON.parse(decodeURI(event.data));

            if (socketHandlers == null)
                return;

            if (Object.keys(socketHandlers).indexOf(data.Action) != -1)
                if (socketHandlers[data.Action] != null)
                    socketHandlers[data.Action](data.Data);
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