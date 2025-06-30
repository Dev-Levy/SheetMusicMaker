<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@24,400,0,0&icon_names=delete" />

<script>
    export let name;
    export let onDelete;
</script>


<div class="card">
    <span class="name">{name}</span>
    <button class="delete-btn" on:click="{onDelete}">
        <span class="material-symbols-outlined">
            delete
        </span>
    </button>
</div>

<style>
  .card {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1rem;
    margin: 0.5rem 0;
    border: 1px solid rgb(65, 65, 65);
    border-radius: 4px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  }
  
  .name {
    font-size: 1.2rem;
    font-weight: bold;
  }
  
  .delete-btn {
    color: white;
    cursor: pointer;
    font-size: 1rem;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  
  .delete-btn:hover {
    background: #757575;
  }
</style>