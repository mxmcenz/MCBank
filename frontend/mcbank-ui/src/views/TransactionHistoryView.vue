<script setup>
import { ref, onMounted } from "vue";
import { getHistory, getAccountById } from "../api/accounts.js";
import { useRouter } from "vue-router";

const props = defineProps(['id'])
const router = useRouter()
const transactions = ref([])
const account = ref(null)

async function fetchData() {
  try {
    const [historyRes, accRes] = await Promise.all([
      getHistory(props.id),
      getAccountById(props.id)
    ]);
    transactions.value = historyRes.data
    account.value = accRes.data
  } catch (error) {
    alert('Не удалось загрузить данные')
  }
}

onMounted(() => {
  fetchData();
})

const transactionTypes = {
  'Deposit': 'Пополнение',
  'Withdraw': 'Снятие',
  'Transfer': 'Перевод'
};

const getTransactionType = (type) => transactionTypes[type] || type;
</script>

<template>
  <div class="max-w-4xl mx-auto bg-white p-6 rounded-lg shadow">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-2xl font-bold">Выписка по счету {{ account ? account.iban : id }}</h1>
      <button @click="router.push(`/accounts/${props.id}`)" class="bg-gray-800 text-white px-4 py-2 rounded-md hover:bg-gray-700">Назад к счету</button>
    </div>

    <div v-if="transactions.length > 0" class="overflow-x-auto">
      <table class="w-full text-left border-collapse">
        <thead>
          <tr class="border-b">
            <th class="py-2">Тип</th>
            <th class="py-2">Сумма</th>
            <th class="py-2">Дата</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="t in transactions" :key="t.id" class="border-b border-gray-800">
            <td class="py-2" :class="t.type === 'Deposit' ? 'text-emerald-400' : 'text-red-400'">{{ getTransactionType(t.type) }}</td>
            <td class="py-2 font-medium">{{ t.amount }} KZT</td>
            <td class="py-2 text-gray-400">{{ new Date(t.createdAt).toLocaleString() }}</td>
          </tr>
        </tbody>
      </table>
    </div>
    <p v-else class="text-center text-gray-500 py-10">Транзакций нет.</p>
  </div>
</template>

<style scoped>

</style>