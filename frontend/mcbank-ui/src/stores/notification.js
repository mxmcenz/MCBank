import { defineStore } from 'pinia';

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    message: '',
    type: 'success', 
    isVisible: false,
    redirectPath: null
  }),
  actions: {
    show(message, type = 'success', redirectPath = null) {
      this.message = message;
      this.type = type;
      this.isVisible = true;
      this.redirectPath = redirectPath;
    },
    clear() {
      this.isVisible = false;
      this.message = '';
      this.redirectPath = null;
    }
  }
});
