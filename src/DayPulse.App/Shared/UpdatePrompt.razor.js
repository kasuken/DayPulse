const stateKey = "__dayPulseUpdate";

function getState() {
    const state = window[stateKey];
    return state && typeof state === "object" ? state : null;
}

export function initialize(reference) {
    const state = getState();
    if (!state) {
        return false;
    }

    state.onUpdateAvailable = () => {
        reference.invokeMethodAsync("OnUpdateAvailable").catch(error => {
            // A callback can arrive while the component is being torn down.
            console.debug("DayPulse update callback ended before the component was ready.", error);
        });
    };

    return state.waitingWorker !== null;
}

export function applyUpdate() {
    const state = getState();
    const waitingWorker = state?.waitingWorker;
    if (!state || !waitingWorker) {
        window.location.reload();
        return;
    }

    state.applying = true;
    waitingWorker.postMessage({ type: "SKIP_WAITING" });
}

export function dispose() {
    const state = getState();
    if (state) {
        state.onUpdateAvailable = null;
    }
}
