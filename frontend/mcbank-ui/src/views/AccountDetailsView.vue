<script setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAccountsStore } from '../stores/accounts';
import { getAccountById } from '../api/accounts';
import { useNotificationStore } from '../stores/notification';

const props = defineProps(['id']);
const router = useRouter();
const store = useAccountsStore();
const notification = useNotificationStore();
const account = ref(null);

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
  <div v-if="account" class="max-w-2xl mx-auto bg-white p-6 rounded-lg shadow">
    <header class="border-b border-gray-800 pb-4 mb-6">
      <h1 class="text-2xl font-bold text-gray-100">Счет: {{ account.iban }}</h1>
      <p class="text-gray-400">Баланс: <span class="font-semibold text-emerald-400 text-lg">{{ account.balance }}</span></p>
      <p class="text-sm text-gray-500">Тип: {{ account.type }}</p>
    </header>

    <div class="grid grid-cols-2 gap-4">
      <button @click="router.push(`/accounts/${props.id}/operation/deposit`)" class="bg-emerald-700 text-white py-2 rounded-md hover:bg-emerald-600">Пополнить</button>
      <button @click="router.push(`/accounts/${props.id}/operation/withdraw`)" class="bg-emerald-700 text-white py-2 rounded-md hover:bg-emerald-600">Снять</button>
      <button @click="router.push(`/transfer?from=${props.id}`)" class="bg-emerald-700 text-white py-2 rounded-md hover:bg-emerald-600 col-span-2">Перевести</button>
      <button @click="router.push(`/accounts/${props.id}/history`)" class="bg-emerald-700 text-white py-2 rounded-md hover:bg-emerald-600 col-span-2">Выписка по счету</button>
      <button @click="router.push('/dashboard')" class="bg-gray-800 text-white py-2 rounded-md hover:bg-gray-700 col-span-2">Назад на главную</button>
      <button @click="handleDelete" class="bg-gray-800 text-white py-2 rounded-md hover:bg-gray-700 col-span-2">Удалить счет</button>
    </div>
  </div>
</template>
