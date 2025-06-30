<script lang="ts">
  import CardZone from "./lib/Uploads/CardZone.svelte";
  import AnalyzeZone from "./lib/Analyze/AnalyzeZone.svelte";
  import ExportZone from "./lib/Export/ExportZone.svelte";
  
</script>

<main>
  <CardZone/>
  <AnalyzeZone/>
  <ExportZone/>
</main>

<style>
  main{
    display: flex;
    flex-direction:row;
    align-items: center;
    justify-content: space-evenly;
    width: 80%;
    height: 100%;
  }

  /* :global(*){
    border: red, solid, 1px;
  } */
</style>
