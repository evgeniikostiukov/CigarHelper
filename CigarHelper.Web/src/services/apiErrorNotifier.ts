/** Регистрация колбэка из корня приложения (App.vue + useToast). */
export interface ApiErrorToastPayload {
  summary: string;
  detail: string;
  severity?: 'success' | 'info' | 'warn' | 'error' | 'secondary' | 'contrast';
}

type NotifyFn = (payload: ApiErrorToastPayload) => void;

let notify: NotifyFn | null = null;

export function registerApiErrorNotifier(fn: NotifyFn): void {
  notify = fn;
}

export function notifyApiError(payload: ApiErrorToastPayload): void {
  notify?.(payload);
}
