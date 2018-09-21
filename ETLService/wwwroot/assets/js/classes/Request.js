let handlers = [];

class Request {
    static send(url, data) {
        let curInfo = {
            'method': 'GET',
            'headers': {}
        };

        if (data.info != null)
            Object.keys(data.info).forEach((k) => {
                curInfo[k] = data.info[k];
            });

        handlers.forEach((h) => h(curInfo));

        fetch(url, curInfo)
            .then((response) => response.json())
            .then((d) => {
                if (data.success != null)
                    data.success(d);
            })
            .catch((d) => {
                if (data.error != null)
                    data.error(d);
            });
    }

    static addInfoHandler(f) {
        if (handlers.indexOf(f) == -1)
            handlers.push(f);
    }
}

export { Request };