import api from './api';
import { jwtDecode } from 'jwt-decode';
import { reactive, readonly } from 'vue';

// --- Interfaces ---

export interface User {
  id: number;
  nameid: string;
  /** Нет в JWT, если email в профиле не задан */
  email?: string;
  unique_name: string;
  /** jwt-decode даёт readonly-массив для claim `role` — совместимость с шаблонным JwtPayload */
  role: string | readonly string[];
}

export interface AuthCredentials {
  username?: string;
  password?: string;
}

export interface RegisterData {
  username?: string;
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

  /** После восстановления сессии из storage — новый JWT с продлённым сроком (сервер /auth/refresh). */
  async refreshSessionIfAuthenticated(): Promise<void> {
    if (!state.isAuthenticated) return;

    try {
      const response = await api.post<AuthResponse>('/auth/refresh');
      if (response.data.success && response.data.token) {
        localStorage.setItem(TOKEN_KEY, response.data.token);
        updateUserFromToken(response.data.token);
      }
    } catch {
      // 401 обрабатывает interceptor; при сетевой ошибке оставляем текущий токен.
    }
  },

  /** После смены своей роли сервер может вернуть новый JWT. */
  setToken(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
    updateUserFromToken(token);
  },
};

authService.initialize();
void authService.refreshSessionIfAuthenticated();

export default authService;
