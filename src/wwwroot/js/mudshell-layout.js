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

// ── Responsive shell tiers ──────────────────────────────────────────────────
// Breakpoints mirror MdsAppShell.razor.css: mobile < 600px, tablet 600-1279px, desktop >= 1280px.
const MOBILE_QUERY = '(max-width: 599.98px)';
const TABLET_QUERY = '(min-width: 600px) and (max-width: 1279.98px)';

// Reports the current tier ('desktop' | 'tablet' | 'mobile') to .NET now and on every change.
// Returns { dispose } to stop listening.
export function observeShellMode(dotnet, callbackName) {
    const mobile = window.matchMedia(MOBILE_QUERY);
    const tablet = window.matchMedia(TABLET_QUERY);
    const report = () => {
        const mode = mobile.matches ? 'mobile' : tablet.matches ? 'tablet' : 'desktop';
        dotnet.invokeMethodAsync(callbackName, mode);
    };
    mobile.addEventListener('change', report);
    tablet.addEventListener('change', report);
    report();
    return {
        dispose: () => {
            mobile.removeEventListener('change', report);
            tablet.removeEventListener('change', report);
        },
    };
}

// ── Modal focus management (overlay / bottom sheet) ─────────────────────────
const FOCUSABLE = 'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), '
    + 'textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

// Moves focus into `container` (which must be focusable, e.g. tabindex="-1"), keeps Tab / Shift+Tab
// cycling inside it, and remembers the element that had focus. Returns { release } which restores
// that element's focus.
export function trapFocus(container) {
    const previous = document.activeElement;

    const onKeyDown = (e) => {
        if (e.key !== 'Tab') return;
        const items = Array.from(container.querySelectorAll(FOCUSABLE));
        if (items.length === 0) {
            e.preventDefault();
            return;
        }
        const first = items[0];
        const last = items[items.length - 1];
        const active = document.activeElement;
        const outside = !container.contains(active);
        if (e.shiftKey && (active === first || active === container || outside)) {
            e.preventDefault();
            last.focus();
        } else if (!e.shiftKey && (active === last || outside)) {
            e.preventDefault();
            first.focus();
        }
    };

    container.addEventListener('keydown', onKeyDown);
    container.focus({ preventScroll: true });

    return {
        release: () => {
            container.removeEventListener('keydown', onKeyDown);
            if (previous && typeof previous.focus === 'function') previous.focus({ preventScroll: true });
        },
    };
}
