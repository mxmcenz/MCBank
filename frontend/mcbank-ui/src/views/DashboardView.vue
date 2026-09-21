<script setup>
import { onMounted, computed } from 'vue';
import { useAccountsStore } from '../stores/accounts';
import AccountCard from '../components/AccountCard.vue';
const store = useAccountsStore();
onMounted(() => {
  store.fetchAccounts();
});
const currentAccounts = computed(() => 
  store.accounts
    .filter(a => a.type === 'Current')
    .sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt))
);
const savingsAccounts = computed(() => 
  store.accounts
    .filter(a => a.type !== 'Current')
    .sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt))
);
</script>
<template>
  <div class="space-y-8">
    <header class="flex justify-between items-center">
      <h1 class="text-2xl font-bold text-gray-900">Мои счета</h1>
      <div class="flex items-center gap-4">
        <button @click="$router.push('/transfer')" class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">
          Перевод
        </button>
        <button @click="$router.push('/create-account')" class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">
          Открыть счет
        </button>
      </div>
    </header>
    <div v-if="store.loading" class="text-center py-10 text-gray-500">Загрузка...</div>
    <div v-else class="space-y-6">
      <section>
        <h2 class="text-xl font-semibold mb-4 text-gray-600">Текущие счета</h2>
        <div class="space-y-4">
          <AccountCard
            v-for="acc in currentAccounts"
            :key="acc.id"
            :account="acc"
            @click="$router.push(`/accounts/${acc.id}`)"
          />
        </div>
      </section>
      <section>
        <h2 class="text-xl font-semibold mb-4 text-gray-600">Сберегательные счета</h2>
        <div class="space-y-4">
          <AccountCard
            v-for="acc in savingsAccounts"
            :key="acc.id"
            :account="acc"
            @click="$router.push(`/accounts/${acc.id}`)"
          />
        </div>
      </section>
    </div>
  </div>
</template>
