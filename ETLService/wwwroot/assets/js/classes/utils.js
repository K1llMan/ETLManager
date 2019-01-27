function htmlToElement(html) {
    let template = document.createElement('template');
    html = html.trim(); // Never return a text node of whitespace as the result
    template.innerHTML = html;
    return template.content.childElementCount > 1
        ? template.content
        : template.content.firstElementChild;
}

function addLink(path) {
    let link = document.createElement('link');
    link.rel = 'stylesheet';
    link.type = 'text/css';
    link.href = path;
    link.media = 'screen,projection';

    document.head.appendChild(link);

    return link;
}

function formatDate(date) {
    let d = new Date(date);
    let month = ('' + (d.getMonth() + 1)).padStart(2, '0');
    let day = ('' + d.getDate()).padStart(2, '0');
    let year = d.getFullYear();

    let hours = ('' + d.getHours()).padStart(2, '0');
    let minutes = ('' + d.getMinutes()).padStart(2, '0');
    let second = ('' + d.getSeconds()).padStart(2, '0');

    return [year, month, day].join('.') + ' ' + [hours, minutes, second].join(':');
}

function getDuration(startDate, endDate) {
    let start = new Date(startDate);
    let end = new Date(endDate);
    let duration = end - start;

    var milliseconds = parseInt((duration % 1000) / 100),
        seconds = ('' + parseInt((duration / 1000) % 60)).padStart(2, '0'),
        minutes = ('' + parseInt((duration / (1000 * 60)) % 60)).padStart(2, '0'),
        hours = ('' + parseInt((duration / (1000 * 60 * 60)) % 24)).padStart(2, '0');

    return [hours, minutes, seconds].join(':') + '.' + milliseconds;
}

export { htmlToElement, addLink, formatDate, getDuration };