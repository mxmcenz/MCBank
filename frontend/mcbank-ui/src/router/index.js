import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import AuthLayout from '../layouts/AuthLayout.vue'
import MainLayout from '../layouts/MainLayout.vue'
import LoginView from "../views/LoginView.vue";
import RegisterView from "../views/RegisterView.vue";
import DashboardView from "../views/DashboardView.vue";
import AccountDetailsView from "../views/AccountDetailsView.vue";
import OperationView from "../views/OperationView.vue";
import TransactionHistoryView from "../views/TransactionHistoryView.vue";
import TransferView from "../views/TransferView.vue";
import ResultView from "../views/ResultView.vue";

const routes = [
    {
        path: '/auth',
        component: AuthLayout,
        redirect: '/auth/login',
        children: [
            { path: 'login', component: LoginView },
            { path: 'register', component: RegisterView },
            { path: 'result', component: ResultView }
        ]
    },
    {
        path: '/',
        component: MainLayout,
        meta: { requiresAuth: true },
        redirect: '/dashboard',
        children: [
            { path: 'dashboard', component: DashboardView },
            { path: 'accounts/:id', component: AccountDetailsView, props: true },
            { path: 'accounts/:id/operation/:type', component: OperationView, props: true },
            { path: 'accounts/:id/history', component: TransactionHistoryView, props: true },
            { path: 'transfer', component: TransferView },
            { path: 'result', component: ResultView }
        ]
    }
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

router.beforeEach(async (to, from, next) => {
    const authStore = useAuthStore()
    
    if (to.meta.requiresAuth && !authStore.isAuthenticated) {
        next('/auth/login')
    } else if ((to.path === '/auth/login' || to.path === '/auth/register') && authStore.isAuthenticated) {
        next('/dashboard')
    } else {
        next()
    }
})

export default router