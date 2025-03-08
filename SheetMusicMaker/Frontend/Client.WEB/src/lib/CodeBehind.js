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
            const errorDetails = await response.text();
            console.error("Error details:", errorDetails);
        }
    } catch (error) {
        console.error("Error uploading file:", error);
    }
}

export async function DownloadPdf(id) {
    let pdfblob = await fetchPdf(id);
    if (pdfblob != -1 && pdfblob != -2) {
        DowloadFile(pdfblob[0], pdfblob[1]);
    }
}

async function fetchPdf(id) {
    try {
        const response = await fetch(`http://localhost:5151/api/Pdf/${id}`, {
            method: "GET",
        });

        if (response.ok) {
            let filename = getFilename(response);
            const result = await response.blob();
            console.log("Download successful:", filename);
            return [filename, result];
        } else {
            console.error("Download failed:", response.statusText);
            return -1;
        }
    } catch (error) {
        console.error("Error downloading file:", error);
        return -2;
    }

    function getFilename(response) {
        const headers = response.headers;
        const contentDisposition = headers.get("Content-Disposition");

        let fileName = "yoursheetmusic.pdf";
        if (contentDisposition && contentDisposition.includes("filename=")) {
            fileName = contentDisposition
                .split("filename=")[1]
                .split(";")[0]
                .replace(/['"]/g, "");
        }
        return fileName;
    }
}

function DowloadFile(filename, pdfblob) {
    const link = document.createElement("a");
    link.href = URL.createObjectURL(pdfblob);
    link.download = filename;

    document.body.appendChild(link);
    link.click();

    document.body.removeChild(link);
    URL.revokeObjectURL(link.href);
}
