<script>
    let files = $state(null);
    import {UploadWav, DownloadPdf} from './lib/CodeBehind';
    import './lib/style.css'
</script>

<main>
  <h1>Hej</h1>

    <h2>Try my sheet music maker app</h2>

    <div class="container">
        
        <div class="column">
            <input type="file" bind:files={files} accept=".wav"/>
            <button disabled={!files}>Upload</button>
        </div>
    
        <div class="column">
            <button >Analyze</button>
        </div>
    
        <div class="column">
            <p>ASDASDASD</p>
            <button>Download Result</button>
        </div>
    </div>
</main>
