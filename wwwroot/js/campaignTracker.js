window.campaignTracker = {
    actionLogAutoScroll: {
        active: false,
        container: null,
        pointerY: 0,
        frameId: null,
        handleDragOver: null,
        handleStop: null,

        start(containerId) {
            const autoScroll = window.campaignTracker.actionLogAutoScroll;
            autoScroll.stop();

            const container = document.getElementById(containerId);

            if (!container) {
                return;
            }

            const rect = container.getBoundingClientRect();
            autoScroll.active = true;
            autoScroll.container = container;
            autoScroll.pointerY = rect.top + rect.height / 2;
            autoScroll.handleDragOver = event => {
                autoScroll.pointerY = event.clientY;
            };
            autoScroll.handleStop = () => autoScroll.stop();

            document.addEventListener("dragover", autoScroll.handleDragOver, true);
            document.addEventListener("drop", autoScroll.handleStop, true);
            document.addEventListener("dragend", autoScroll.handleStop, true);
            autoScroll.scrollLoop();
        },

        scrollLoop() {
            const autoScroll = window.campaignTracker.actionLogAutoScroll;

            if (!autoScroll.active || !autoScroll.container) {
                autoScroll.frameId = null;
                return;
            }

            const rect = autoScroll.container.getBoundingClientRect();
            const threshold = Math.min(72, Math.max(32, rect.height / 4));
            const maxStep = 18;
            let scrollStep = 0;

            if (autoScroll.pointerY < rect.top + threshold) {
                scrollStep = -maxStep * Math.min(1, (rect.top + threshold - autoScroll.pointerY) / threshold);
            } else if (autoScroll.pointerY > rect.bottom - threshold) {
                scrollStep = maxStep * Math.min(1, (autoScroll.pointerY - (rect.bottom - threshold)) / threshold);
            }

            if (scrollStep !== 0) {
                autoScroll.container.scrollTop += scrollStep;
            }

            autoScroll.frameId = window.requestAnimationFrame(() => autoScroll.scrollLoop());
        },

        stop() {
            const autoScroll = window.campaignTracker.actionLogAutoScroll;

            if (autoScroll.handleDragOver) {
                document.removeEventListener("dragover", autoScroll.handleDragOver, true);
            }

            if (autoScroll.handleStop) {
                document.removeEventListener("drop", autoScroll.handleStop, true);
                document.removeEventListener("dragend", autoScroll.handleStop, true);
            }

            if (autoScroll.frameId) {
                window.cancelAnimationFrame(autoScroll.frameId);
            }

            autoScroll.active = false;
            autoScroll.container = null;
            autoScroll.frameId = null;
            autoScroll.handleDragOver = null;
            autoScroll.handleStop = null;
        }
    },

    indexedDb: {
        databaseName: "CampaignTracker",
        databaseVersion: 1,
        storeName: "campaigns",
        activeCampaignKey: "active",

        openDatabase() {
            return new Promise((resolve, reject) => {
                const storage = window.campaignTracker.indexedDb;

                if (!window.indexedDB) {
                    resolve(null);
                    return;
                }

                const request = window.indexedDB.open(storage.databaseName, storage.databaseVersion);

                request.onupgradeneeded = () => {
                    const database = request.result;

                    if (!database.objectStoreNames.contains(storage.storeName)) {
                        database.createObjectStore(storage.storeName, { keyPath: "id" });
                    }
                };

                request.onsuccess = () => resolve(request.result);
                request.onerror = () => reject(request.error);
            });
        },

        async loadCampaignJson() {
            const storage = window.campaignTracker.indexedDb;
            const database = await storage.openDatabase();

            if (!database) {
                return null;
            }

            try {
                return await new Promise((resolve, reject) => {
                    const transaction = database.transaction(storage.storeName, "readonly");
                    const store = transaction.objectStore(storage.storeName);
                    const request = store.get(storage.activeCampaignKey);

                    request.onsuccess = () => resolve(request.result?.json ?? null);
                    request.onerror = () => reject(request.error);
                });
            } finally {
                database.close();
            }
        },

        async saveCampaignJson(json) {
            const storage = window.campaignTracker.indexedDb;
            const database = await storage.openDatabase();

            if (!database) {
                return;
            }

            try {
                await new Promise((resolve, reject) => {
                    const transaction = database.transaction(storage.storeName, "readwrite");
                    const store = transaction.objectStore(storage.storeName);

                    store.put({
                        id: storage.activeCampaignKey,
                        schemaVersion: 1,
                        updatedUtc: new Date().toISOString(),
                        json
                    });

                    transaction.oncomplete = () => resolve();
                    transaction.onerror = () => reject(transaction.error);
                    transaction.onabort = () => reject(transaction.error);
                });
            } finally {
                database.close();
            }
        },

        async clearCampaign() {
            const storage = window.campaignTracker.indexedDb;
            const database = await storage.openDatabase();

            if (!database) {
                return;
            }

            try {
                await new Promise((resolve, reject) => {
                    const transaction = database.transaction(storage.storeName, "readwrite");
                    const store = transaction.objectStore(storage.storeName);

                    store.delete(storage.activeCampaignKey);

                    transaction.oncomplete = () => resolve();
                    transaction.onerror = () => reject(transaction.error);
                    transaction.onabort = () => reject(transaction.error);
                });
            } finally {
                database.close();
            }
        }
    },

    async saveTextFile(fileName, text, contentType) {
        if (!window.showSaveFilePicker) {
            return false;
        }

        const handle = await window.showSaveFilePicker({
            suggestedName: fileName,
            types: [
                {
                    description: "JSON file",
                    accept: { [contentType || "application/json"]: [".json"] }
                }
            ]
        });

        const writable = await handle.createWritable();
        await writable.write(new Blob([text], { type: contentType || "application/json" }));
        await writable.close();

        return true;
    },

    openFileInput(inputId) {
        const input = document.getElementById(inputId);

        if (!input) {
            return;
        }

        input.value = "";
        input.click();
    },

    selectInputText(inputId) {
        window.setTimeout(() => {
            const input = document.getElementById(inputId);

            if (!input || typeof input.select !== "function") {
                return;
            }

            input.select();
        }, 0);
    }
};
