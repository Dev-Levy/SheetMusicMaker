<script lang="ts">
    import Card from './Card.svelte'

    let items = $state([
        {id: 1, name: 'file1'},
        {id: 2, name: 'file2'},
        {id: 3, name: 'file3'}
    ])
    
    let file: String = $state("")
    $inspect("File is updated!", file)
    
    function deleteItem(id: number) {
        items = items.filter(item => item.id !== id);
        file = ""
    }
    
    function upload(){
        console.log("uploading file")
        
        const newId = Math.max(0, ...items.map(i => i.id)) + 1;
        items = [...items, { id: newId, name: `file${newId}` }];
        file = ""
    }
</script>

<div class="CardZone">
    <div class="card-list">
        {#each items as item (item.id)}
            <Card 
                name={item.name} 
                onDelete={() => deleteItem(item.id)} 
            />
        {/each}
    </div>
    <div>
        <input type="file" name="fileselect" id="fileselect" accept=".wav , .mp3" bind:value={file}>
        <button onclick="{upload}">Upload</button>
    </div>
</div>

<style>
    .CardZone{
        display: flex;
        flex-direction: column;
        justify-content: space-between;
        height: 100%;
    }
</style>
