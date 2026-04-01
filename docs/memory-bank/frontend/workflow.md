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

## PWA / Service Worker

Приложение — полноценное PWA: manifest, иконки, установка на домашний экран, офлайн-кеш GET-запросов, BackgroundSync-очередь мутаций.

### Стек

- **`vite-plugin-pwa`** (Workbox), стратегия **`injectManifest`** — кастомный SW в `src/sw.ts`.
- Override peer `"vite-plugin-pwa": { "vite": "$vite" }` в `package.json` (плагин ещё не объявил peer Vite 8; работает штатно).

### Иконки

Исходник — `public/logo.svg`; генерация в `public/` через `@vite-pwa/assets-generator`:

```bash
npm run generate:pwa-assets   # pwa-assets-generator (конфиг pwa-assets.config.ts)
```

Генерирует: `favicon.ico`, `pwa-64x64.png`, `pwa-192x192.png`, `pwa-512x512.png`, `maskable-icon-512x512.png`, `apple-touch-icon-180x180.png`.

### Стратегии кеширования (в `src/sw.ts`)

| Маршрут | Стратегия |
|---------|-----------|
| Build assets | **Precache** (Workbox) |
| `GET /api/(humidors\|cigars\|dashboard\|reviews\|brands)` | **NetworkFirst** (TTL 1h) |
| `GET /api/cigars/bases/*` | **StaleWhileRevalidate** (TTL 24h) |
| `GET /api/cigar-images/*/thumbnail\|data` | **CacheFirst** (TTL 30d, max 200) |
| `GET /api/search` | **NetworkOnly** |
| `POST/PUT/DELETE /api/*` | **NetworkOnly + BackgroundSync** (очередь `cigar-helper-mutations`, 7 дней) |

Экземпляр Axios в **`src/services/api.ts`** должен использовать **`adapter: 'fetch'`**: по умолчанию в браузере идёт XHR, который Service Worker не перехватывает, поэтому мутации не попадали бы в очередь.

При постановке в очередь SW отвечает **202** с заголовком **`X-CigarHelper-Offline-Queued: 1`**, чтобы страничный `fetch` не завершался сетевой ошибкой (`net::ERR_INTERNET_DISCONNECTED` в консоли). В **`api.ts`** это превращается в **`OfflineQueuedError`**. Если `navigator.onLine === false`, SW **не вызывает** сетевой `fetch` для мутации — только кладёт запрос в очередь.

Для **GET** списков (`/api/humidors`, `/api/cigars`, …) при **`!navigator.onLine`** SW не вызывает сеть: только **`api-lists`** cache, иначе синтетический JSON (пустые массивы / нулевая сводка дашборда), чтобы не было `net::ERR_*` от `NetworkFirst`.

Воспроизведение очереди мутаций: один общий **`drainMutationQueue`** для **`onSync`**, **`REPLAY_QUEUE`** и эвристики на **GET** (через цепочку промисов — без параллельных `shiftRequest`). В **`activate`** счётчик **`pendingCount`** подтягивается из IndexedDB и рассылается клиентам, чтобы после перезапуска SW UI не терял длину очереди.

### Composables (в `src/composables/`)

| Файл | Назначение |
|------|-----------|
| `usePwaUpdate.ts` | Prompt for update (toast «Доступно обновление» + кнопка «Обновить») |
| `useOnlineStatus.ts` | Реактивный `isOnline`, toast при переходах online/offline |
| `usePendingSync.ts` | `pendingCount`, `lastError` — postMessage от SW |
| `useInstallPrompt.ts` | `canInstall`, `install()` — beforeinstallprompt |

### UI (App.vue)

- **Офлайн-баннер**: жёлтый sticky `z-60` при `!isOnline`, показывает число действий в очереди.
- **Update-баннер**: синий sticky при `needRefresh`, кнопка «Обновить» / «Закрыть».
- **Sync-badge**: счётчик ожидающих мутаций в шапке (анимированная иконка `pi-sync`).
- **Install-кнопка**: `pi-download` в шапке, только когда `canInstall === true`.

### Ограничения MVP

- **Зависимые операции**: создание сущности офлайн + последующая привязка к ней — второй запрос может не найти ID при воспроизведении. Пользователь увидит предупреждение.
- **JWT**: токен может истечь за время офлайна; после синхронизации потребуется re-login.

### Замечания к сборке

- **Tailwind CSS v4:** не использовать устаревшие утилиты вида `bg-opacity-*` — прозрачность задаётся через слэш (`bg-red-500/70`, `bg-black/0`, `group-hover:bg-black/50`). Иначе при `vite build` возможны предупреждения про неизвестные классы. В `<style>` с `@apply` к кастомной теме из `main.css` может понадобиться `@reference` на этот файл (см. документацию Tailwind v4).
- **Lightning CSS / корневой шел:** глобальные стили лейаута и Menubar вынесены в **`src/assets/app-shell.css`** (подключается из `main.css`), без `scoped` и без `:deep`/`:global` в `App.vue` — так проще избежать предупреждений при минификации. В остальных SFC `:deep`/`:global` для PrimeVue — осознанный компромисс.
- **PrimeIcons:** подключать **`import 'primeicons/primeicons.css'`** из **`main.ts`** (до `main.css`), а не через `@import` внутри `main.css` / цепочки Tailwind — иначе Vite не переписывает `url(./fonts/*)` и сыпятся предупреждения при сборке; в `dist/assets/` появляются хешированные woff2/ttf и т.д.
- **Vue DevTools:** **`vite-plugin-vue-devtools` 8.x** объявляет peer **Vite ^6–8**. У плагина транзитивно был **`vite-plugin-inspect` 11.x** с peer только Vite 6–7 (ложные `invalid` в `npm ls`). В **`package.json`** задан **`overrides`**: у поддерева devtools поднят **`vite-plugin-inspect@12.0.0-beta.1`**, у которого peer **Vite ^8** — после `npm install` дерево чистое. В **`vite.config.js`** DevTools подключаются **только при `command === 'serve'`** (`npm run dev`), не в `vite build` / CI.

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

## OpenAPI — источник правды + генерация типов

Инструменты: **`Swashbuckle.AspNetCore.Cli`** (dotnet local tool, `dotnet-tools.json` в корне) + **`openapi-typescript`** (devDependency фронта).

### Файлы (оба коммитятся)

| Файл | Описание |
|------|----------|
| `openapi.json` | Спецификация OpenAPI (источник правды) |
| `CigarHelper.Web/src/types/api.generated.ts` | Авто-сгенерированные TypeScript-типы |

### Обновление типов при изменении API-контракта

```bash
# 1. Сборка API
dotnet build CigarHelper.API/CigarHelper.Api.csproj -c Debug

# 2. Генерация спецификации (в корне репозитория)
dotnet tool run swagger tofile --output openapi.json \
  "CigarHelper.API/bin/Debug/net10.0/CigarHelper.Api.dll" v1
# PowerShell: установить $env:ASPNETCORE_ENVIRONMENT = "Development" перед командой

# 3. Генерация TypeScript-типов (из CigarHelper.Web)
npm run generate:api

# 4. Обновить тест-контракт при переименовании полей
#    src/types/api.types.test.ts → npm run typecheck
```

Либо, если API запущен на `localhost:5184`:
```bash
# Из CigarHelper.Web:
npm run generate:spec   # скачивает openapi.json из /swagger/v1/swagger.json
npm run generate:api    # перегенерирует api.generated.ts
```

### Использование сгенерированных типов

```typescript
// Публичный фасад — всегда импортировать из @/types
import type { ApiCigarResponse, ApiDashboardSummary } from '@/types';

// Сырые схемы при необходимости
import type { components } from '@/types/api.generated';
type RawDto = components['schemas']['CigarResponseDto'];
```

- **Сервисный слой нормализует** `optional/nullable` поля API → компоненты получают определённые типы (см. `dashboardService.ts`).
- **TDD-тест контракта:** `src/types/api.types.test.ts` — при переименовании DTO typecheck упадёт раньше, чем сломается компонент.

## Согласование с backend

- JWT и cookie-less схема: только заголовок Bearer из `localStorage`.
- CORS в проде: origin фронта должен быть в `Cors:Origins` API (см. security memory bank). Локальный dev чаще обходится Vite-прокси без отдельной CORS-настройки для браузера.

## E2E (Playwright)

Интеграционные браузерные тесты живут в корневом каталоге **`e2e/`** (не внутри `CigarHelper.Web`). Краткая инструкция: **[`e2e/README.md`](../../../e2e/README.md)**. Полный порядок «БД → API → Vite → `npm test` в `e2e/`» и заметки про тестового пользователя и env — в **[`docs/memory-bank/workflow.md`](../workflow.md)** (раздел «E2E (Playwright)»).

- **Фронт:** `http://localhost:3000` (`npm run dev`).
- **API:** прокси с фронта на **`http://localhost:5184`** (см. `vite.config.js`); при смене порта API обновить `server.proxy['/api'].target`.
