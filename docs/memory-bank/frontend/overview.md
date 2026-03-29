# Frontend — overview

## Расположение

Проект: **`CigarHelper.Web/`** (отдельно от `CigarHelper.sln`, Node/npm).

## Стек

- **Vue 3** (Composition API, `<script setup>`, TypeScript).
- **Vue Router 4** — история `createWebHistory`, guards по `meta.requiresAuth`, `requiresAdmin`, опционально `requiresAnyRole`.
- **Vite 7** — алиас `@` → `src/`, auto-import PrimeVue через `unplugin-vue-components` + `PrimeVueResolver`, Vue DevTools в dev.
- **PrimeVue 4** + **@primeuix/themes** (preset Aura), иконки PrimeIcons.
- **Tailwind CSS 4** + **tailwindcss-primeui**, PostCSS.
- **Axios** — единый клиент `src/services/api.ts`, `baseURL: '/api'`.
- **jwt-decode** — разбор JWT в `authService.ts`; зависимость объявлена в `CigarHelper.Web/package.json`.
- **TipTap** (+ Vue 3) — **TextEditor.vue**; **DOMPurify** — санитизация HTML где нужно.
- **@vueuse/core** — утилиты.

## Точка входа

- `src/main.ts` — приложение, router, PrimeVue (тема, слои CSS), ConfirmationService, ToastService.
- `src/App.vue` — Menubar, `router-view` в `Suspense`, Toast/ConfirmDialog, меню и выход.

## Аутентификация

- Токен: **`localStorage`** ключ `authToken`; при логине/регистрации сохраняется, в **axios** подставляется заголовок `Authorization: Bearer …`.
- Пользователь из payload JWT через `jwt-decode`; роли читаются в **`src/utils/roles.ts`** (`getRoleClaims`, `hasRole`, `hasAnyRole`). Поле `unique_name` и др. — см. **`authService.ts`** (`User`).
- При **401** interceptor очищает `authToken` и `user`, редирект на `/login` (полная перезагрузка страницы).
- **`authService.initialize()`** при импорте модуля восстанавливает сессию из `localStorage`.

## Связь с API в разработке

В **`vite.config.js`**: `server.port = 3000`, прокси **`/api` → `http://localhost:5184`**. Backend должен слушать тот же порт, что в прокси (или изменить target под порт API). Браузер ходит на `http://localhost:3000/api/...`, Vite проксирует на Kestrel без CORS-проблем в типичном локальном сценарии.
