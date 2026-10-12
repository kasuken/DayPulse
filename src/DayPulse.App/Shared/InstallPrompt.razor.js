const dismissalStorageKey = "daypulse.install-prompt.dismissed.v1";
const earlyPromptStateKey = "__dayPulseInstallPrompt";

let beforeInstallPromptEvent = null;
let dotNetReference = null;
let listenerAttached = false;

function handleBeforeInstallPrompt(event) {
    event.preventDefault();
    beforeInstallPromptEvent = event;
    void notifyBeforeInstallPromptAvailable();
}

function ensureBeforeInstallPromptListener() {
    if (listenerAttached) {
        return;
    }

    window.addEventListener("beforeinstallprompt", handleBeforeInstallPrompt);
    listenerAttached = true;
}

function consumeEarlyPromptEvent() {
    const earlyPromptState = window[earlyPromptStateKey];
    if (!earlyPromptState?.beforeInstallPromptEvent) {
        return;
    }

    beforeInstallPromptEvent = earlyPromptState.beforeInstallPromptEvent;
    earlyPromptState.beforeInstallPromptEvent = null;
}

async function notifyBeforeInstallPromptAvailable() {
    const currentReference = dotNetReference;
    if (!currentReference) {
        return;
    }

    try {
        await currentReference.invokeMethodAsync("OnBeforeInstallPromptAvailable");
    } catch (error) {
        // A circuit can disconnect between the browser event and the .NET callback.
        if (dotNetReference === currentReference) {
            console.debug("DayPulse install prompt callback ended before the circuit was ready.", error);
        }
    }
}

function isStandalone() {
    try {
        return window.matchMedia("(display-mode: standalone)").matches
            || navigator.standalone === true;
    } catch {
        return navigator.standalone === true;
    }
}

function getPlatformState() {
    const userAgent = navigator.userAgent ?? "";
    const platform = navigator.platform ?? "";
    const isIos = /iPad|iPhone|iPod/i.test(userAgent)
        || (platform === "MacIntel" && navigator.maxTouchPoints > 1);
    const isIosSafari = isIos
        && /Safari/i.test(userAgent)
        && !/CriOS|FxiOS|EdgiOS|OPiOS|GSA/i.test(userAgent);
    const isAndroid = /Android/i.test(userAgent);

    return {
        isIosSafari,
        isAndroid,
        isMobile: isIos || isAndroid
    };
}

function isDismissed() {
    try {
        return window.localStorage.getItem(dismissalStorageKey) === "true";
    } catch {
        // Private browsing and browser policy can make localStorage unavailable.
        return false;
    }
}

export function initialize(reference) {
    dotNetReference = reference;
    ensureBeforeInstallPromptListener();
    consumeEarlyPromptEvent();

    const platformState = getPlatformState();
    return {
        isStandalone: isStandalone(),
        isIosSafari: platformState.isIosSafari,
        isAndroid: platformState.isAndroid,
        isMobile: platformState.isMobile,
        isDismissed: isDismissed(),
        canPrompt: beforeInstallPromptEvent !== null
    };
}

export async function promptInstall() {
    const promptEvent = beforeInstallPromptEvent;
    beforeInstallPromptEvent = null;

    if (!promptEvent) {
        return {
            wasAvailable: false,
            wasAccepted: false
        };
    }

    try {
        await promptEvent.prompt();
        const choice = await promptEvent.userChoice;
        return {
            wasAvailable: true,
            wasAccepted: choice?.outcome === "accepted"
        };
    } catch (error) {
        console.debug("DayPulse native install prompt was unavailable.", error);
        return {
            wasAvailable: false,
            wasAccepted: false
        };
    }
}

export function dismiss(remember) {
    if (!remember) {
        return;
    }

    try {
        window.localStorage.setItem(dismissalStorageKey, "true");
    } catch {
        // The component still dismisses for this render when browser storage is unavailable.
    }
}

export function dispose() {
    if (listenerAttached) {
        window.removeEventListener("beforeinstallprompt", handleBeforeInstallPrompt);
        listenerAttached = false;
    }

    beforeInstallPromptEvent = null;
    dotNetReference = null;
}

ensureBeforeInstallPromptListener();
