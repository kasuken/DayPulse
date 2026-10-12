const systemThemeQuery = "(prefers-color-scheme: dark)";
const onSystemThemeChangedMethod = "OnSystemThemeChanged";

let dotNetReference = null;
let mediaQueryList = null;
let listenerAttached = false;
let notificationChain = Promise.resolve();
let disposed = false;

async function notifySystemThemeChanged(reference, query) {
    if (disposed
        || dotNetReference !== reference
        || mediaQueryList !== query) {
        return;
    }

    try {
        await reference.invokeMethodAsync(
            onSystemThemeChangedMethod,
            query.matches);
    } catch (error) {
        if (disposed || dotNetReference !== reference) {
            console.debug(
                "DayPulse system theme callback ended after disposal or disconnect.",
                error);
        } else {
            console.error("DayPulse system theme callback failed.", error);
        }
    }
}

function handleSystemThemeChange() {
    const currentReference = dotNetReference;
    const currentQuery = mediaQueryList;
    notificationChain = notificationChain
        .then(() => notifySystemThemeChanged(currentReference, currentQuery))
        .catch((error) => {
            console.error("DayPulse system theme notification failed.", error);
        });
}

function subscribeToSystemTheme() {
    if (mediaQueryList) {
        return;
    }

    if (typeof window.matchMedia !== "function") {
        throw new Error("This browser does not support matchMedia.");
    }

    mediaQueryList = window.matchMedia(systemThemeQuery);
    try {
        if (typeof mediaQueryList.addEventListener === "function") {
            mediaQueryList.addEventListener("change", handleSystemThemeChange);
        } else if (typeof mediaQueryList.addListener === "function") {
            mediaQueryList.addListener(handleSystemThemeChange);
        } else {
            throw new Error("This browser does not support media query change events.");
        }

        listenerAttached = true;
    } catch (error) {
        mediaQueryList = null;
        listenerAttached = false;
        throw error;
    }
}

function unsubscribeFromSystemTheme() {
    if (!mediaQueryList) {
        return;
    }

    if (listenerAttached && typeof mediaQueryList.removeEventListener === "function") {
        mediaQueryList.removeEventListener("change", handleSystemThemeChange);
    } else if (listenerAttached && typeof mediaQueryList.removeListener === "function") {
        mediaQueryList.removeListener(handleSystemThemeChange);
    }

    mediaQueryList = null;
    listenerAttached = false;
}

function resolveDarkMode(mode) {
    if (mode === "Dark") {
        return true;
    }

    if (mode === "Light") {
        return false;
    }

    if (mode === "System" && mediaQueryList) {
        return mediaQueryList.matches;
    }

    throw new Error(`Unknown DayPulse theme mode: ${mode}`);
}

function applyMode(mode) {
    unsubscribeFromSystemTheme();

    if (mode === "System") {
        subscribeToSystemTheme();
    }

    return resolveDarkMode(mode);
}

export function initialize(mode, reference) {
    disposed = false;
    dotNetReference = reference;
    return applyMode(mode);
}

export function setMode(mode) {
    if (disposed) {
        throw new Error("DayPulse theme module has been disposed.");
    }

    return applyMode(mode);
}

export async function dispose() {
    disposed = true;
    unsubscribeFromSystemTheme();
    dotNetReference = null;
    await notificationChain;
}
