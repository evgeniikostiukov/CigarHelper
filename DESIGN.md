# DESIGN — CigarHelper (UI и фронт-решения)

**Назначение:** зафиксировать **принятые визуальные и UX-решения** и стек UI, чтобы не разъезжались тема, палитра и паттерны между чатами и PR. **После изменений темы, глобальных стилей или ключевых layout-паттернов** обновляй этот файл и при необходимости `docs/memory-bank/frontend/overview.md` (там детальный memory bank).

**Источник правды по структуре экранов:** `docs/memory-bank/frontend/` (маршруты, списки, `data-testid`).

---

## Стек UI

| Слой | Выбор |
|------|--------|
| Фреймворк | Vue 3, Composition API, `<script setup>`, TypeScript |
| Сборка | Vite (см. `CigarHelper.Web/package.json` для актуальной major) |
| Компоненты | PrimeVue 4, PrimeIcons |
| Тема Prime | `@primeuix/themes`, база **Aura**, кастомный пресет в `src/main.ts` |
| Утилитарные стили | Tailwind CSS 4 + `tailwindcss-primeui` |
| Запросы | Axios, `baseURL: '/api'` |
| Редактор HTML | TipTap (+ DOMPurify где нужна санитизация) |

---

## Тема и палитра (текущее состояние)

- **Пресет:** `definePreset` на базе `Aura` (**CigarAura** в `src/main.ts`).
- **Primary:** палитра **rose** (акцент кнопок, ссылки, акцентные состояния).
- **Surface:** **stone** в светлой и тёмной схеме (нейтральный фон компонентов).
- **Tailwind на экранах:** акценты **`rose-*`**; фоны страниц осветлены (**`from-stone-50` / `to-stone-50`** и аналоги там, где был общий градиент).
- **`src/assets/main.css`:** в `@theme` задан **`--color-app-body`** — цвет текста `body` вне PrimeVue (тёплый нейтраль).
- **`App.vue`:** градиент оболочки (`stone-50` + лёгкий **`rose-50`**; в dark — **`rose-950`**), hover/focus пунктов Menubar в розовой гамме.

Тёмная тема: переключение и наследование — как реализовано в `App.vue` / PrimeVue (не дублировать здесь детали кода; при смене механизма — обновить этот абзац).

---

## Паттерны UX

- **Оболочка:** `App.vue` — Menubar, `router-view` в `Suspense`, глобальные Toast / ConfirmDialog.
- **Аутентификация:** токен в `localStorage`, guards по `meta.requiresAuth` / `requiresAdmin`; при 401 — сброс сессии и редирект на логин (см. `authService`, interceptor в API-клиенте).
- **Публичные страницы:** профиль/хьюмидор по slug и т.д. — без обязательного логина; детали в frontend memory bank.

---

## Что обновлять при изменениях

1. Смена **primary/surface** или пресета Aura → раздел «Тема и палитра» + `docs/memory-bank/frontend/overview.md`.
2. Новый **глобальный** UI-паттерн (например единый empty state) → кратко здесь + при необходимости `collection-list-views.md`.
3. Крупный редизайн → строка в **`TODO.md`** (журнал) и при необходимости задачи P2/P3 (скриншотные тесты, a11y).

---

## Не дублировать

Детальная карта views, порт прокси Vite→API, скрипты npm — в **`docs/memory-bank/frontend/`**. Безопасность API и CORS — в **`docs/security-refactor-memory-bank.md`**.
