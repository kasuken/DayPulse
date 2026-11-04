export function download(filename, contentType, content) {
    const blob = new Blob([content], { type: contentType });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    try {
        anchor.href = url;
        anchor.download = filename;
        document.body.appendChild(anchor);
        anchor.click();
    } finally {
        anchor.remove();
        // Keep the object URL alive long enough for the browser to start the download.
        window.setTimeout(() => URL.revokeObjectURL(url), 1000);
    }
}

export function resetInstallDismissal() {
    localStorage.removeItem("daypulse.install-prompt.dismissed.v1");
    localStorage.removeItem("daypulse.reminder.last-shown.v1");
}
