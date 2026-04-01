import axios, { type AxiosInstance, type AxiosResponse, isAxiosError } from 'axios';
import { notifyApiError } from './apiErrorNotifier';

export class OfflineQueuedError extends Error {
  constructor() {
    super('Request queued for background sync');
    this.name = 'OfflineQueuedError';
  }
}

export function isOfflineQueued(err: unknown): err is OfflineQueuedError {
  return err instanceof OfflineQueuedError;
}

/** Ответ SW при постановке мутации в очередь (без сетевого провала страницы). */
function isOfflineQueuedAxiosResponse(response: AxiosResponse): boolean {
  if (response.status !== 202) return false;
  const raw = response.headers['x-cigarhelper-offline-queued'];
  const v = Array.isArray(raw) ? raw[0] : raw;
  return v === '1';
}

// fetch — чтобы запросы шли через Service Worker (очередь офлайн-мутаций).
// По умолчанию Axios в браузере использует XHR, который SW не видит.
const api: AxiosInstance = axios.create({
  baseURL: '/api',
  adapter: 'fetch',
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
  (response) => {
    if (isOfflineQueuedAxiosResponse(response)) {
      notifyApiError({
        summary: 'Действие в очереди',
        detail: 'Запрос сохранён и будет отправлен при восстановлении сети.',
        severity: 'info',
      });
      return Promise.reject(new OfflineQueuedError());
    }
    return response;
  },
  (error: unknown) => {
    if (isOfflineQueued(error)) {
      return Promise.reject(error);
    }

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
      const method = isAxiosError(error) ? error.config?.method?.toUpperCase() : undefined;
      const isMutation = method === 'POST' || method === 'PUT' || method === 'DELETE';

      if (!navigator.onLine && isMutation) {
        notifyApiError({
          summary: 'Действие в очереди',
          detail: 'Запрос сохранён и будет отправлен при восстановлении сети.',
          severity: 'info',
        });
        return Promise.reject(new OfflineQueuedError());
      } else {
        notifyApiError({
          summary: 'Сеть недоступна',
          detail: error.message || 'Проверьте подключение и повторите попытку.',
          severity: 'error',
        });
      }
    }

    return Promise.reject(error);
  },
);

export default api;
