<script lang="ts">
    import UploadsZone from "./lib/UploadsZone.svelte";
    import AnalyzeZone from "./lib/AnalyzeZone.svelte";
    import ExportZone from "./lib/ExportZone.svelte";
</script>

<main>
    <UploadsZone />
    <AnalyzeZone />
    <ExportZone />
</main>

<style>
    main {
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: space-evenly;
        width: 1800px;
        height: 600px;
    }

    :global(*) {
        box-sizing: border-box;
        margin: 0;
        padding: 0;
    }
</style>
