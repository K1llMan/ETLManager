function htmlToElement(html) {
    let template = document.createElement('template');
    html = html.trim(); // Never return a text node of whitespace as the result
    template.innerHTML = html;
    return template.content.childElementCount > 1
        ? template.content
        : template.content.firstElementChild;
}

export { htmlToElement };