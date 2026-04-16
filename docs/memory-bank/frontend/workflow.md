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
- **Vue DevTools:** **`vite-plugin-vue-devtools` 8.x** объявляет peer **Vite ^6–8**. У плагина транзитивно был **`vite-plugin-inspect` 11.x** с peer только Vite 6–7 (ложные `invalid` в `npm ls`). В **`package.json`** задан **`overrides`**: у поддерева devtools поднят **`vite-plugin-inspect@12.0.0-beta.1`**, у которого peer **Vite ^8** — после `npm install` дерево чистое. В **`vite.config.js`** DevTools подключаются **только при `command === 'serve'`** (`npm run dev`), не в `vite build` / CI.

## Проверки качества (локально и CI)

Скрипты в **`package.json`**:

```bash
npm run typecheck   # vue-tsc
npm run lint        # ESLint (flat config)
npm run test        # Vitest + happy-dom; unit (*.test.ts), компоненты через @vue/test-utils
npm run ci          # typecheck + lint + test + build
```

В корне репозитория workflow **GitHub Actions** (`.github/workflows/ci.yml`): `dotnet test CigarHelper.sln` и шаги фронта выше. Версия **.NET SDK** для CI задаётся **`global.json`** в корне (`setup-dotnet` с `global-json-file`): указывайте реальный номер SDK вида `10.0.100`, а не `10.0.0` — иначе установщик на раннере не найдёт пакет.

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
dotnet build CigarHelper.Api/CigarHelper.Api.csproj -c Debug

# 2. Генерация спецификации (в корне репозитория)
dotnet tool run swagger tofile --output openapi.json \
  "CigarHelper.Api/bin/Debug/net10.0/CigarHelper.Api.dll" v1
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
