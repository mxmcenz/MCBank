import axios from "axios";
import {useAuthStore} from "../stores/auth.js";

const api = axios.create({
    baseURL: "http://localhost:5555/api",
    withCredentials: true
})

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originRequest = error.config;
        const authStore = useAuthStore();

        if (error.response?.status === 401 && !originRequest._retry) {
            originRequest._retry = true;

            try {
                await api.post('/auth/refresh');
                return api(originRequest);
            } catch (refreshError) {
                authStore.logout();
                window.location.href = '/auth/login';
                return Promise.reject(refreshError)
            }
        }

        return Promise.reject(error)
    }
);

export default api