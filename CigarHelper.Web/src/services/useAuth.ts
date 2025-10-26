import { computed } from 'vue';
import authService from './authService';

export function useAuth() {
  const isAuthenticated = computed(() => authService.state.isAuthenticated);
  const user = computed(() => authService.state.user);

  return {
    isAuthenticated,
    user,
    logout: authService.logout,
  };
} 