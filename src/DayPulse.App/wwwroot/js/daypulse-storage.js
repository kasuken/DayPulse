// Journal storage: a single IndexedDB key/value store ("daypulse" / "key-value").
//
// Every read, write and conditional write runs in one IndexedDB transaction, so a conditional write
// (compare the stored value, then store) is atomic across tabs without needing the Web Locks API.
// Data written by earlier versions lived in localStorage; it is copied across once, on first use.

const databaseName = "daypulse";
const storeName = "key-value";

// The keys the repositories used in localStorage before the move to IndexedDB. Other localStorage
// entries (install prompt dismissal, reminder markers) are interface conveniences and stay there.
const legacyKeys = [
    "daypulse.check-ins",
    "daypulse.activities",
    "daypulse.settings"
];

// Written in the same transaction that copies the legacy values. Once present, localStorage is never
// copied from again, so data deleted from IndexedDB cannot reappear from a stale legacy copy.
const migrationMarkerKey = "daypulse.migration.local-storage.v1";

let databasePromise = null;
let migrationPromise = null;

export async function storageGet(key) {
    await ensureMigrated();
    return transact("readonly", (store, setResult) => {
        const request = store.get(key);
        request.onsuccess = () => setResult(request.result === undefined ? null : request.result);
    });
}

export async function storageSet(key, value) {
    await ensureMigrated();
    return transact("readwrite", (store) => {
        store.put(value, key);
    });
}

export async function storageRemove(key) {
    await ensureMigrated();
    return transact("readwrite", (store) => {
        store.delete(key);
    });
}

// Stores value only when the current value equals expectedValue (null means "absent").
export async function storageTrySet(key, expectedValue, value) {
    await ensureMigrated();
    return transact("readwrite", (store, setResult) => {
        const request = store.get(key);
        request.onsuccess = () => {
            const current = request.result === undefined ? null : request.result;
            if (current !== expectedValue) {
                setResult(false);
                return;
            }

            store.put(value, key);
            setResult(true);
        };
    });
}

// Removes the value only when it equals expectedValue (null means "absent").
export async function storageTryRemove(key, expectedValue) {
    await ensureMigrated();
    return transact("readwrite", (store, setResult) => {
        const request = store.get(key);
        request.onsuccess = () => {
            const current = request.result === undefined ? null : request.result;
            if (current !== expectedValue) {
                setResult(false);
                return;
            }

            store.delete(key);
            setResult(true);
        };
    });
}

function ensureMigrated() {
    if (!migrationPromise) {
        migrationPromise = runMigration().catch((error) => {
            migrationPromise = null;
            throw error;
        });
    }

    return migrationPromise;
}

async function runMigration() {
    // The Web Locks API keeps two tabs from migrating at once. Older browsers without it still
    // migrate safely: the copy and the marker are written in a single IndexedDB transaction that
    // skips everything when the marker already exists.
    if (navigator.locks) {
        await navigator.locks.request("daypulse-storage-migration", migrateLegacyData);
        return;
    }

    await migrateLegacyData();
}

async function migrateLegacyData() {
    const legacyValues = readLegacyValues();
    if (legacyValues === null) {
        // localStorage is unavailable, so there is nothing to copy and no reason to record a marker.
        return;
    }

    await transact("readwrite", (store) => {
        const marker = store.get(migrationMarkerKey);
        marker.onsuccess = () => {
            if (marker.result !== undefined) {
                return;
            }

            for (const [key, value] of Object.entries(legacyValues)) {
                const existing = store.get(key);
                existing.onsuccess = () => {
                    if (existing.result === undefined) {
                        store.put(value, key);
                    }
                };
            }

            store.put(new Date().toISOString(), migrationMarkerKey);
        };
    });

    // Only reached after the transaction committed: the copy is durable, so the legacy entries are
    // redundant. Removing them also means "delete all data" leaves nothing behind in localStorage.
    removeLegacyValues(Object.keys(legacyValues));
}

function readLegacyValues() {
    try {
        const values = {};
        for (const key of legacyKeys) {
            const value = window.localStorage.getItem(key);
            if (value !== null) {
                values[key] = value;
            }
        }

        return values;
    } catch {
        return null;
    }
}

function removeLegacyValues(keys) {
    try {
        for (const key of keys) {
            window.localStorage.removeItem(key);
        }
    } catch {
        // Leaving a redundant legacy copy behind is harmless: the marker prevents re-importing it.
    }
}

function transact(mode, work) {
    return openDatabase().then((database) => new Promise((resolve, reject) => {
        let transaction;
        try {
            transaction = database.transaction(storeName, mode);
        } catch (error) {
            reject(error);
            return;
        }

        let result = null;
        transaction.oncomplete = () => resolve(result);
        transaction.onerror = () => reject(transaction.error);
        transaction.onabort = () => reject(transaction.error ?? new Error("The IndexedDB transaction was aborted."));

        // Everything inside work() must stay synchronous with the request callbacks: awaiting any
        // other promise would let the transaction auto-commit before the next request is issued.
        work(transaction.objectStore(storeName), (value) => {
            result = value;
        });
    }));
}

function openDatabase() {
    if (!databasePromise) {
        databasePromise = new Promise((resolve, reject) => {
            const request = indexedDB.open(databaseName, 1);

            request.onupgradeneeded = () => {
                if (!request.result.objectStoreNames.contains(storeName)) {
                    request.result.createObjectStore(storeName);
                }
            };
            request.onblocked = () => {
                reject(new Error(`IndexedDB upgrade for "${databaseName}" is blocked.`));
            };
            request.onerror = () => reject(request.error);
            request.onsuccess = () => {
                const database = request.result;
                const forget = () => {
                    databasePromise = null;
                };

                database.onversionchange = () => {
                    database.close();
                    forget();
                };
                database.onclose = forget;
                resolve(database);
            };
        }).catch((error) => {
            databasePromise = null;
            throw error;
        });
    }

    return databasePromise;
}
