export async function UploadWav(file) {
    const data = new FormData();
    data.append("file", file);

    try {
        const response = await fetch("http://localhost:5151/api/Recording", {
            method: "POST",
            body: data,
        });

        if (response.ok) {
            const result = await response.json();
            console.log("Upload successful:", result);
        } else {
            console.error("Upload failed:", response.statusText);
        }
    } catch (error) {
        console.error("Error uploading file:", error);
    }
}

export async function DownloadPdf() {
    try {
        const response = await fetch("http://localhost:5151/api/Recording", {
            method: "GET",
        });

        if (response.ok) {
            const result = await response.json();
            console.log("Upload successful:", result);
        } else {
            console.error("Upload failed:", response.statusText);
        }
    } catch (error) {
        console.error("Error uploading file:", error);
    }
}
