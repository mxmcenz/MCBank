import {createRouter, createWebHistory} from 'vue-router'
import LoginView from "../views/LoginView.vue";
import DashboardView from "../views/DashboardView.vue";

const routes = [
    {path: '/login', component: LoginView},
    {path: '/', component: DashboardView}
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

export default router