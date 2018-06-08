// Template HTML generator
Templater = (function() {
    var templates = null;

    return {
        // Create html element from template
        'useTemplate': function(templateName, data) {
            if (templates == null)
                return;
            
            var template = templates.filter(function(k, v){
                return v.id == templateName;
            });
            
            if (template.length == 0)
                return
            
            template = template[0].innerHTML;

            if (data == null)
                return template;
            
            var i = 0,
                len = data.length,
                html = '';
            // Replace the {% raw %}{{XXX}}{% endraw %} with the corresponding property
            function replaceWithData(data_bit) {
                var html_snippet, prop, regex;
                for (prop in data_bit) {
                    regex = new RegExp('{{' + prop + '}}', 'ig');
                    html_snippet = (html_snippet || template).replace(regex, data_bit[prop]);
                }
                return html_snippet;
            }
            // Go through each element in the array and add the properties to the template
            for (; i < len; i++) {
                html += replaceWithData(data[i]);
            }
            
            // Give back the HTML to be added to the DOM
            return html;
        },
        
        'downloadTemplates': function( path, callback ) {
            $.ajax({ 
                url: path,
                cache: false
            })
            .done(function( data ) {
                templates = $(data).filter(function(key, value){
                    return value.id;
                });
                
                // Continue processing after loading templates
                if (callback)
                    callback();
            });
        }
    };
})()