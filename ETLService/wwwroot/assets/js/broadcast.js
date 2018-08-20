/*-----------------------------------------------------------------------------
                             ETL Broadcast (jQuery 3.3.1)
-----------------------------------------------------------------------------*/
Broadcast = function (url, handlers) {
    var socket = new WebSocket(url);
    var socketHandlers = handlers;

    socket.onopen = function () {
        alert("Соединение установлено.");
    };

    socket.onclose = function (event) {
        if (event.wasClean) {
            alert('Соединение закрыто чисто');
        } else {
            alert('Обрыв соединения'); // например, "убит" процесс сервера
        }
        alert('Код: ' + event.code + ' причина: ' + event.reason);
    };

    socket.onmessage = function (event) {
        alert("Получены данные " + decodeURI(event.data));
        //if (Object.keys(socketHandlers).indexOf() != -1)
        //    socketHandlers[]
    };

    socket.onerror = function (error) {
        alert("Ошибка " + error.message);
    };

    return {
        'socket': socket,
        'send': function(data) {
            socket.send(encodeURI(data));
        }
    };
};