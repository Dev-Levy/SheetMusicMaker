<script lang="ts">
    import FileCard from "./FileCard.svelte";

    let items = $state([
        { id: 1, name: "file1" },
        { id: 2, name: "file2" },
        { id: 3, name: "file3" },
    ]);

    let file: String = $state("");
    $inspect("File is updated!", file);

    function deleteItem(id: number) {
        console.log("deleted item with id: " + id);

        items = items.filter((item) => item.id !== id);
        file = "";
    }

    function upload() {
        console.log("uploading file");

        const newId = Math.max(0, ...items.map((i) => i.id)) + 1;
        items = [...items, { id: newId, name: `file${newId}` }];
        file = "";
    }

    function playItem(id: number) {
        console.log("playing item with id: " + id);
    }
</script>

<div class="UploadsZone">
    <div id="list-items">
        {#each items as item}
            <FileCard
                name={item.name}
                onDelete={() => deleteItem(item.id)}
                onPlay={() => playItem(item.id)}
            />
        {/each}
    </div>
    <div id="upload">
        <input type="file" bind:value={file} />
        <button onclick={upload}>Upload</button>
    </div>
</div>

<style>
    .UploadsZone {
        display: flex;
        flex-direction: column;
        align-items: center;
        height: 100%;
        width: 40%;
        margin: 2em;
    }
    #list-items {
        width: 100%;
        height: 100%;
        overflow-y: scroll;
        scrollbar-width: none;
        max-width: 500px;

        border-radius: 5px;
        border: 2px #e0e1dd solid;
    }
    #upload {
        display: flex;
        align-items: center;
        justify-content: center;
        margin: 2em 0;
    }
</style>
