# Workflow — CigarHelper.Web

## Установка

**Node.js:** ветка **24.x**, эталон **24.14.1** — см. **`CigarHelper.Web/.nvmrc`**, `engines` в `package.json` и CI. Из каталога **`CigarHelper.Web`**:

```bash
cd CigarHelper.Web
npm install
```

## Разработка

```bash
npm run dev
```

По умолчанию Vite: **порт 3000**, `host: true` (доступ в LAN). Запросы к **`/api/*`** проксируются на **`http://localhost:5184`** — перед этим поднять API (`dotnet run` для `CigarHelper.Api`) на этом порту или поправить `vite.config.js` → `server.proxy['/api'].target`.

## Сборка и превью

```bash
npm run build
npm run preview
```

Для production обычно настраивают обратный прокси или static host так, чтобы префикс `/api` уходил на тот же backend; `baseURL: '/api'` в axios остаётся относительным от origin фронта.

### Замечания к сборке

- **Tailwind CSS v4:** не использовать устаревшие утилиты вида `bg-opacity-*` — прозрачность задаётся через слэш (`bg-red-500/70`, `bg-black/0`, `group-hover:bg-black/50`). Иначе при `vite build` возможны предупреждения про неизвестные классы. В `<style>` с `@apply` к кастомной теме из `main.css` может понадобиться `@reference` на этот файл (см. документацию Tailwind v4).
- **Lightning CSS / корневой шел:** глобальные стили лейаута и Menubar вынесены в **`src/assets/app-shell.css`** (подключается из `main.css`), без `scoped` и без `:deep`/`:global` в `App.vue` — так проще избежать предупреждений при минификации. В остальных SFC `:deep`/`:global` для PrimeVue — осознанный компромисс.
- **PrimeIcons:** подключать **`import 'primeicons/primeicons.css'`** из **`main.ts`** (до `main.css`), а не через `@import` внутри `main.css` / цепочки Tailwind — иначе Vite не переписывает `url(./fonts/*)` и сыпятся предупреждения при сборке; в `dist/assets/` появляются хешированные woff2/ttf и т.д.
- **Vue DevTools** подключаются в Vite только в режиме `serve`, чтобы не тянуть цепочку зависимостей с peer Vite 7 в production-сборку.

## Проверки качества (локально и CI)

Скрипты в **`package.json`**:

```bash
npm run typecheck   # vue-tsc
npm run lint        # ESLint (flat config)
npm run test        # Vitest + happy-dom; unit (*.test.ts), компоненты через @vue/test-utils
npm run ci          # typecheck + lint + test + build
```

В корне репозитория workflow **GitHub Actions** (`.github/workflows/ci.yml`): `dotnet test CigarHelper.sln` и шаги фронта выше.

## Линтинг

**ESLint 10** (`eslint.config.js`) и **Prettier** (через `eslint-plugin-prettier` + `eslint-config-prettier`). Игнорируются сгенерированные файлы (`components.d.ts`, `vitest.config.ts` и т.д.).

## Согласование с backend

- JWT и cookie-less схема: только заголовок Bearer из `localStorage`.
- CORS в проде: origin фронта должен быть в `Cors:Origins` API (см. security memory bank). Локальный dev чаще обходится Vite-прокси без отдельной CORS-настройки для браузера.

## E2E (Playwright)

Интеграционные браузерные тесты живут в корневом каталоге **`e2e/`** (не внутри `CigarHelper.Web`). Краткая инструкция: **[`e2e/README.md`](../../../e2e/README.md)**. Полный порядок «БД → API → Vite → `npm test` в `e2e/`» и заметки про тестового пользователя и env — в **[`docs/memory-bank/workflow.md`](../workflow.md)** (раздел «E2E (Playwright)»).

- **Фронт:** `http://localhost:3000` (`npm run dev`).
- **API:** прокси с фронта на **`http://localhost:5184`** (см. `vite.config.js`); при смене порта API обновить `server.proxy['/api'].target`.
