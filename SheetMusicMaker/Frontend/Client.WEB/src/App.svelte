<script>
    let file = $state();
    import {UploadWav, DownloadPdf} from './lib/CodeBehind';
</script>

<main>
    <h1>Hej</h1>

    <h2>Try my sheet music maker app</h2>
    <input type="file" bind:value={file} accept=".wav">
    <button onclick={()=>UploadWav(file)}>Upload</button>

    <h2>Try downloading your PDF</h2>
    <p>PDF created!</p>
    <button onclick={()=>DownloadPdf(2)}>Download</button>
</main>
