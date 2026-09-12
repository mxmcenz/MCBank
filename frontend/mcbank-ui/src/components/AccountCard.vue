<script setup>
defineProps(['account'])
</script>

<template>
<div class="account-card">
  <div class="iban">{{account.iban}}</div>
  <div class="balance">{{account.balance}}</div>
  <div class="type">{{account.type}}</div>
</div>
</template>

<style scoped>.account-card {
  border: 1px solid #ddd;
  padding: 15px;
  border-radius: 8px;
  cursor: pointer;
  transition: transform 0.2s;
}
.account-card:hover {
  transform: scale(1.02);
}
.iban { font-weight: bold; }
.balance { color: green; font-size: 1.2em; }
</style>