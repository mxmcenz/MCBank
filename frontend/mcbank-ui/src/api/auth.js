import api from "./axios.js";

export function login(credentials) {
    return  api.post('/auth/login', credentials)
}

export default login