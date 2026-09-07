<script setup>
import {ref, onMounted} from "vue";
import {getAccounts} from "../api/accounts.js";

const accounts = ref([])
const loading = ref(true)

async function fetchAccounts() {
  try {
    const response = await getAccounts()
    accounts.value = response.data
  } catch (error) {
    console.error('Ошибка при загрузке счетов:', error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchAccounts()
})
</script>

<template>
<div>
  <h1>Мои счета</h1>

  <div v-if="loading">Загрузка...</div>

  <div v-else>
    <ul v-if="accounts.length > 0">
      <li v-for="acc in accounts" :key="acc.id">
        <strong>{{acc.iban}}</strong> - {{acc.balance}} KZT ({{acc.type}})
      </li>
    </ul>
    <p v-else>У вас пока нет открытых счетов.</p>
  </div>
</div>
</template>

<style scoped>

</style>