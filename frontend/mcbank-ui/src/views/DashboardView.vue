<script setup>
import {ref, onMounted} from "vue";
import {getAccounts, createAccount, deposit, withdraw, transfer, getHistory} from "../api/accounts.js";
import {useRouter} from "vue-router";
import {useAuthStore} from "../stores/auth.js";
import {watch} from "vue";

const router = useRouter()
const authStore = useAuthStore()
const accounts = ref([])
const loading = ref(true)
const selectedType = ref('Current')
const selectedAccountId = ref(null)
const amount = ref(0)
const transferFromId = ref(null)
const transferToId = ref(null)
const transferAmount = ref(0)
const transactions = ref([])

async function fetchHistory(id) {
  if (!id) {
    transactions.value = []
    return
  }

  try {
    const response = await getHistory(id)
    transactions.value = response.data
  } catch (error) {
    console.error('Ошибка истории: ', error)
  }
}

watch(selectedAccountId, (newId) => {
  fetchHistory(newId)
})

async function handleTransfer() {
  if (!transferFromId.value || !transferToId.value || transferAmount.value <= 0) {
    alert('Заполните все поля для перевода')
    return
  }

  try {
    await transfer(transferFromId.value, transferToId.value, transferAmount.value)
    transferAmount.value = 0
    await fetchAccounts()
    alert('Перевод выполнен')
  } catch (error) {
    alert(error.response?.data?.Message || 'Ошибка перевода')
  }
}

async function handleTransaction(type) {
  if (!selectedAccountId.value || amount.value <= 0){
    alert('Выберите счет и введите корректную сумму')
    return
  }

  try {
    if (type === 'deposit') {
      await deposit(selectedAccountId.value, amount.value)
    } else {
      await withdraw(selectedAccountId.value, amount.value)
    }
    amount.value = 0
    await fetchAccounts()
    alert('Успешно')
  } catch (error) {
    alert(error.response?.data?.Message || 'Ошибка транзакции')
  }
}

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

function handleLogout() {
  authStore.logout()
  router.push('/login')
}

onMounted(() => {
  fetchAccounts()
})
</script>

<template>
<div>
  <button @click="handleLogout" style="float: right">Выйти</button>

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

  <div v-if="accounts.length > 0" style="margin-top: 30px; padding: 15px; background: #f9f9f9;">
    <hr />
    <h3>Операции по счету</h3>
    <div>
      <label>Счет: </label>
      <select v-model="selectedAccountId">
        <option :value="null">Выберите счет</option>
        <option v-for="acc in accounts" :key="acc.id" :value="acc.id">
          {{acc.iban}} ({{acc.balance}} KZT)
        </option>
      </select>
    </div>

    <div style="margin-top: 10px;">
      <label>Сумма: </label>
      <input v-model="amount" type="number" placeholder="Введите сумму" />
    </div>

    <div style="margin-top: 10px;">
      <button @click="handleTransaction('deposit')">Пополнить</button>
      <button @click="handleTransaction('withdraw')" style="margin-left: 10px;">Снять</button>
    </div>

  </div>

  <div v-if="accounts.length > 1" style="margin-top: 30px; padding: 15px; border: 1px dashed blue;">
    <h3>Перевод между своими счетами</h3>
    <div>
      <label>Откуда: </label>
      <select v-model="transferFromId">
        <option v-for="acc in accounts" :key="acc.id" :value="acc.id">{{acc.iban}}</option>
      </select>
    </div>

    <div style="margin-top: 10px;">
      <label>Куда: </label>
      <select v-model="transferToId">
        <option v-for="acc in accounts" :key="acc.id" :value="acc.id">{{acc.iban}}</option>
      </select>
    </div>

    <div style="margin-top: 10px;">
      <input v-model="transferAmount" type="number" placeholder="Сумма перевода" />
      <button @click="handleTransfer">Перевести</button>
    </div>
  </div>

  <div v-if="selectedAccountId && transactions.length > 0" style="margin-top: 30px;">
    <h3>История транзакций по счету</h3>
    <table border="1" cellpadding="5" style="width: 100%; text-align: left">
      <thead>
        <tr>
          <th>Тип</th>
          <th>Сумма</th>
          <th>Дата</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="t in transactions" :key="t.id">
          <td>{{t.type}}</td>
          <td>{{t.amount}}</td>
          <td>{{new Date(t.createdAt).toLocaleString()}}</td>
        </tr>
      </tbody>
    </table>
  </div>
  <p v-else-if="selectedAccountId">Транзакций пока нет</p>

</div>
</template>

<style scoped>

</style>