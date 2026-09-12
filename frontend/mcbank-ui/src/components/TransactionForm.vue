<script setup>
import { ref } from 'vue'

const props = defineProps(['accounts'])
const emit = defineEmits(['transaction'])

const localAccountId = ref(null)
const localAmount = ref(0)

function submit(type) {
  if (!localAccountId.value || localAmount <= 0) return

  emit('transaction', {
    accountId: localAccountId.value,
    amount: localAmount.value,
    type: type
  })
}
</script>

<template>
<div>
  <select v-model="localAccountId">
    <option v-for="acc in props.accounts" :key="acc.id" :value="acc.id">
      {{acc.iban}}
    </option>
  </select>
  <input v-model="localAmount" type="number" />
  <button @click="submit('deposit')">Пополнить</button>
  <button @click="submit('withdraw')">Снять</button>
</div>
</template>

<style scoped>

</style>