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

## Линтинг

В проекте есть **ESLint 9** (`eslint.config.js`) и **Prettier**. Команды в `package.json` не добавлены — при необходимости: `npx eslint src` / интеграция в IDE.

## Согласование с backend

- JWT и cookie-less схема: только заголовок Bearer из `localStorage`.
- CORS в проде: origin фронта должен быть в `Cors:Origins` API (см. security memory bank). Локальный dev чаще обходится Vite-прокси без отдельной CORS-настройки для браузера.
