<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAccountsStore } from '../stores/accounts';
const router = useRouter();
const store = useAccountsStore();
const accountType = ref('Current'); 
const selectedType = ref(''); 
const selectedTerm = ref(null);
const typeLabels = {
  'SavingsFlexible': 'Сберегательный (гибкий)',
  'SavingsFixed': 'Сберегательный (фиксированный)',
  'SavingsReplenishable': 'Сберегательный (пополняемый)'
};
onMounted(() => {
  store.fetchSavingsPlans();
});
const plans = computed(() => store.savingsPlans);
const savingsTypes = computed(() => {
  const types = new Set(plans.value.map(p => p.type));
  return Array.from(types);
});
const availableTerms = computed(() => {
  return plans.value
    .filter(p => p.type === selectedType.value)
    .map(p => p.termMonths);
});
const setDefaultTerm = () => {
  selectedTerm.value = availableTerms.value.length > 0 ? availableTerms.value[0] : null;
};
const interestRate = computed(() => {
  const plan = plans.value.find(p => p.type === selectedType.value && p.termMonths === selectedTerm.value);
  return plan ? plan.interestRate : null;
});
const handleCreate = async () => {
  try {
    if (accountType.value === 'Current') {
      await store.create('Current', null);
    } else {
      await store.create(selectedType.value, selectedTerm.value);
    }
    router.push('/dashboard');
  } catch (err) {
    console.error(err);
  }
};
</script>
<template>
  <div class="max-w-md mx-auto mt-10 p-6 bg-white rounded-lg shadow border border-gray-700">
    <h1 class="text-2xl font-bold mb-6">Открыть новый счет</h1>
    <div class="mb-4">
      <label class="block text-sm font-medium text-gray-700">Тип счета</label>
      <select v-model="accountType" class="w-full border p-2 rounded">
        <option value="Current">Текущий</option>
        <option value="Savings">Депозит</option>
      </select>
    </div>
    <template v-if="accountType === 'Savings'">
      <div class="mb-4">
        <label class="block text-sm font-medium text-gray-700">Вид депозита</label>
        <select v-model="selectedType" @change="setDefaultTerm" class="w-full border p-2 rounded">
          <option v-for="type in savingsTypes" :key="type" :value="type">{{ typeLabels[type] || type }}</option>
        </select>
      </div>
      <div class="mb-4" v-if="selectedType">
        <label class="block text-sm font-medium text-gray-700">Срок</label>
        <select v-model="selectedTerm" class="w-full border p-2 rounded">
          <option v-for="term in availableTerms" :key="term" :value="term">{{ term }} месяцев</option>
        </select>
      </div>
      <div class="mb-4 p-4 bg-blue-50 rounded-md" v-if="interestRate">
        <p class="text-sm font-medium text-blue-800">
          Процентная ставка: <span class="text-2xl font-bold">{{ interestRate }}%</span> годовых
        </p>
      </div>
    </template>
    <button @click="handleCreate" :disabled="accountType === 'Savings' && !selectedTerm" class="w-full bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700 disabled:bg-gray-400">
      Открыть счет
    </button>
  </div>
</template>
