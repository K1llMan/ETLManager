function setStatus(icon, status) {
    // Remove colors
    $(icon).attr('class', icon.attr('class').replace(/[\s](.+)?-text/g, ''));

    var iconStr = 'remove';
    var color = '';
    switch (status) {
        case "Running":
            iconStr = 'trending_flat';
            break;
        case 'Successful':
            iconStr = 'check';
            color = 'green-text';
            break;
        case 'Warnings':
            iconStr = 'error_outline';
            color = 'orange-text';
            break;
        case 'Errors':
            iconStr = 'error';
            color = 'red-text';
            break;
        case "Terminated":
            iconStr = 'clear';
            break;
        case 'None':
            iconStr = 'remove';
            break;
    }

    $(icon).html(iconStr);
    $(icon).addClass(color);
}

function updateBadges(header) {
    var statuses = $(header).find("#status");
    var running = statuses.filter(function (i, s) {
        return $(s).html() == 'trending_flat';
    }).length;

    var warnings = statuses.filter(function (i, s) {
        return $(s).html() == 'error_outline';
    }).length;

    var errors = statuses.filter(function (i, s) {
        return $(s).html() == 'error';
    }).length;

    $(header).find(".badge.green").toggleClass('hide', running < 1).html(running);
    $(header).find(".badge.orange").toggleClass('hide', warnings < 1).html(warnings);
    $(header).find(".badge.red").toggleClass('hide', errors < 1).html(errors);
}

