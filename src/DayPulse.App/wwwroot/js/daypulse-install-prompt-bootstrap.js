(() => {
    const stateKey = "__dayPulseInstallPrompt";
    const existingState = window[stateKey];
    const state = existingState && typeof existingState === "object"
        ? existingState
        : { beforeInstallPromptEvent: null };

    window[stateKey] = state;
    window.addEventListener("beforeinstallprompt", event => {
        event.preventDefault();
        state.beforeInstallPromptEvent = event;
    });
})();
