import { defineStore } from 'pinia';
import { 
    getAccounts, 
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
        async create(type) {
            try {
                await createAccount(type);
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
