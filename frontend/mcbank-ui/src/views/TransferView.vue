<script setup>
import { ref, onMounted, computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useAccountsStore } from "../stores/accounts";
import { transfer, getAccountIdByIban } from "../api/accounts";
import { useNotificationStore } from "../stores/notification";

const route = useRoute();
const router = useRouter();
const store = useAccountsStore();
const notification = useNotificationStore();

const transferMode = ref('internal'); 
const sourceAccountId = ref(route.query.from || "");
const destinationAccountId = ref("");
const destinationIban = ref("");
const amount = ref(null);

onMounted(() => {
  store.fetchAccounts();
});

const preventNegative = (e) => {
  if (e.key === '-' || e.key === 'e') e.preventDefault();
};

async function handleTransfer() {
  if (!sourceAccountId.value || amount.value <= 0) {
    notification.show("Заполните все поля корректно", "error");
    return;
  }
  
  let targetId = null;

  if (transferMode.value === 'internal') {
    if (!destinationAccountId.value) {
        notification.show("Выберите счет получателя", "error");
        return;
    }
    targetId = destinationAccountId.value;
  } else {
    if (!destinationIban.value) {
        notification.show("Введите IBAN получателя", "error");
        return;
    }
    try {
        const response = await getAccountIdByIban(destinationIban.value);
        targetId = response.data;
    } catch (e) {
        notification.show("Счет с таким IBAN не найден", "error");
        return;
    }
  }

  try {
    await transfer(sourceAccountId.value, targetId, amount.value);
    notification.show("Перевод выполнен успешно", "success", "/dashboard");
    router.push("/result");
  } catch (error) {
    const data = error.response?.data;
    const errorMessage = typeof data === 'string' ? data : (data?.message || "Ошибка перевода");
    notification.show(errorMessage, "error");
    router.push("/result");
  }
}
</script>

<template>
  <div class="max-w-md mx-auto bg-white p-6 rounded-lg shadow">
    <h1 class="text-xl font-bold mb-4">Перевод средств</h1>
    
    <div class="mb-4">
        <label class="block text-sm font-medium text-gray-700">Тип перевода</label>
        <select v-model="transferMode" class="w-full border p-2 rounded">
            <option value="internal">Между своими счетами</option>
            <option value="external">Клиенту банка</option>
        </select>
    </div>

    <div class="mb-4">
      <label class="block text-sm font-medium text-gray-700">С какого счета</label>
      <select v-model="sourceAccountId" class="w-full border p-2 rounded">
        <option value="" disabled>Выберите счет</option>
        <option v-for="acc in store.accounts" :key="acc.id" :value="acc.id">
          {{ acc.iban }} ({{ acc.balance }} KZT)
        </option>
      </select>
    </div>

    <div class="mb-4" v-if="transferMode === 'internal'">
      <label class="block text-sm font-medium text-gray-700">На какой счет</label>
      <select v-model="destinationAccountId" class="w-full border p-2 rounded">
        <option value="" disabled>Выберите счет</option>
        <option v-for="acc in store.accounts" :key="acc.id" :value="acc.id" :disabled="acc.id === sourceAccountId">
          {{ acc.iban }} ({{ acc.balance }} KZT)
        </option>
      </select>
    </div>

    <div class="mb-4" v-else>
      <label class="block text-sm font-medium text-gray-700">IBAN получателя</label>
      <input v-model="destinationIban" type="text" placeholder="Введите IBAN" class="w-full border p-2 rounded" />
    </div>

    <div class="mb-4">
      <label class="block text-sm font-medium text-gray-700">Сумма</label>
      <input v-model="amount" type="number" min="0" placeholder="Сумма" class="w-full border p-2 rounded no-spinners" @keydown="preventNegative" />
    </div>

    <div class="flex gap-2">
      <button @click="handleTransfer" class="bg-emerald-700 text-white p-2 rounded flex-1 hover:bg-emerald-600">Перевести</button>
      <button @click="router.back()" class="bg-gray-800 text-white p-2 rounded flex-1 hover:bg-gray-700">Назад</button>
    </div>
  </div>
</template>

<style scoped>
.no-spinners::-webkit-outer-spin-button,
.no-spinners::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}
.no-spinners {
  -moz-appearance: textfield;
}
</style>