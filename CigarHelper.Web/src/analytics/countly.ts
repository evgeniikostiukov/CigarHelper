import CountlyModule from 'countly-sdk-web';
import type CountlyApi from 'countly-sdk-web';

let initialized = false;
/** Ссылка на SDK после успешного `init` (один раз снимаем обёртки CJS `default`). */
let countlyRef: typeof CountlyApi | null = null;

/**
 * countly-sdk-web (CJS) в Vite/Node отдаёт цепочку `default` → объект с `init`.
 * Иногда двойная: `{ default: { default: Countly } }`.
 */
function getCountly(): typeof CountlyApi | null {
  let cur: unknown = CountlyModule;
  for (let i = 0; i < 6; i += 1) {
    if (cur !== null && typeof cur === 'object' && typeof (cur as { init?: unknown }).init === 'function') {
      return cur as typeof CountlyApi;
    }
    if (cur !== null && typeof cur === 'object' && 'default' in cur) {
      cur = (cur as { default: unknown }).default;
      continue;
    }
    return null;
  }
  return null;
}

function normalizeCountlyUrl(url: string): string {
  return url.replace(/\/+$/, '');
}

/**
 * Включает Countly Web SDK, если заданы `VITE_COUNTLY_APP_KEY` и `VITE_COUNTLY_URL`.
 * Иначе — безопасный no-op (сборки и окружения без аналитики).
 */
export function initCountly(): void {
  if (initialized) {
    return;
  }
  const appKey = import.meta.env.VITE_COUNTLY_APP_KEY as string | undefined;
  const url = import.meta.env.VITE_COUNTLY_URL as string | undefined;
  if (!appKey?.trim() || !url?.trim()) {
    return;
  }

  const Countly = getCountly();
  if (!Countly) {
    if (import.meta.env.DEV) {
      console.warn('[countly] Пакет countly-sdk-web: неожиданная форма экспорта, init недоступен');
    }
    return;
  }

  Countly.init({
    app_key: appKey.trim(),
    url: normalizeCountlyUrl(url.trim()),
    debug: import.meta.env.DEV,
  });

  Countly.q = Countly.q || [];
  Countly.q.push(['track_sessions']);
  countlyRef = Countly;
  initialized = true;
}

export function isCountlyEnabled(): boolean {
  return initialized;
}

/** После навигации Vue Router (в т.ч. первый заход). */
export function trackCountlyPageView(fullPath: string): void {
  if (!initialized || !countlyRef) {
    return;
  }
  countlyRef.q.push(['track_pageview', fullPath]);
}
