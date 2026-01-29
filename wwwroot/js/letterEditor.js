window.letterEditor = {
    // Make a document editable while sanitizing input (plain-text only)
    makeDocEditable: function (doc) {
        if (!doc) return;
        try {
            doc.body.contentEditable = true;
            try { doc.title = doc.title || 'Editable Letter'; } catch (e) { }

            // Paste handler: insert plain text only
            var onPaste = function (ev) {
                try {
                    ev.preventDefault();
                    var text = (ev.clipboardData || window.clipboardData).getData('text/plain');
                    if (!text) return;
                    var sel = doc.getSelection && doc.getSelection();
                    if (!sel || !sel.rangeCount) {
                        try { doc.execCommand('insertText', false, text); } catch (e) {
                            var tn = doc.createTextNode(text);
                            doc.body.appendChild(tn);
                        }
                    } else {
                        var range = sel.getRangeAt(0);
                        // letterEditor functionality removed — inline editing and iframe helpers disabled.
                        window.letterEditor = {};
                        range.insertNode(node);
