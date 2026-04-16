import 'axios';

declare module 'axios' {
  interface AxiosRequestConfig {
    /** Не показывать глобальные toast при ошибке (например, опциональные превью из MinIO). */
    skipGlobalErrorNotification?: boolean;
  }

  interface InternalAxiosRequestConfig {
    skipGlobalErrorNotification?: boolean;
  }
}
