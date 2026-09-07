<script setup>
import {ref} from 'vue'
import {useRouter} from "vue-router";
import {login} from '../api/auth.js';
import {useAuthStore} from "../stores/auth.js";

const authStore = useAuthStore();
const router = useRouter();

const username = ref('');
const password = ref('');

async function handleLogin() {
  const response = await login({username: username.value, password: password.value})
  if (response && response.data.accessToken){
    authStore.setToken(response.data.accessToken)
    await router.push('/')
  }
}
</script>

<template>
<h1>Login</h1>
  <input v-model="username" placeholder="Логин"/>
  <input v-model="password" type="password" placeholder="Пароль"/>
  <button @click="handleLogin">Войти</button>
</template>

<style scoped>

</style>