<script setup>
import { useRouter } from 'vue-router';
import { useNotificationStore } from '../stores/notification';
import { useAuthStore } from '../stores/auth';

const router = useRouter();
const notification = useNotificationStore();
const authStore = useAuthStore();

const handleOk = () => {
  const path = notification.redirectPath;
  notification.clear();
  
  if (path) {
    router.push(path);
  } else if (authStore.isAuthenticated) {
    router.push('/dashboard');
  } else {
    router.push('/auth/login');
  }
};
</script>

<template>
  <div v-if="notification.isVisible" class="bg-white p-8 rounded-lg shadow-md max-w-md mx-auto mt-10">
    <div class="text-center">
      <div v-if="notification.type === 'success'" class="text-green-500 text-5xl mb-4">✓</div>
      <div v-else class="text-red-500 text-5xl mb-4">✕</div>
      
      <h2 class="text-2xl font-bold mb-2">{{ notification.type === 'success' ? 'Успешно' : 'Ошибка' }}</h2>
      <p class="text-gray-600 mb-6">{{ notification.message }}</p>
      
      <button 
        @click="handleOk" 
        class="w-full bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700"
      >
        ОК
      </button>
    </div>
  </div>
</template>
