function readBrowserLocales() {
    const locales = [];
    const browserNavigator = typeof navigator === "undefined"
        ? null
        : navigator;
    if (!browserNavigator) {
        return locales;
    }

    const preferredLocales = Array.isArray(browserNavigator.languages)
        ? browserNavigator.languages
        : [];

    for (const locale of preferredLocales) {
        if (typeof locale === "string" && locale.trim().length > 0) {
            locales.push(locale);
        }
    }

    if (typeof browserNavigator.language === "string"
        && browserNavigator.language.trim().length > 0
        && !locales.includes(browserNavigator.language)) {
        locales.push(browserNavigator.language);
    }

    return locales;
}

export function getBrowserLocales() {
    return readBrowserLocales();
}

export function setDocumentLanguage(cultureName) {
    if (typeof document !== "undefined"
        && document.documentElement
        && typeof cultureName === "string"
        && cultureName.length > 0) {
        document.documentElement.lang = cultureName;
    }
}

export function setBootstrapErrorText(unhandledError, reload, errorBoundary) {
    if (typeof document === "undefined") {
        return;
    }

    const unhandledErrorElement = document.getElementById("blazor-error-message");
    if (unhandledErrorElement && typeof unhandledError === "string") {
        unhandledErrorElement.textContent = unhandledError;
    }

    const reloadElement = document.getElementById("blazor-error-reload");
    if (reloadElement && typeof reload === "string") {
        reloadElement.textContent = reload;
    }

    const errorBoundaryElement = document.querySelector(".blazor-error-boundary");
    if (errorBoundaryElement && typeof errorBoundary === "string") {
        errorBoundaryElement.dataset.errorMessage = errorBoundary;
    }
}

export function dispose() {
}
