<script setup>
import { ref } from 'vue';
import { useRouter } from "vue-router";
import { login } from '../api/auth.js';
import { useAuthStore } from "../stores/auth.js";
import { useNotificationStore } from '../stores/notification';

const authStore = useAuthStore();
const router = useRouter();
const notification = useNotificationStore();

const username = ref('');
const password = ref('');
const loading = ref(false);

async function handleLogin() {
  loading.value = true;
  try {
    await login({ username: username.value, password: password.value });
    authStore.loginSuccess();
    router.push('/dashboard');
  } catch (error) {
    const data = error.response?.data;
    let errorMessage = 'Ошибка авторизации';
    
    if (typeof data === 'string') {
        errorMessage = data;
    } else if (data && typeof data === 'object') {
        errorMessage = data.message || (data.errors ? Object.values(data.errors).flat().join(', ') : JSON.stringify(data));
    }
    
    notification.show(errorMessage, 'error');
    router.push('/auth/result');
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="bg-gray-900 p-8 rounded-lg shadow-md border border-gray-800">
    <h1 class="text-2xl font-bold text-gray-100 mb-6 text-center">Вход в MCBank</h1>
    <div class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-gray-400">Логин</label>
        <input v-model="username" type="text" class="mt-1 block w-full rounded-md bg-gray-800 border-gray-700 text-gray-100 py-2 px-3 shadow-sm focus:border-emerald-500 focus:ring-emerald-500" />
      </div>
      <div>
        <label class="block text-sm font-medium text-gray-400">Пароль</label>
        <input v-model="password" type="password" class="mt-1 block w-full rounded-md bg-gray-800 border-gray-700 text-gray-100 py-2 px-3 shadow-sm focus:border-emerald-500 focus:ring-emerald-500" />
      </div>
      <button 
        @click="handleLogin" 
        :disabled="loading"
        class="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-emerald-700 hover:bg-emerald-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-emerald-500"
      >
        {{ loading ? 'Вход...' : 'Войти' }}
      </button>
      <div class="text-center">
        <router-link to="/auth/register" class="text-sm text-emerald-400 hover:text-emerald-300">Нет аккаунта? Зарегистрироваться</router-link>
      </div>
    </div>
  </div>
</template>
