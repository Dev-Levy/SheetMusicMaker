<script>
    import Dropzone from "svelte-file-dropzone";

    let recordings = $state(null);

    let files = $state({
        accepted: [],
        rejected: []
    });

    function handleFilesSelect(e) {
        const { acceptedFiles, fileRejections } = e.detail;
        files.accepted = [...files.accepted, ...acceptedFiles];
        files.rejected = [...files.rejected, ...fileRejections];
    }

    let pdfUrl;
    
    async function LoadRecordingList() {
        try {
            const response = await fetch("http://localhost:5151/api/Recording", {
                method: "GET",
            });

            if (response.ok) {
                const result = await response.json();
                console.log("Getting all recordings was successful:", result);
                recordings = result;
            } else {
                console.error(
                    "Getting all recordings failed:",
                    response.statusText
                );
                const errorDetails = await response.text();
                console.error("Error details:", errorDetails);
            }
        } catch (error) {
            console.error("Error getting all recording file:", error);
        }
    }   

    async function UploadWavFiles(files) {
        for (let index = 0; index < files.length; index++) {
            const file = files[index];
            const data = new FormData();
            data.append("file", file[0]);

            try {
                const response = await fetch("http://localhost:5151/api/Recording", {
                    method: "POST",
                    body: data,
                });

                if (response.ok) {
                    LoadRecordingList();
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
    import './lib/style.css'

</script>

<main onload={()=>LoadRecordingList()}>
  <h1>Hej</h1>

    <h2>Try my sheet music maker app</h2>

    <div class="container">
        <div class="column">
            <ul>
                {#each recordings as recording}
                  <li>{recording.fileName}</li>
                {/each}
            </ul>
            <Dropzone on:drop={handleFilesSelect} accept=".wav"/>
            <button onclick={()=>UploadWavFiles(files.accepted)} disabled={!files.accepted.length}>Upload</button>
        </div>
    
        <div class="column">
            <button >Analyze</button>
        </div>
    
        <div class="column">
            {#if pdfUrl}
                <iframe src={pdfUrl} title="PDF Viewer"></iframe>
            {:else}
                <p>No PDF to display.</p>
            {/if}
            <button>Download Result</button>
        </div>
    </div>
</main>
