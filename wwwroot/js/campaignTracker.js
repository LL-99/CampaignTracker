window.campaignTracker = {
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
