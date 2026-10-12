export async function indexedDbGet(databaseName, storeName, key) {
    return withStore(databaseName, storeName, "readonly", async (store) => {
        return await requestResult(store.get(key));
    });
}

export async function indexedDbSet(databaseName, storeName, key, value) {
    return withStore(databaseName, storeName, "readwrite", async (store) => {
        await requestResult(store.put(value, key));
    });
}

export async function indexedDbRemove(databaseName, storeName, key) {
    return withStore(databaseName, storeName, "readwrite", async (store) => {
        await requestResult(store.delete(key));
    });
}

export async function localStorageTrySet(key, expectedValue, value) {
    return withLocalStorageLock(key, () => {
        const currentValue = window.localStorage.getItem(key);
        if (currentValue !== expectedValue) {
            return false;
        }

        window.localStorage.setItem(key, value);
        return true;
    });
}

export async function localStorageTryRemove(key, expectedValue) {
    return withLocalStorageLock(key, () => {
        const currentValue = window.localStorage.getItem(key);
        if (currentValue !== expectedValue) {
            return false;
        }

        window.localStorage.removeItem(key);
        return true;
    });
}

async function withLocalStorageLock(key, operation) {
    if (!navigator.locks) {
        throw new Error("The Web Locks API is required for atomic DayPulse storage updates.");
    }

    return await navigator.locks.request(
        `daypulse-local-storage:${key}`,
        { mode: "exclusive" },
        operation);
}

async function withStore(databaseName, storeName, mode, operation) {
    const database = await openDatabase(databaseName, storeName);

    try {
        const transaction = database.transaction(storeName, mode);
        return await operation(transaction.objectStore(storeName));
    } finally {
        database.close();
    }
}

function openDatabase(databaseName, storeName) {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(databaseName);

        request.onerror = () => reject(request.error);
        request.onupgradeneeded = () => {
            ensureObjectStore(request.result, storeName);
        };
        request.onblocked = () => {
            reject(new Error(`IndexedDB upgrade for "${databaseName}" is blocked.`));
        };
        request.onsuccess = () => {
            const database = request.result;
            database.onversionchange = () => database.close();

            if (database.objectStoreNames.contains(storeName)) {
                resolve(database);
                return;
            }

            const nextVersion = database.version + 1;
            database.close();

            const upgradeRequest = indexedDB.open(databaseName, nextVersion);
            upgradeRequest.onerror = () => reject(upgradeRequest.error);
            upgradeRequest.onblocked = () => {
                reject(new Error(`IndexedDB upgrade for "${databaseName}" is blocked.`));
            };
            upgradeRequest.onupgradeneeded = () => {
                ensureObjectStore(upgradeRequest.result, storeName);
            };
            upgradeRequest.onsuccess = () => {
                upgradeRequest.result.onversionchange = () =>
                    upgradeRequest.result.close();
                resolve(upgradeRequest.result);
            };
        };
    });
}

function ensureObjectStore(database, storeName) {
    if (!database.objectStoreNames.contains(storeName)) {
        database.createObjectStore(storeName);
    }
}

function requestResult(request) {
    return new Promise((resolve, reject) => {
        request.onerror = () => reject(request.error);
        request.onsuccess = () => resolve(request.result ?? null);
    });
}
