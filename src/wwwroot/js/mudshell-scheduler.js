// Radzen.Blazor.js is a classic script that only defines window.Radzen, so it is added once as a
// script element rather than imported as a module. Callers await the same promise.
let loading;

export function ensureRadzen(scriptUrl) {
    if (window.Radzen) {
        return Promise.resolve();
    }

    if (!loading) {
        loading = new Promise((resolve, reject) => {
            const script = document.createElement("script");
            script.src = scriptUrl;
            script.onload = resolve;
            script.onerror = () => {
                loading = undefined;
                reject(new Error(`MudShell: could not load ${scriptUrl}`));
            };
            document.head.appendChild(script);
        });
    }

    return loading;
}
