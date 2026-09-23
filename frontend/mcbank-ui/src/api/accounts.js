import api from "./axios.js";
export function getAccountIdByIban(iban) {
    return api.get(`/accounts/iban/${iban}`)
}
export function getAccounts() {
    return api.get('/accounts')
}
export function getSavingsPlans() {
    return api.get('/accounts/plans')
}
export function getAccountById(id){
    return api.get(`/accounts/${id}`)
}
export function createAccount(type, termMonths = null) {
    return api.post('/accounts', {accountType: type, termMonths});
}
export function deposit(accountId, amount) {
    return api.post('/transactions/deposit', {accountId, amount: Number(amount)})
}
export function withdraw(accountId, amount) {
    return api.post('/transactions/withdraw', {accountId, amount: Number(amount)})
}
export function transfer (fromAccountId, toAccountId, amount) {
    return api.post('/transactions/transfer', {
        fromAccountId,
        toAccountId,
        amount: Number(amount)
    });
}
export function getHistory(accountId) {
    return api.get(`/transactions/${accountId}`)
}
export function deleteAccount(accountId) {
    return api.delete(`/accounts/${accountId}`)
}