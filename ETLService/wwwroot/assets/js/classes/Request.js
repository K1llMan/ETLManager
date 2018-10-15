let handlers = [];

class Request {
    static send(url, data) {
        let curInfo = {
            'method': 'GET',
            'headers': {}
        };

        if (data != null && data.info != null)
            Object.keys(data.info).forEach((k) => {
                curInfo[k] = data.info[k];
            });

        handlers.forEach((h) => h(curInfo));

        return fetch(url, curInfo)
            .then((response) => response.json());
    }

    static addInfoHandler(f) {
        if (handlers.indexOf(f) == -1)
            handlers.push(f);
    }
}

export { Request };