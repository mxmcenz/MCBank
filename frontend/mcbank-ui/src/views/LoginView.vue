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
  try {
    const response = await login({username: username.value, password: password.value})

    if (response.status === 200){
      authStore.loginSuccess()
      router.push('/')
    }
  } catch (error) {
    alert('Ошибка авторизации')
  }
}
</script>

<template>
<h1>Авторизация</h1>
  <input v-model="username" placeholder="Логин"/>
  <input v-model="password" type="password" placeholder="Пароль"/>
  <button @click="handleLogin">Войти</button>
  <router-link to="/register">Нет аккаунта? Зарегистрироваться</router-link>
</template>

<style scoped>

</style>