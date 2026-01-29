window.ShowModal = function (modalId) {
    try {
        const el = document.getElementById(modalId);
        if (!el) return;
        // Use Bootstrap's JS API to show modal (getOrCreateInstance for v5+)
        const m = bootstrap.Modal.getOrCreateInstance(el);
        m.show();
    } catch (e) {
        console && console.error && console.error('ShowModal error', e);
    }
};

window.HideModal = function (modalId) {
    try {
        const el = document.getElementById(modalId);
        if (!el) return;
        const m = bootstrap.Modal.getOrCreateInstance(el);
        m.hide();
    } catch (e) {
        console && console.error && console.error('HideModal error', e);
    }
};
