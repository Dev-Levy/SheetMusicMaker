<script>
    import { Play, Trash } from "lucide-svelte";

    export let name;
    export let onDelete;
    export let onPlay;
</script>

<div id="card">
    <span id="name">{name}</span>
    <button onclick={onPlay}>
        <Play />
    </button>
    <button onclick={onDelete}>
        <Trash />
    </button>
</div>

<style>
    #card {
        display: flex;
        align-items: center;
        border-radius: 5px;
        background-color: #415a77;
        padding: 1em;
        margin: 0.5em;
    }
    #name {
        font-weight: bold;
        width: 100%;
    }

    button {
        margin: 0.2em;
    }
</style>
