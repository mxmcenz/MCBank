import {createRouter, createWebHistory} from 'vue-router'
import LoginView from "../views/LoginView.vue";
import DashboardView from "../views/DashboardView.vue";
import RegisterView from "../views/RegisterView.vue";

const routes = [
    {path: '/login', component: LoginView},
    {path: '/register', component: RegisterView},
    {path: '/', component: DashboardView, meta: {requiresAuth: true}}
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

router.beforeEach((to, from, next) => {
    const isAuthenticated = !!localStorage.getItem('token')

    if (to.meta.requiresAuth && !isAuthenticated) {
        next('/login')
    } else {
        next()
    }
})

export default router