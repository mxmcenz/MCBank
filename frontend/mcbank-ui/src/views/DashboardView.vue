<script setup>
import {ref, onMounted} from "vue";
import {getAccounts, createAccount} from "../api/accounts.js";

const accounts = ref([])
const loading = ref(true)
const selectedType = ref('Current')

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

async function handleCreate() {
  try {
    await createAccount(selectedType.value)
    await fetchAccounts()
  } catch (error) {
    alert('Ошибка при создании счета')
  }
}

onMounted(() => {
  fetchAccounts()
})
</script>

<template>
<div>
  <h1>Мои счета</h1>

  <div style="margin-bottom: 20px; padding: 10px; border: 1px solid #ccc;">
    <h3>Открыть новый счет</h3>
    <select v-model = "selectedType">
      <option value="Current">Текущий</option>
      <option value="Savings">Сберегательный</option>
    </select>
    <button @click="handleCreate()">Создать</button>
  </div>

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