import api from './api';
import { jwtDecode } from 'jwt-decode';
import { reactive, readonly } from 'vue';

// --- Interfaces ---

export interface User {
  id: number;
  nameid: string;
  email: string;
  unique_name: string;
  role: string | string[];
  // Добавьте другие поля, если они есть в токене
}

export interface AuthCredentials {
  email?: string;
  password?: string;
}

export interface RegisterData {
  username?: string;
  email?: string;
  password?: string;
  confirmPassword?: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token: string;
  username: string;
  expiration: string; // ISO 8601 date string
}

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
}

// --- Reactive State ---
const state = reactive<AuthState>({
  user: null,
  isAuthenticated: false,
});

const TOKEN_KEY = 'authToken';

// --- Private Methods ---
function updateUserFromToken(token: string | null) {
  if (!token) {
    state.user = null;
    state.isAuthenticated = false;
    delete api.defaults.headers.common['Authorization'];
    return;
  }

  try {
    const decoded = jwtDecode<{ exp: number }>(token);
    if (decoded.exp * 1000 > Date.now()) {
      state.user = jwtDecode<User>(token);
      state.isAuthenticated = true;
      api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    } else {
      // Token is expired
      localStorage.removeItem(TOKEN_KEY);
      state.user = null;
      state.isAuthenticated = false;
      delete api.defaults.headers.common['Authorization'];
    }
  } catch (error) {
    console.error('Error decoding token:', error);
    localStorage.removeItem(TOKEN_KEY);
    state.user = null;
    state.isAuthenticated = false;
    delete api.defaults.headers.common['Authorization'];
  }
}

const authService = {
  // --- Readonly State ---
  state: readonly(state),

  async login(credentials: AuthCredentials): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/login', credentials);
    if (response.data.success && response.data.token) {
      localStorage.setItem(TOKEN_KEY, response.data.token);
      updateUserFromToken(response.data.token);
    }
    return response.data;
  },

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    updateUserFromToken(null);
  },

  async register(data: RegisterData): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/register', data);
    if (response.data.success && response.data.token) {
      localStorage.setItem(TOKEN_KEY, response.data.token);
      updateUserFromToken(response.data.token);
    }
    return response.data;
  },

  initialize(): void {
    const token = localStorage.getItem(TOKEN_KEY);
    updateUserFromToken(token);
  },
};

authService.initialize();

export default authService; 