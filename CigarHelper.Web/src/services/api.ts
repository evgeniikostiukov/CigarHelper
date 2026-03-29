import axios, { type AxiosInstance, isAxiosError } from 'axios';
import { notifyApiError } from './apiErrorNotifier';

const api: AxiosInstance = axios.create({
  baseURL: '/api',
});

// Add a request interceptor to include the auth token in every request
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  },
);

// Ответы: 401 — сброс сессии; 403/5xx и прочие ошибки — единый toast (без 404, чтобы не дублировать экраны).
api.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    if (isAxiosError(error) && error.response?.status === 401) {
      localStorage.removeItem('authToken');
      localStorage.removeItem('user');
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
      return Promise.reject(error);
    }

    if (isAxiosError(error) && error.response) {
      const status = error.response.status;
      const data = error.response.data as { message?: string; title?: string } | string | undefined;
      const detailFromBody =
        typeof data === 'string'
          ? data
          : typeof data?.message === 'string'
            ? data.message
            : typeof data?.title === 'string'
              ? data.title
              : '';

      if (status === 403) {
        notifyApiError({
          summary: 'Доступ запрещён',
          detail: detailFromBody || 'Недостаточно прав для этой операции.',
          severity: 'warn',
        });
      } else if (status >= 500) {
        notifyApiError({
          summary: 'Ошибка сервера',
          detail: detailFromBody || 'Попробуйте позже.',
          severity: 'error',
        });
      } else if (status !== 404 && status >= 400) {
        notifyApiError({
          summary: 'Ошибка запроса',
          detail: detailFromBody || `Код ${status}`,
          severity: 'error',
        });
      }
    } else if (error instanceof Error) {
      notifyApiError({
        summary: 'Сеть недоступна',
        detail: error.message || 'Проверьте подключение и повторите попытку.',
        severity: 'error',
      });
    }

    return Promise.reject(error);
  },
);

export default api;
