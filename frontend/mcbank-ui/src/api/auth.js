import api from "./axios.js";

export function login(credentials) {
    return  api.post('/auth/login', credentials)
}

export function register(credentials) {
    return  api.post('/auth/register', credentials)
}

export function logout() {
    return api.post('/auth/logout')
}