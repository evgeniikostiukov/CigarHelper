---
name: vue-3-typescript
description: Guides Vue 3 with TypeScript using Composition API, `<script setup>`, typed props/emits, Vue Router, and Vite. Use when writing or refactoring Vue SFCs, composables, router, or frontend TypeScript in this codebase or when the user mentions Vue 3, Vue + TS, or Vite frontend work.
---

# Vue 3 + TypeScript

## Defaults

- **API**: Composition API with `<script setup>` (не Options API, если в файле уже не legacy).
- **Именование**: PascalCase для компонентов; composables — `useXxx`.
- **Импорты**: алиас `@` → `src` (см. Vite `resolve.alias`).

## SFC

- Используй `defineProps` / `defineEmits` с дженериками или `withDefaults` для значений по умолчанию.
- Для моделей — `defineModel` (в проекте включены `defineModel` и `propsDestructure` в `@vitejs/plugin-vue`).
- Разделяй: презентация в шаблоне, логика в composables/функциях; избегай огромных `<script setup>` без разбиения.

## Типы

- Явно типизируй публичные props и возвращаемые значения composables (`Ref<T>`, интерфейсы DTO).
- Для ref из шаблона — корректные generic (`ref<HTMLInputElement | null>(null)`).
- Не злоупотребляй `any`; для API — типы рядом с сервисами или импорт из общих `types`.

## Маршрутизация

- Vue Router 4: `createRouter`, `createWebHistory`, lazy `import()` для тяжёлых страниц.
- Защита маршрутов — через `meta` и навигационные хуки; не дублируй проверки без нужды.

## Состояние и данные

- Локальное UI-состояние — `ref` / `reactive`; общее — по необходимости отдельный store или composable singleton (в проекте Pinia может не быть — не добавляй без запроса).
- HTTP — существующий слой (`axios` и сервисы); новые вызовы — через те же паттерны и базовый URL/proxy.

## Стили и UI

- В этом репозитории: **PrimeVue** (часто автоимпорт через `unplugin-vue-components`), **Tailwind** — следуй текущим классам и компонентам PrimeVue, не смешивай произвольно новые UI-библиотеки.

## Сборка и проверка

- Dev: `npm run dev` (Vite). Сборка: `npm run build`; типы Vue: `vue-tsc` при необходимости.
- После правок — учитывай ESLint/Prettier проекта (`eslint-plugin-vue`, `typescript-eslint`).

## Антипаттерны

- Не переводить рабочий `<script setup>` на Options API «для единообразия».
- Не вставлять крупную бизнес-логику в шаблон (`v-if` с тяжёлыми выражениями — вынести в computed/функции).
- Не хардкодить базовый URL API, если уже настроен proxy (`/api` → backend).

## Когда читать дальше

- Нестандартная интеграция (Tiptap, кастомные директивы) — смотри существующие модули в `src` и повторяй паттерн.
