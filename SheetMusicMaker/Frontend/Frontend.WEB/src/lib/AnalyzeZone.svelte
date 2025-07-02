<script lang="ts">
    function analyze(id: number) {
        console.log("analyzing file with id: " + id);
    }
</script>

<div class="AnalyzeZone">
    <button onclick={() => analyze(1)}>Analyze</button>
</div>

<style>
    .AnalyzeZone {
        width: 20%;
        display: flex;
        align-items: center;
        justify-content: center;
    }
</style>
