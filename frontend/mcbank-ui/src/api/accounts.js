import api from "./axios.js";

export function getAccounts() {
    return api.get('/accounts')
}

export function createAccount(type) {
    return api.post('/accounts', {accountType: type});
}

export function deposit(accountId, amount) {
    return api.post('/accounts/deposit', {accountId, amount: Number(amount)})
}

export function withdraw(accountId, amount) {
    return api.post('/accounts/withdraw', {accountId, amount: Number(amount)})
}

export function transfer (fromAccountId, toAccountId, amount) {
    return api.post('/accounts/transfer', {
        fromAccountId,
        toAccountId,
        amount: Number(amount)
    });
}