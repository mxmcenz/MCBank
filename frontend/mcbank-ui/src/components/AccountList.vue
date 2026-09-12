<script setup>
defineProps(['accounts'])
</script>

<template>
  <ul v-if="accounts.length > 0">
    <li v-for="acc in accounts" :key="acc.id">
      <strong>{{ acc.iban }}</strong> - {{ acc.balance }} KZT ({{ acc.type }})
      <button @click="$emit('delete', acc.id)">Удалить</button>
    </li>
  </ul>
  <p v-else>У вас пока нет открытых счетов.</p>
</template>

<style scoped>

</style>