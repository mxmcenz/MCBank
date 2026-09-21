<script setup>
defineProps(['account'])
const formatDate = (date) => {
  if (!date) return '';
  return new Date(date).toLocaleDateString();
}
const typeLabels = {
  'Current': 'Текущий',
  'SavingsFlexible': 'Сберегательный (гибкий)',
  'SavingsFixed': 'Сберегательный (фиксированный)',
  'SavingsReplenishable': 'Сберегательный (пополняемый)'
};
</script>
<template>
<div class="account-card flex items-center justify-between p-4 border border-gray-700 rounded-lg shadow-sm bg-white cursor-pointer hover:bg-gray-50 transition w-full max-w-lg">
  <div>
    <div class="iban font-bold text-sm text-gray-800">{{account.iban}}</div>
    <div class="type text-xs text-gray-500 font-medium">{{ typeLabels[account.type] || account.type }}</div>
  </div>
  <div class="text-right">
    <div class="balance text-sm font-semibold text-emerald-600">{{account.balance}} KZT</div>
    <div class="text-[10px] text-gray-400">
      Открыт: {{ formatDate(account.createdAt) }}
      <span v-if="account.expirationDate"> | Истекает: {{ formatDate(account.expirationDate) }}</span>
    </div>
  </div>
</div>
</template>
<style scoped>
.account-card {
  height: 75px; 
}
</style>