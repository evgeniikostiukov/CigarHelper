# TODO — CigarHelper

**Назначение:** открытые задачи, приоритеты и журнал выполненного. **После значимых изменений в коде** обновляй этот файл: закрывай пункты (`[x]`), добавляй новые, переноси строку в **«Журнал выполненного»** с датой (формат `YYYY-MM-DD`).

**Связанные документы:** детали UI/темы — [`DESIGN.md`](./DESIGN.md); архитектура и команды — `docs/memory-bank/`; безопасность — `docs/security-refactor-memory-bank.md`.

---

## Приоритеты

| Уровень | Смысл |
|--------|--------|
| **P0** | Безопасность, блокеры релиза, критичные дыры |
| **P1** | Качество продукта, стабильность, заметный UX/наблюдаемость |
| **P2** | Техдолг, предупреждения сборки, расширение тестов |
| **P3** | По желанию, низкая срочность |

Внутри раздела задачи **сверху вниз** — от более срочных к менее.

---

## P0 — безопасность и критичное

- [x] Разобрать **оставшиеся применимые пункты** из `docs/security-refactor-memory-bank.md`: в коде закрыты шаги 1–9; для выката вынесены в [`docs/memory-bank/security-deploy-checklist.md`](./docs/memory-bank/security-deploy-checklist.md). *(Отсылка в `docs/memory-bank/workflow.md` сохраняется.)*
- [ ] **Первый прод-выкат:** пройти [`docs/memory-bank/security-deploy-checklist.md`](./docs/memory-bank/security-deploy-checklist.md) и зафиксировать реальные env/секреты (без коммита в git).
- [x] **API:** лимиты размера тела запроса / загрузок там, где принимаются бинарники или крупный HTML (в духе security memory bank).

---

## P1 — продукт, UX, инфра разработки

- [x] **SPA:** единый слой обработки ошибок API (toast + маппинг 401/403/5xx), меньше дублирования по view.
- [x] **API:** health / readiness эндпоинты для хостинга и мониторинга (если ещё не покрыто текущей конфигурацией).
- [x] **Docker Compose** для локалки: Postgres + API (опционально фронт) одной командой; кратко в memory bank / README.
- [x] **E2E (Playwright):** добавить в репозиторий (`e2e/` или внутри `CigarHelper.Web` — по решению команды).
- [x] **E2E:** описать в README/memory bank, как поднимать API + фронт (порты, тестовый пользователь, env).
- [x] **E2E:** smoke-набор — логин, 2–3 ключевых раздела, сценарий с диалогом/таблицей (например база сигар).
- [ ] **E2E:** подключить прогон в CI (Testing/staging по возможности). *(Пока пропущено по запросу.)*
- [ ] **Коллекция:** экспорт/импорт данных пользователя (CSV/JSON) для бэкапа и переноса.
- [ ] **UX:** дашборд-сводка — объём коллекции, разрез по брендам, недавняя активность/отзывы.
  - [x] API: `DashboardController` + `IDashboardService.GetUserDashboardSummary(userId)` с агрегатами по коллекции и брендам, недавними отзывами.
  - [x] Frontend: маршрут `Dashboard` + блоки «Объём коллекции», «Бренды», «Недавние обзоры» в общем каркасе коллекции.
  - [x] E2E: smoke-journey покрывает открытие `/dashboard` и наличие ключевых блоков.
  - [x] Unit (Vitest): тесты для `Dashboard.vue` и `dashboardService`.
- [x] **UX:** онбординг после регистрации (первый хьюмидор, 1–2 сигары из каталога).
- [x] **UX:** глобальный поиск по бренду/сигаре/хьюмидору и клавиатурные шорткаты.
- [x] **Домен:** история сигары во времени (купил/выкурил), средние сроки, мягкие напоминания «давно не трогал».
- [ ] **Наблюдаемость:** структурированные логи, correlation id, базовые метрики (запросы, ошибки, длительность) после стабилизации API.

---

## P2 — техдолг, тесты, сборка

- [ ] При изменении контрактов API — обновлять или добавлять тесты в `CigarHelper.Api.Tests` до или вместе с правкой фронта.
- [ ] По мере выноса логики из крупных `.vue` — точечные тесты на чистые функции/composables (Vitest).
- [x] Подключить **Vue Test Utils** совместно с Vitest (если ещё не на этапе unit-тестов).
- [x] Подключить прекоммит-хуки: **husky + lint-staged + commitlint** (pre-commit: линт/формат только staged; commit-msg: conventional commits).
- [ ] Компонентные тесты для 1–2 переиспользуемых или сложных компонентов (диалоги, нестандартная логика).
- [ ] Периодически обновлять зависимости (`npm audit`, Browserslist) и устранять критичные уязвимости без ломки сборки.
- [x] Разобраться с предупреждением Tailwind при `vite build` (`unknown utility class`, при необходимости `@reference` / конфиг v4).
- [x] **NuGet `NU1510` / `Microsoft.Extensions.Configuration.Json`:** убраны избыточные прямые ссылки в `CigarHelper.Data` и `CigarHelper.Import` (пакет транзитивен; в `CigarHelper.Api` отдельной ссылки не было).
- [ ] По мере роста бандла — точечный code-splitting тяжёлых экранов (например формы с редактором).
- [ ] Актуализировать `docs/memory-bank/` после крупных изменений API, маршрутов или обязательных секретов.
- [x] Убрать или сузить предупреждения Lightning CSS при `vite build` про `:deep` / `:global` в `App.vue`.
- [x] Согласовать **Vite 8** и **vite-plugin-vue-devtools** (обновить плагин или зафиксировать версии и описать в workflow).
- [x] Разобраться с предупреждениями сборки про шрифты **PrimeIcons** (пути в Vite/CSS или задокументировать как ожидаемое).
- [ ] Когда **typescript-eslint** поддерживает TypeScript 6 — поднять TS и снять потолок **5.9.x** во фронте.
- [ ] Рассмотреть **Dependabot / Renovate** (или регламент) для автоматических PR по зависимостям с лимитами на major.
- [x] **`index.html`:** выставить `lang="ru"` (или иной язык интерфейса), если контент не на английском.
- [x] В `docs/memory-bank/workflow.md` (и при желании `engines` в `CigarHelper.Web/package.json`) зафиксировать **целевую ветку Node**, совпадающую с CI/локальной средой (**24.x**, эталон **24.14.1** в `.nvmrc`).
- [ ] **Контракт API:** OpenAPI как источник правды + генерация типов клиента для `CigarHelper.Web` (или иной регламент синхронизации с ростом контроллеров).
- [ ] **Каталог:** регламент периодического обновления данных (import/scraper), дедуп и при необходимости версионирование записей `CigarBase`.
- [ ] **Медиа:** миниатюры, политика хранения изображений (локально vs объектное хранилище) при росте нагрузки и объёма.

---

## P3 — по необходимости

- [ ] Визуальная регрессия (скриншоты) — при жёсткой дизайн-системе или частых визуальных регрессиях.
- [ ] Контрактные тесты фронт↔бэк (Pact и т.п.) — если интеграционных тестов API станет недостаточно.
- [ ] Сценарии доступности (axe в Playwright) для публичных страниц.
- [ ] **Соц/каталог:** публичные подборки, подписки, лента активности — только если появится явный продуктовый фокус (MVP без перегруза).
- [ ] **Отзывы:** оси оценок (сила, аромат, сочетания) и фильтры/сортировки в каталоге по ним.
- [ ] **PWA / офлайн:** минимум — кэш списков и очередь действий при появлении мобильного сценария.
- [ ] **Монетизация (если актуально):** лимиты по тарифам (хьюмидоры, фото, объём) под покрытие хостинга.

---

## Завершённые этапы (кратко)

- **Этап 0 (CI):** фронт `build` / `lint` / typecheck; `dotnet test` на чистой машине.
- **Этап 1 (бэкенд-тесты):** критичные контроллеры, `ApiAuthorizationAndContractsIntegrationTests`, InMemory в `Program.cs` для сидов.
- **Этап 2 (Vitest):** конфиг, тесты `imageUtils`, `roles`.

---

## Журнал выполненного

- [x] **2026-03-30** — UX: анализ и проектирование дашборда-сводки (объём коллекции, разрез по брендам, недавняя активность/отзывы): выбран отдельный `DashboardController` и `IDashboardService.GetUserDashboardSummary(userId)`, определены ключевые блоки UI и маршрут `Dashboard`.
- [x] **2026-03-30** — API: реализован `DashboardService.GetUserDashboardSummaryAsync(userId)` с агрегатами по хьюмидорам, пользовательским сигарам, брендам и недавним обзорам; добавлены `DashboardController` (`GET /api/dashboard/summary`), DTO `DashboardSummaryDto`/`BrandBreakdownItemDto`/`RecentReviewDto`, регистрация сервиса в `Program.cs` и unit-тесты `DashboardServiceTests` (InMemory EF).
- [x] **2026-03-30** — Frontend: добавлен сервис `dashboardService` (`GET /api/dashboard/summary`), маршрут `Dashboard` (`/dashboard`, requiresAuth) с новым view `Dashboard.vue` в общем каркасе коллекции (панель «Сводка коллекции», блоки объёма, брендов и недавних обзоров, CTA при пустой коллекции); в `App.vue` добавлен пункт меню «Сводка».
- [x] **2026-03-30** — UX: добавлен онбординг после регистрации: новый маршрут `/onboarding` (requiresAuth), флаг `needsOnboarding` в `localStorage`, редирект после регистрации на онбординг, мастер «создать хьюмидор → добавить 1–2 сигары из базы в коллекцию»; unit-тесты `src/views/Onboarding.test.ts`.
- [x] **2026-03-30** — Repo hygiene: подключены git hooks на прекоммит: `husky` + `lint-staged` (eslint/prettier по staged-файлам фронта), `commitlint` (проверка commit message на conventional commits).
- [x] **2026-03-30** — Repo hygiene: добавлены хуки для .NET: `dotnet format` (только staged `*.cs` через `--include`) на pre-commit и `dotnet test CigarHelper.sln -c Release` на pre-push.
- [x] **2026-03-30** — E2E: обновлён `e2e/tests/smoke-journey.spec.ts` — после логина открывается «Сводка» (`/dashboard`), проверяются `data-testid="dashboard"` и `data-testid="dashboard-content"`; `npm test` (Playwright) — ok.
- [x] **2026-03-30** — Unit (Vitest): добавлены `src/views/Dashboard.test.ts` (рендер контента/ошибка/навигация) и `src/services/dashboardService.test.ts` (контракт вызова `/dashboard/summary`); `npm test` в `CigarHelper.Web` — ok.
- [x] **2026-03-30** — Docs (memory bank): обновлены `docs/memory-bank/frontend/code-map.md` и `docs/memory-bank/frontend/collection-list-views.md` — добавлены маршрут/экран `Dashboard` и `dashboardService`.
- [x] **2026-03-30** — Repo hygiene: `.env.development` добавлен в `.gitignore` (локальный флаг `VITE_ENABLE_DEVTOOLS` не коммитим).
- [x] **2026-03-30** — Зависимости: `npx npm-check-updates -u` в `CigarHelper.Web`, корне репо и `cigar-scraper` + `npm install`; TypeScript **^5.9.3** (peer `typescript-eslint@8`); TipTap 3 в `TextEditor.vue`; удалён `@types/dompurify`.
- [x] **2026-03-30** — Фронт: палитра светлее, акцент rose — пресет PrimeVue Aura (`rose` primary, `surface` stone), Tailwind amber→rose, фоны stone, `App.vue` / `main.css`.
- [x] **2026-03-30** — Этап 1 бэкенд: интеграционные тесты 401/403/404, публичный профиль, пагинация; InMemory одно имя БД на хост.
- [x] **2026-03-30** — CI GitHub Actions: `dotnet test` + фронт (`typecheck`, `lint`, `test`, `build`); `eslint-config-prettier`; удалён `.eslintignore`.
- [x] **2026-03-30** — Правки под `vue-tsc`: `User.role`, `getRoleClaims`, `getAuthUserId`; ESLint/Prettier; мелкие правки линта.
- [x] **2026-03-30** — Vitest + `happy-dom`: `vitest.config.ts`, `imageUtils.test.ts`, `roles.test.ts`.
- [x] **2026-03-30** — Добавлены корневые **`TODO.md`** (приоритетный бэклог + журнал) и **`DESIGN.md`** (UI/тема); правило в `.cursor/rules/memory-bank.mdc` ссылается на `TODO.md`.
- [x] **2026-03-30** — P0 security: по `security-refactor-memory-bank.md` шаги 1–9 уже в коде; добавлен операционный чеклист `docs/memory-bank/security-deploy-checklist.md`, пункт бэклога закрыт, открыт явный follow-up на первый прод-выкат.
- [x] **2026-03-30** — E2E: каталог `e2e/` с `@playwright/test`, `playwright.config.ts`, smoke `tests/smoke.spec.ts` (главная + `data-testid`), `README.md` с портами и порядком запуска API/фронта.
- [x] **2026-03-30** — E2E: разделы в `docs/memory-bank/workflow.md` и `docs/memory-bank/frontend/workflow.md` (порты 3000/5184, Docker Postgres, порядок запуска, тестовый пользователь через env `E2E_*`, `PLAYWRIGHT_BASE_URL`); оглавления memory bank обновлены.
- [x] **2026-03-30** — E2E: `e2e/tests/smoke-journey.spec.ts` — регистрация через UI или вход по `E2E_EMAIL`/`E2E_PASSWORD`, хьюмидоры/форма, сигары, обзоры, `/cigar-bases`; описание в `e2e/README.md`.
- [x] **2026-03-30** — Фронт: `@vue/test-utils` + `@vitejs/plugin-vue` в `vitest.config.ts`, smoke `src/vue-test-utils.smoke.test.ts`; закрыт пункт `index.html` `lang="ru"` (уже было); E2E CI в бэклоге помечен как временно пропущенный; `docs/memory-bank/frontend/workflow.md` обновлён.
- [x] **2026-03-30** — NuGet NU1510: удалены прямые ссылки `Microsoft.Extensions.Configuration.Json` из `CigarHelper.Data` и `CigarHelper.Import` (остаётся транзитивно); в API прямой ссылки не было. `dotnet build` / `dotnet test` по solution — ок.
- [x] **2026-03-30** — Tailwind v4: в `CigarBaseEditDialog.vue` заменены `bg-opacity-*` на синтаксис `/opacity`; в `docs/memory-bank/frontend/workflow.md` — заметка по v4 и `@reference` для будущих `@apply` в SFC.
- [x] **2026-03-30** — Lightning CSS / шел: стили из `App.vue` перенесены в `src/assets/app-shell.css` + импорт из `main.css`; `App.vue` без `<style>`. Предупреждений по сборке для шела нет; workflow обновлён.
- [x] **2026-03-30** — PrimeIcons: импорт `primeicons.css` перенесён из `main.css` в `main.ts`, шрифты попадают в `dist/assets` с хешами, предупреждения Vite при `vite build` убраны; `docs/memory-bank/frontend/workflow.md` обновлён.
- [x] **2026-03-30** — Node **24.x**: эталон **24.14.1** из окружения — `CigarHelper.Web/.nvmrc`, `engines` во фронте и `e2e/`, CI `node-version-file`, разделы в `docs/memory-bank/workflow.md`, `frontend/workflow.md`, `e2e/README.md`.
- [x] **2026-03-30** — Vite 8 + **vite-plugin-vue-devtools**: `npm overrides` подняли транзитивный **vite-plugin-inspect** до **12.0.0-beta.1** (peer Vite 8), убраны ложные `invalid` в `npm ls`; комментарий в `vite.config.js`, раздел в `frontend/workflow.md`, обновлён `package-lock.json`.
- [x] **2026-03-30** — Frontend: откат Vite до **7.3.1** из‑за падения `vite-plugin-vue-devtools` на Vite 8; перевод флага `VITE_ENABLE_DEVTOOLS` на `loadEnv(mode, ...)` и добавлен `CigarHelper.Web/.env.development`.
- [x] **2026-03-30** — В бэклог добавлены идеи развития продукта (экспорт коллекции, дашборд, онбординг, поиск, история сигар, наблюдаемость; OpenAPI/каталог/медиа; P3 — соц-слой, оси отзывов, PWA, тарифы).
- [x] **2026-03-31** — UX: глобальный поиск — `SearchController` (`GET /api/search?q=&limit=`), DTO `GlobalSearchResultDto` + вложенные; composable `useGlobalSearch` (дебаунс 300 мс, клавиатурная навигация ↑↓ Enter Esc, шорткат Ctrl+K); компонент `GlobalSearch.vue` (модал PrimeVue Dialog, группированные результаты по типам); кнопка поиска в шапке `App.vue`.
- [x] **2026-03-31** — Frontend: реализовано переключение тёмной/светлой темы. `@custom-variant dark` в Tailwind v4 (класс-based по `.dark` на `<html>`); `darkModeSelector: '.dark'` в PrimeVue; семантические CSS-переменные (`--c-bg`, `--c-surface`, `--c-border`, `--c-text`, `--c-accent`) в `:root`/`.dark` + Tailwind-утилиты `bg-theme-*` / `text-theme-*`; composable `useTheme` (useDark из @vueuse/core, localStorage `cigar-color-scheme`); компонент `ThemeToggle.vue` добавлен в шапку приложения.
- [x] **2026-03-31** — Домен/UX: добавлен lifecycle сигары (`PurchasedAt`, `SmokedAt`, `LastTouchedAt`) и API-операция `POST /api/cigars/{id}/smoked`; `DashboardSummary` расширен блоками истории по месяцам (купил/выкурил), среднего срока до выкуривания и мягких напоминаний по «давно не трогал»; `Dashboard.vue` и `CigarList.vue` обновлены под новый сценарий, добавлены/обновлены unit-тесты `DashboardServiceTests`, `Dashboard.test.ts`, `dashboardService.test.ts`, создана миграция `AddUserCigarLifecycleDates`.
