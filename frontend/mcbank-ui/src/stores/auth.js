import {defineStore} from "pinia";

export const useAuthStore = defineStore('auth', {
    state: () => ({
        token: localStorage.getItem('token') || null,
    }),
    actions: {
        setToken(newToken) {
            this.token = newToken
            localStorage.setItem('token', newToken)
        },
        logout() {
            this.token = null
            localStorage.removeItem('token')
        }
    }
})