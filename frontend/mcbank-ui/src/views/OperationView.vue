<template>
<div class="p-6 max-w-md mx-auto bg-gray-900 rounded-lg shadow border border-gray-800">
  <h1 class="text-xl font-bold mb-4">{{type === 'deposit' ? 'Пополнение' : 'Снятие'}}</h1>
  <input 
    v-model="amount" 
    type="number" 
    min="0" 
    placeholder="Сумма" 
    class="border p-2 rounded mb-4 no-spinners w-full" 
    @keydown="preventNegative"
  />
  <div class="flex gap-2">
    <button @click="submit" class="bg-emerald-700 text-white p-2 rounded flex-1 hover:bg-emerald-600">Подтвердить</button>
    <button @click="router.back()" class="bg-gray-800 text-white p-2 rounded flex-1 hover:bg-gray-700">Назад</button>
    <button @click="router.push('/dashboard')" class="bg-gray-800 text-white p-2 rounded flex-1 hover:bg-gray-700">На главную</button>
  </div>
</div>
</template>

<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import { deposit, withdraw } from "../api/accounts.js";
import { useNotificationStore } from '../stores/notification';

const props = defineProps(['id', 'type'])
const router = useRouter()
const notification = useNotificationStore()
const amount = ref(null) 

function preventNegative(e) {
  if (e.key === '-' || e.key === 'e') {
    e.preventDefault();
  }
}

async function submit() {
  if (amount.value === null || amount.value <= 0) {
    notification.show('Сумма должна быть больше 0', 'error');
    return;
  }
  
  try {
    if (props.type === 'deposit') await deposit(props.id, amount.value)
    else await withdraw(props.id, amount.value)

    notification.show('Операция выполнена успешно', 'success', `/accounts/${props.id}`);
    router.push('/result');
  } catch (error) {
    const data = error.response?.data;
    let errorMessage = 'Не удалось выполнить операцию';
    
    if (typeof data === 'string') {
        errorMessage = data;
    } else if (data && typeof data === 'object') {
        errorMessage = data.message || (data.errors ? Object.values(data.errors).flat().join(', ') : JSON.stringify(data));
    }

    notification.show(errorMessage, 'error', `/accounts/${props.id}`);
    router.push('/result');
  }
}
</script>

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