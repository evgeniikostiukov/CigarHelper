# Frontend — overview

## Расположение

Проект: **`CigarHelper.Web/`** (отдельно от `CigarHelper.sln`, Node/npm).

## Стек

- **Vue 3** (Composition API, `<script setup>`, TypeScript).
- **Vue Router 4** — история `createWebHistory`, guards по `meta.requiresAuth`, `requiresAdmin`, опционально `requiresAnyRole`.
- **Vite** — алиас `@` → `src/`, auto-import PrimeVue через `unplugin-vue-components` + `PrimeVueResolver`, Vue DevTools в dev.
- **PrimeVue 4** + **@primeuix/themes** (база Aura, кастомный пресет в `main.ts` — см. ниже), иконки PrimeIcons.
- **Tailwind CSS 4** + **tailwindcss-primeui**, PostCSS.
- **Axios** — единый клиент `src/services/api.ts`, `baseURL: '/api'`.
- **jwt-decode** — разбор JWT в `authService.ts`; зависимость объявлена в `CigarHelper.Web/package.json`. Регистрация и вход по **логину** (username); claim `email` в JWT есть только если пользователь указал email в профиле.
- **TipTap** (+ Vue 3) — **TextEditor.vue**; **DOMPurify** — санитизация HTML где нужно.
- **@vueuse/core** — утилиты.

## Тема и палитра (UI)

- **`src/main.ts`**: `definePreset` из `@primeuix/styled` на базе `Aura` — пресет **CigarAura**. Акцент **primary** = палитра **rose**. **Surface** = **stone**. `darkModeSelector: '.dark'` — PrimeVue переключается синхронно с Tailwind.
- **Tailwind**: класс-based dark mode через `@custom-variant dark (&:where(.dark, .dark *))` в `main.css`. Классы `dark:` активируются при `.dark` на `<html>`.
- **`src/assets/main.css`**: семантические CSS-переменные (`--c-bg`, `--c-surface`, `--c-border`, `--c-text`, `--c-text-muted`, `--c-accent`) в `:root` и `.dark` блоках. В `@theme` зарегистрированы как Tailwind-утилиты (`bg-theme-bg`, `text-theme-text` и т.д.).
- **`src/composables/useTheme.ts`**: синглтон-composable `useTheme()`. Использует `useDark` из `@vueuse/core` (читает system preference, сохраняет в `localStorage` ключ `cigar-color-scheme`). Экспортирует `{ isDark, toggleTheme, setTheme }`.
- **`src/components/ThemeToggle.vue`**: кнопка переключения темы (иконка ☀️/🌙). Добавлена в `App.vue` header (`data-testid="theme-toggle"`).
- **`src/App.vue`**: градиент оболочки (`stone-50` + лёгкий **`rose-50`** / в dark **`rose-950`**), hover и focus пунктов Menubar под розовую гамму.

## Тексты в UI (подсказки и мета-пояснения)

Когда в подзаголовках страниц, hero-блоках, карточках фич или вспомогательных абзацах есть **пояснение «про приложение»** (не подпись к полю формы и не сообщение об ошибке):

- формулировки должны быть **краткими и понятными конечному пользователю**;
- **не упоминать** технические детали реализации: JWT, прокси, брейкпоинты, «как на десктопе/телефоне», единый стиль с другими экранами, устройство вёрстки (отступы, крупные кнопки), внутренние названия модулей;
- допустимо описывать **смысл для пользователя**: что он видит или что может сделать (коллекция, фильтры, роли администратора — без объяснения, *как* это сверстано).

Такие тексты живут преимущественно в **`src/views/*.vue`**; при добавлении новых страниц придерживаться этого же критерия.

**Детали правил расчёта или загрузки фото:** короткая строка у элемента + при необходимости иконка **`pi-info-circle`** с **`v-tooltip`** (см. плитки **Dashboard**, блок «Добавить сигары» в **HumidorDetail**, **`FormImageGallerySection`**: `url-help-text` + `url-help-detail`). В tooltip — чуть больше контекста, всё ещё без жаргона разработчика.

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
