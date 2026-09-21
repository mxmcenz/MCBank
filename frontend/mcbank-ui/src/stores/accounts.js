import { defineStore } from 'pinia';
import { 
    getAccounts, 
    getSavingsPlans,
    createAccount, 
    deposit, 
    withdraw, 
    transfer, 
    getHistory, 
    deleteAccount 
} from '../api/accounts';
export const useAccountsStore = defineStore('accounts', {
    state: () => ({
        accounts: [],
        savingsPlans: [],
        loading: false,
        error: null
    }),
    actions: {
        async fetchAccounts() {
            this.loading = true;
            this.error = null;
            try {
                const response = await getAccounts();
                this.accounts = response.data;
            } catch (err) {
                this.error = err.response?.data?.Message || 'Ошибка загрузки счетов';
                throw err;
            } finally {
                this.loading = false;
            }
        },
        async fetchSavingsPlans() {
            try {
                const response = await getSavingsPlans();
                this.savingsPlans = response.data;
            } catch (err) {
                this.error = err.response?.data?.Message || 'Ошибка загрузки планов';
                throw err;
            }
        },
        async create(type, termMonths) {
            try {
                await createAccount(type, termMonths);
                await this.fetchAccounts();
            } catch (err) {
                this.error = err.response?.data?.Message || 'Ошибка создания счета';
                throw err;
            }
        },
        async performTransaction(type, accountId, amount) {
            try {
                if (type === 'deposit') await deposit(accountId, amount);
                else await withdraw(accountId, amount);
                await this.fetchAccounts();
            } catch (err) {
                this.error = err.response?.data?.Message || 'Ошибка транзакции';
                throw err;
            }
        },
        async performTransfer(fromAccountId, toAccountId, amount) {
            try {
                await transfer(fromAccountId, toAccountId, amount);
                await this.fetchAccounts();
            } catch (err) {
                this.error = err.response?.data?.Message || 'Ошибка перевода';
                throw err;
            }
        },
        async delete(accountId) {
            try {
                await deleteAccount(accountId);
                await this.fetchAccounts();
            } catch (err) {
                this.error = err.response?.data?.Message || 'Ошибка удаления счета';
                throw err;
            }
        }
    }
});
