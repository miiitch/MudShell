// MudShell layout helpers: split-pane drag resize and localStorage persistence.

export function load(key) {
    try {
        return window.localStorage.getItem(key);
    } catch {
        return null;
    }
}

export function save(key, value) {
    try {
        window.localStorage.setItem(key, value);
    } catch {
        // storage unavailable (private mode, blocked): persistence is best-effort
    }
}

// Pointer-drag resize of the side pane (anchored on the right edge, so dragging left grows it).
// Updates the width live on the pane element and reports the final width once on release.
export function initResize(handle, pane, min, max, dotnet) {
    let startX = 0;
    let startWidth = 0;
    let dragging = false;

    const clamp = (w) => Math.max(min, Math.min(max, w));

    const onMove = (e) => {
        if (!dragging) return;
        const width = clamp(startWidth + (startX - e.clientX));
        pane.style.width = `${width}px`;
    };

    const onUp = (e) => {
        if (!dragging) return;
        dragging = false;
        handle.classList.remove('mds-split-dragging');
        document.body.style.userSelect = '';
        try { handle.releasePointerCapture(e.pointerId); } catch { /* already released */ }
        handle.removeEventListener('pointermove', onMove);
        handle.removeEventListener('pointerup', onUp);
        handle.removeEventListener('pointercancel', onUp);
        dotnet.invokeMethodAsync('OnResizeEnd', Math.round(pane.getBoundingClientRect().width));
    };

    const onDown = (e) => {
        if (e.button !== 0 || e.target.closest('button')) return;
        dragging = true;
        startX = e.clientX;
        startWidth = pane.getBoundingClientRect().width;
        handle.classList.add('mds-split-dragging');
        document.body.style.userSelect = 'none';
        try { handle.setPointerCapture(e.pointerId); } catch { /* synthetic or inactive pointer */ }
        handle.addEventListener('pointermove', onMove);
        handle.addEventListener('pointerup', onUp);
        handle.addEventListener('pointercancel', onUp);
        e.preventDefault();
    };

    handle.addEventListener('pointerdown', onDown);

    return {
        dispose: () => {
            handle.removeEventListener('pointerdown', onDown);
            handle.removeEventListener('pointermove', onMove);
            handle.removeEventListener('pointerup', onUp);
            handle.removeEventListener('pointercancel', onUp);
            document.body.style.userSelect = '';
        },
    };
}
