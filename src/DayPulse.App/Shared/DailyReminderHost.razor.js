const lastShownKey = "daypulse.reminder.last-shown.v1";

export async function requestPermission() {
    if (!("Notification" in window) || !window.isSecureContext) {
        return "unsupported";
    }
    return await Notification.requestPermission();
}

export async function showOnce(date, title, message) {
    const show = () => {
        if (localStorage.getItem(lastShownKey) === date) {
            return false;
        }
        localStorage.setItem(lastShownKey, date);
        if ("Notification" in window && Notification.permission === "granted") {
            try {
                new Notification(title, { body: message, tag: "daypulse-daily-reminder" });
            } catch (error) {
                // Some mobile browsers require service-worker notifications.
                // The in-app reminder remains the supported delivery mechanism.
                console.info("DayPulse will show this reminder inside the app.", error);
            }
        }
        return true;
    };

    return navigator.locks
        ? await navigator.locks.request("daypulse-daily-reminder", show)
        : show();
}
