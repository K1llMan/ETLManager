﻿var broadcastHandlers = {
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

/*-----------------------------------------------------------------------------
                             ETL Broadcast (jQuery 3.3.1)
-----------------------------------------------------------------------------*/
Broadcast = (function () {
    var socket = null;
    var socketHandlers = null;

    return {
        'getSocket': function() {
            return socket;
        },
        'connect': function (url) {
            if (socket != null) {
                socket.close();
            }

            socket = new WebSocket(url);

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
                var msg = JSON.parse(decodeURI(event.data));

                if (socketHandlers == null)
                    return;

                if (Object.keys(socketHandlers).indexOf(msg.action) != -1)
                    if (socketHandlers[msg.action] != null)
                        socketHandlers[msg.action](msg.data);
            };

            socket.onerror = function (error) {
                console.log("Ошибка " + error.message);
            };
        },
        'send': function (data) {
            if (socket == null || socket.readyState === socket.CLOSED)
                return;

            socket.send(encodeURI(JSON.stringify(data)));
        },
        'setHandlers': function (handlers) {
            if (handlers == null)
                return;
            socketHandlers = handlers;
        }
    };
})();