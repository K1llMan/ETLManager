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

export { htmlToElement, addLink };