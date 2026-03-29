# Workflow — CigarHelper.Web

## Установка

Из каталога **`CigarHelper.Web`**:

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

## Проверки качества (локально и CI)

Скрипты в **`package.json`**:

```bash
npm run typecheck   # vue-tsc
npm run lint        # ESLint (flat config)
npm run test        # Vitest, unit-тесты (*.test.ts)
npm run ci          # typecheck + lint + test + build
```

В корне репозитория workflow **GitHub Actions** (`.github/workflows/ci.yml`): `dotnet test CigarHelper.sln` и шаги фронта выше.

## Линтинг

**ESLint 9** (`eslint.config.js`) и **Prettier** (через `eslint-plugin-prettier` + `eslint-config-prettier`). Игнорируются сгенерированные файлы (`components.d.ts`, `vitest.config.ts` и т.д.).

## Согласование с backend

- JWT и cookie-less схема: только заголовок Bearer из `localStorage`.
- CORS в проде: origin фронта должен быть в `Cors:Origins` API (см. security memory bank). Локальный dev чаще обходится Vite-прокси без отдельной CORS-настройки для браузера.
