// Minimal helpers for the letter editor UI
window.letterEditor = {
    // Return innerHTML of an element reference passed from Blazor
    getHtml: function (el) {
        try {
            if (!el) return "";
            return el.innerHTML || "";
        } catch (e) { return ""; }
    },

    // Set HTML content into an element (used by other pages)
    setHtml: function (el, html) {
        try {
            if (!el) return;
            el.innerHTML = html || "";
        } catch (e) { }
    }
};

// Save/generate directly from browser (works even if Blazor circuit is not active)
window.letterEditor.saveAndGenerate = async function (claimNumber, documentNumber, templateName, ruleId) {
    try {
        const el = document.getElementById('editContent');
        if (!el) { alert('Editor not found'); return; }
        const html = el.innerHTML || '';

        // small client log
        try { await fetch('/api/clientlogs', { method: 'POST', body: JSON.stringify({ message: 'saveAndGenerate invoked', claimNumber, documentNumber, templateName, htmlLength: html.length }), headers: { 'Content-Type': 'application/json' } }); } catch { }

        const payload = { claimNumber, documentNumber, templateName, html };
        if (ruleId) payload.ruleId = ruleId;

        const resp = await fetch('/api/letters/form/saveHtml', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
        if (!resp.ok) {
            const txt = await resp.text();
            alert('Generation failed: ' + txt);
            return;
        }
        const obj = await resp.json();
        const url = obj?.url;
        alert('Letter saved please close');
        if (url) window.open(url, '_blank');
        // navigate back to claim page to refresh grid
        window.location.href = '/claim/' + encodeURIComponent(claimNumber);
    }
    catch (ex) {
        alert('Error: ' + (ex?.message || ex));
    }
};

window.letterEditor.cancel = function (claimNumber) {
    try {
        // navigate back to claim page
        window.location.href = '/claim/' + encodeURIComponent(claimNumber);
    } catch { }
};
