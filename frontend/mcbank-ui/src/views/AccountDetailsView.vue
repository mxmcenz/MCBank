<script setup>
import { onMounted, ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAccountsStore } from '../stores/accounts';
import { getAccountById } from '../api/accounts';
import { useNotificationStore } from '../stores/notification';
const props = defineProps(['id']);
const router = useRouter();
const store = useAccountsStore();
const notification = useNotificationStore();
const account = ref(null);
const typeLabels = {
  'Current': 'Текущий',
  'SavingsFlexible': 'Сберегательный (гибкий)',
  'SavingsFixed': 'Сберегательный (фиксированный)',
  'SavingsReplenishable': 'Сберегательный (пополняемый)'
};
const formatDate = (date) => {
  if (!date) return '';
  return new Date(date).toLocaleDateString();
}
async function fetchAccount() {
  try {
    const response = await getAccountById(props.id);
    account.value = response.data;
  } catch (error) {
    notification.show('Счет не найден', 'error', '/dashboard');
    router.push('/result');
  }
}
async function handleDelete() {
  if (confirm('Вы уверены, что хотите удалить счет?')) {
    try {
      await store.delete(props.id);
      notification.show('Счет успешно удален', 'success', '/dashboard');
      router.push('/result');
    } catch (error) {
      notification.show('Ошибка при удалении', 'error', `/accounts/${props.id}`);
      router.push('/result');
    }
  }
}
onMounted(fetchAccount);
</script>
<template>
  <div v-if="account" class="max-w-2xl mx-auto bg-white p-6 rounded-lg shadow border border-gray-700">
    <header class="border-b border-gray-200 pb-4 mb-6 space-y-2">
      <h1 class="text-2xl font-bold text-gray-900">Счет: {{ account.iban }}</h1>
      <div class="grid grid-cols-2 gap-2 text-sm text-gray-800">
        <p>Баланс: <span class="font-semibold text-emerald-700 text-lg">{{ account.balance }} KZT</span></p>
        <p>Тип: <span class="font-medium text-gray-700">{{ typeLabels[account.type] || account.type }}</span></p>
        <p>Открыт: <span class="font-medium text-gray-700">{{ formatDate(account.createdAt) }}</span></p>
        <template v-if="account.expirationDate">
           <p>Истекает: <span class="font-medium text-gray-700">{{ formatDate(account.expirationDate) }}</span></p>
           <p>Ставка: <span class="font-semibold text-blue-800">{{ account.interestRate }}% годовых</span></p>
        </template>
      </div>
    </header>
    <div class="grid grid-cols-2 gap-4">
      <button @click="router.push(`/accounts/${props.id}/operation/deposit`)" class="bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700">Пополнить</button>
      <button @click="router.push(`/accounts/${props.id}/operation/withdraw`)" class="bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700">Снять</button>
      <button @click="router.push(`/transfer?from=${props.id}`)" class="bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700 col-span-2">Перевести</button>
      <button @click="router.push(`/accounts/${props.id}/history`)" class="bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700 col-span-2">Выписка по счету</button>
      <button @click="router.push('/dashboard')" class="bg-gray-700 text-white py-2 rounded-md hover:bg-gray-600 col-span-2">Назад на главную</button>
      <button @click="handleDelete" class="bg-red-900 text-red-200 py-2 rounded-md hover:bg-red-800 col-span-2">Удалить счет</button>
    </div>
  </div>
</template>
