// Registers the service worker before Blazor starts and records when a newer version is waiting,
// so the UpdatePrompt component can offer a reload once it has rendered. The worker never takes
// over on its own: it waits until the person chooses to reload (see service-worker.published.js).
(() => {
    if (!("serviceWorker" in navigator)) {
        return;
    }

    const stateKey = "__dayPulseUpdate";
    const updateCheckIntervalMs = 60 * 60 * 1000;
    const existingState = window[stateKey];
    const state = existingState && typeof existingState === "object"
        ? existingState
        : {};

    state.waitingWorker = state.waitingWorker ?? null;
    state.applying = false;
    state.reloading = false;
    state.onUpdateAvailable = state.onUpdateAvailable ?? null;
    window[stateKey] = state;

    function markWaiting(worker) {
        state.waitingWorker = worker;
        if (typeof state.onUpdateAvailable === "function") {
            state.onUpdateAvailable();
        }
    }

    function trackInstalling(registration) {
        const installing = registration.installing;
        if (!installing) {
            return;
        }

        installing.addEventListener("statechange", () => {
            // Without a controller this is the first install, not an update.
            if (installing.state === "installed" && navigator.serviceWorker.controller) {
                markWaiting(installing);
            }
        });
    }

    // Reload only after the person asked for the update; the first install also fires
    // controllerchange when the new worker claims the page, and that must not reload.
    navigator.serviceWorker.addEventListener("controllerchange", () => {
        if (state.applying && !state.reloading) {
            state.reloading = true;
            window.location.reload();
        }
    });

    navigator.serviceWorker
        .register("service-worker.js", { updateViaCache: "none" })
        .then(registration => {
            if (registration.waiting && navigator.serviceWorker.controller) {
                markWaiting(registration.waiting);
            }

            registration.addEventListener("updatefound", () => trackInstalling(registration));

            const checkForUpdate = () => registration.update().catch(() => { });
            document.addEventListener("visibilitychange", () => {
                if (document.visibilityState === "visible") {
                    checkForUpdate();
                }
            });
            window.setInterval(checkForUpdate, updateCheckIntervalMs);
        })
        .catch(error => {
            console.warn("DayPulse service worker registration failed.", error);
        });
})();
