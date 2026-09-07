import api from "./axios.js";

export function getAccounts() {
    return api.get('/accounts')
}

export function createAccount(type) {
    return api.post('/accounts', {accountType: type});
}