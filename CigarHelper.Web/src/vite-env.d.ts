/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Ключ приложения Countly (из панели → Web). */
  readonly VITE_COUNTLY_APP_KEY?: string;
  /**
   * Базовый URL Countly без завершающего `/`.
   * В dev удобно `http://localhost:3000` + прокси `/i` и `/o` в `vite.config.js`.
   */
  readonly VITE_COUNTLY_URL?: string;
}
