<script setup>
import { onMounted, ref } from 'vue';
import { useAccountsStore } from '../stores/accounts';
import AccountCard from '../components/AccountCard.vue';

const store = useAccountsStore();
const selectedType = ref('Current');

onMounted(() => {
  store.fetchAccounts();
});

const handleCreate = async () => {
  await store.create(selectedType.value);
};
</script>

<template>
  <div class="space-y-8">
    <header class="flex justify-between items-center">
      <h1 class="text-2xl font-bold text-gray-900">Мои счета</h1>
      <div class="flex items-center gap-4">
        <select v-model="selectedType" class="rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500">
          <option value="Current">Текущий</option>
          <option value="Savings">Сберегательный</option>
        </select>
        <button @click="$router.push('/transfer')" class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">
          Перевод
        </button>
        <button @click="handleCreate" class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">
          Открыть счет
        </button>
      </div>
    </header>

    <div v-if="store.loading" class="text-center py-10 text-gray-500">Загрузка...</div>
    
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <AccountCard
        v-for="acc in store.accounts"
        :key="acc.id"
        :account="acc"
        @click="$router.push(`/accounts/${acc.id}`)"
      />
    </div>
  </div>
</template>
