<template>
  <div class="min-h-screen bg-gray-900 text-gray-100">
    <nav class="bg-gray-800 border-b border-gray-700">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between h-16">
          <div class="flex">
            <div class="flex-shrink-0 flex items-center cursor-pointer" @click="router.push('/dashboard')">
              <span class="font-bold text-xl text-blue-400">MCBank</span>
            </div>
            <div class="hidden sm:ml-6 sm:flex sm:space-x-8">
              <router-link to="/dashboard" class="text-gray-300 hover:text-white inline-flex items-center px-1 pt-1 border-b-2 border-transparent text-sm font-medium">Главная</router-link>
            </div>
          </div>
          <div class="flex items-center">
            <button @click="handleLogout" class="text-gray-300 hover:text-white text-sm font-medium">Выйти</button>
          </div>
        </div>
      </div>
    </nav>
    <main class="py-10">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <router-view />
      </div>
    </main>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const router = useRouter();
const authStore = useAuthStore();

const handleLogout = async () => {
  await authStore.logout();
  router.push('/auth/login');
};
</script>
