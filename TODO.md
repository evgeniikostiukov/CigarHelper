# TODO — CigarHelper

**Назначение:** открытые задачи, приоритеты и журнал выполненного. **После значимых изменений в коде** обновляй этот файл: закрывай пункты (`[x]`), добавляй новые, переноси строку в **«Журнал выполненного»** с датой (формат `YYYY-MM-DD`).

**Связанные документы:** детали UI/темы — `[DESIGN.md](./DESIGN.md)`; архитектура и команды — `docs/memory-bank/`; безопасность — `docs/security-refactor-memory-bank.md`.

---

## Приоритеты


| Уровень | Смысл                                                      |
| ------- | ---------------------------------------------------------- |
| **P0**  | Безопасность, блокеры релиза, критичные дыры               |
| **P1**  | Качество продукта, стабильность, заметный UX/наблюдаемость |
| **P2**  | Техдолг, предупреждения сборки, расширение тестов          |
| **P3**  | По желанию, низкая срочность                               |


Внутри раздела задачи **сверху вниз** — от более срочных к менее.

---

## P0 — безопасность и критичное

- [ ] Разобрать **оставшиеся применимые пункты** из `docs/security-refactor-memory-bank.md`: в коде закрыты шаги 1–9; для выката вынесены в `[docs/memory-bank/security-deploy-checklist.md](./docs/memory-bank/security-deploy-checklist.md)`. *(Отсылка в `docs/memory-bank/workflow.md` сохраняется.)*
- [ ] **Первый прод-выкат:** пройти `[docs/memory-bank/security-deploy-checklist.md](./docs/memory-bank/security-deploy-checklist.md)` и зафиксировать реальные env/секреты (без коммита в git).
- [ ] **API:** лимиты размера тела запроса / загрузок там, где принимаются бинарники или крупный HTML (в духе security memory bank).

---

## P1 — продукт, UX, инфра разработки

- [ ] **SPA:** единый слой обработки ошибок API (toast + маппинг 401/403/5xx), меньше дублирования по view.
- [ ] **API:** health / readiness эндпоинты для хостинга и мониторинга (если ещё не покрыто текущей конфигурацией).
- [ ] **Docker Compose** для локалки: Postgres + API (опционально фронт) одной командой; кратко в memory bank / README.
- [x] **E2E (Playwright):** добавить в репозиторий (`e2e/` или внутри `CigarHelper.Web` — по решению команды).
- [x] **E2E:** описать в README/memory bank, как поднимать API + фронт (порты, тестовый пользователь, env).
- [x] **E2E:** smoke-набор — логин, 2–3 ключевых раздела, сценарий с диалогом/таблицей (например база сигар).
- [ ] **E2E:** подключить прогон в CI (Testing/staging по возможности). *(Пока пропущено по запросу.)*
- [ ] **Коллекция:** экспорт/импорт данных пользователя (CSV/JSON) для бэкапа и переноса.
- [x] **UX:** дашборд-сводка — объём коллекции, разрез по брендам, недавняя активность/отзывы.
  - [x] API: `DashboardController` + `IDashboardService.GetUserDashboardSummary(userId)` с агрегатами по коллекции и брендам, недавними отзывами.
  - [x] Frontend: маршрут `Dashboard` + блоки «Объём коллекции», «Бренды», «Недавние обзоры» в общем каркасе коллекции.
  - [x] E2E: smoke-journey покрывает открытие `/dashboard` и наличие ключевых блоков.
  - [x] Unit (Vitest): тесты для `Dashboard.vue` и `dashboardService`.
- [x] **UX:** онбординг после регистрации (первый хьюмидор, 1–2 сигары из каталога).
- [x] **UX:** глобальный поиск по бренду/сигаре/хьюмидору и клавиатурные шорткаты.
- [x] **Домен:** история сигары во времени (купил/выкурил), средние сроки, мягкие напоминания «давно не трогал».
- [x] **Наблюдаемость:** структурированные логи, correlation id, базовые метрики (запросы, ошибки, длительность) после стабилизации API.

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
- [x] `**index.html`:** выставить `lang="ru"` (или иной язык интерфейса), если контент не на английском.
- [x] В `docs/memory-bank/workflow.md` (и при желании `engines` в `CigarHelper.Web/package.json`) зафиксировать **целевую ветку Node**, совпадающую с CI/локальной средой (**24.x**, эталон **24.14.1** в `.nvmrc`).
- [x] **Контракт API:** OpenAPI как источник правды + генерация типов клиента для `CigarHelper.Web` (или иной регламент синхронизации с ростом контроллеров).
- [ ] **Каталог:** регламент периодического обновления данных (import/scraper), дедуп и при необходимости версионирование записей `CigarBase`.
- [x] **Медиа:** миниатюры, политика хранения изображений (локально vs объектное хранилище) при росте нагрузки и объёма.
- [x] **Медиа:** MinIO-провайдер (`MinioImageStorageProvider`) через официальный SDK `Minio 7.x`; docker-compose сервис `minio` (порты 9000/9001); бакет создаётся автоматически при старте API.

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

- **2026-04-04** — `CigarForm.vue`: блок фото на `FormImageGallerySection` (multi URL + файлы, до 12 кадров); сабмит собирает http(s) и data URL; пропсы `addUrlRowTestId` / `applyUrlsToGalleryTestId`; `collection-list-views.md`.
- **2026-04-04** — `FormImageGallerySection`: режим `urlEntryMode` `multi` — несколько полей URL и кнопка «Добавить в галерею» (как строки в `CigarForm`), до `maxUrlRows` (12), пакет с `resolveUrlImport` или локальные http(s). Включено в `ReviewForm` и `CigarBaseEditDialog`; убран дублирующий success-toast из `resolveCigarBaseUrlImport`.
- **2026-04-04** — Вынесена общая галерея загрузки изображений в `FormImageGallerySection.vue` (`FormGalleryImageItem`, v-model, подписи, лимит, URL/файлы, soft-delete; опции `section`/`bare`, `tone` review/dialog, `show-main-image-star`, `resolve-url-import`). Подключено в `ReviewForm.vue` и `CigarBaseEditDialog.vue`.
- **2026-04-04** — `ReviewForm.vue`: блок фото как у `CigarBaseEditDialog` — скрытый `ImageUploader`, кнопка выбора файлов, добавление по HTTP(S)-ссылке, сетка превью, удаление с пометкой «будет удалено» и откат, подписи к кадрам; при сохранении файлы уходят как data URL; загрузка черновика обзора — превью из `imageBytes` (API). `ImageDownloader`: поддержка `data:image/...;base64,...` для тех же пайплайнов скачивания.
- **2026-04-04** — Дашборд: средняя оценка коллекции по полю `Rating` у `UserCigar` (`GET /api/dashboard/summary` → `averageCigarRating`, только сигары с заполненной оценкой); карточка на `Dashboard.vue` (`dashboard-summary-average-rating`); обновлены `openapi.json`, `api.generated.ts`, тесты `DashboardServiceTests`, `Dashboard.test.ts`, `dashboardService.test.ts`, `api.types.test.ts`; `collection-list-views.md`.
- **2026-04-04** — `useGlobalSearch.ts`: в `onKeydown` для шортката Ctrl+K используется `e.key?.toLowerCase()` — у некоторых событий `key` бывает `undefined`, из‑за чего падал `toLowerCase`.
- **2026-04-04** — Добавление в коллекцию только из модерированного `CigarBase`: `POST /api/cigars` принимает `cigarBaseId` (+ цена, хьюмидор, вкус/аромат, URL фото); убрано создание немодерированных записей базы из этой формы. В `UserCigars` — колонки `Taste`/`Aroma` (миграция `AddUserCigarTasteAndAroma`). `PUT /api/cigars/{id}` меняет только личные поля и галерею UserCigar, не трогает `CigarBase`. Фронт: `CigarForm.vue` — только выбор из справочника, чекбокс «Добавить в хьюмидор»; редактирование — `CigarCollectionEdit.vue` (`/cigars/:id/edit`); `cigarService.createCigar`/`updateCigar` с новыми контрактами; онбординг и `openapi`/`api.generated` обновлены.
- **2026-04-04** — Диалог базовой сигары (`CigarBaseEditDialog.vue`): превью в сетке — `object-contain` + центрирование вместо `object-cover`, контейнер с `overflow-hidden` и нейтральным фоном, чтобы картинка целиком помещалась в квадратную ячейку.
- **2026-04-03** — Импорт: при дубликате `CigarBase` строка пропускается только если в БД уже есть главное фото (`CigarImages` + `IsMain`); иначе к существующей сигаре дозагружается картинка (хранилище/HTTP). Периодический `SaveChanges` по `progressTick`, не только по `importedCount`.
- **2026-04-03** — Импорт: `TryGetExistingBySourceImageUrlAsync` выполняется после резолва бренда и **до** запроса дубликата сигары в БД; результат затем используется при добавлении `CigarImage` (один вызов MinIO/файлов на строку CSV).
- **2026-04-03** — Импорт (`CigarHelper.Import`): детерминированные ключи в MinIO/LocalFile по SHA256 URL картинки (`import/{hash}{ext}`, `import/{hash}_thumb.webp`); перед сохранением `StatObject`/проверка файлов — если оригинал и миниатюра уже есть, HTTP-скачивание и Put пропускаются, в БД пишутся существующие пути; иначе скачивание и запись по тем же ключам.
- **2026-04-03** — Форма сигары: несколько URL фото (`CreateCigarRequest.ImageUrls`, `UserCigarUpdateRequest.ImageUrlsToAdd` / `ImageIdsToRemove`), UI в `CigarForm.vue` (строки ссылок, сохранённые миниатюры, удаление); `cigarService.createCigar` / `updateCigar` с массивами.
- **2026-03-30** — UX: анализ и проектирование дашборда-сводки (объём коллекции, разрез по брендам, недавняя активность/отзывы): выбран отдельный `DashboardController` и `IDashboardService.GetUserDashboardSummary(userId)`, определены ключевые блоки UI и маршрут `Dashboard`.
- **2026-03-30** — API: реализован `DashboardService.GetUserDashboardSummaryAsync(userId)` с агрегатами по хьюмидорам, пользовательским сигарам, брендам и недавним обзорам; добавлены `DashboardController` (`GET /api/dashboard/summary`), DTO `DashboardSummaryDto`/`BrandBreakdownItemDto`/`RecentReviewDto`, регистрация сервиса в `Program.cs` и unit-тесты `DashboardServiceTests` (InMemory EF).
- **2026-03-30** — Frontend: добавлен сервис `dashboardService` (`GET /api/dashboard/summary`), маршрут `Dashboard` (`/dashboard`, requiresAuth) с новым view `Dashboard.vue` в общем каркасе коллекции (панель «Сводка коллекции», блоки объёма, брендов и недавних обзоров, CTA при пустой коллекции); в `App.vue` добавлен пункт меню «Сводка».
- **2026-03-30** — UX: добавлен онбординг после регистрации: новый маршрут `/onboarding` (requiresAuth), флаг `needsOnboarding` в `localStorage`, редирект после регистрации на онбординг, мастер «создать хьюмидор → добавить 1–2 сигары из базы в коллекцию»; unit-тесты `src/views/Onboarding.test.ts`.
- **2026-03-30** — Repo hygiene: подключены git hooks на прекоммит: `husky` + `lint-staged` (eslint/prettier по staged-файлам фронта), `commitlint` (проверка commit message на conventional commits).
- **2026-03-30** — Repo hygiene: добавлены хуки для .NET: `dotnet format` (только staged `*.cs` через `--include`) на pre-commit и `dotnet test CigarHelper.sln -c Release` на pre-push.
- **2026-03-30** — E2E: обновлён `e2e/tests/smoke-journey.spec.ts` — после логина открывается «Сводка» (`/dashboard`), проверяются `data-testid="dashboard"` и `data-testid="dashboard-content"`; `npm test` (Playwright) — ok.
- **2026-03-30** — Unit (Vitest): добавлены `src/views/Dashboard.test.ts` (рендер контента/ошибка/навигация) и `src/services/dashboardService.test.ts` (контракт вызова `/dashboard/summary`); `npm test` в `CigarHelper.Web` — ok.
- **2026-03-30** — Docs (memory bank): обновлены `docs/memory-bank/frontend/code-map.md` и `docs/memory-bank/frontend/collection-list-views.md` — добавлены маршрут/экран `Dashboard` и `dashboardService`.
- **2026-03-30** — Repo hygiene: `.env.development` добавлен в `.gitignore` (локальный флаг `VITE_ENABLE_DEVTOOLS` не коммитим).
- **2026-03-30** — Зависимости: `npx npm-check-updates -u` в `CigarHelper.Web`, корне репо и `cigar-scraper` + `npm install`; TypeScript **^5.9.3** (peer `typescript-eslint@8`); TipTap 3 в `TextEditor.vue`; удалён `@types/dompurify`.
- **2026-03-30** — Фронт: палитра светлее, акцент rose — пресет PrimeVue Aura (`rose` primary, `surface` stone), Tailwind amber→rose, фоны stone, `App.vue` / `main.css`.
- **2026-03-30** — Этап 1 бэкенд: интеграционные тесты 401/403/404, публичный профиль, пагинация; InMemory одно имя БД на хост.
- **2026-03-30** — CI GitHub Actions: `dotnet test` + фронт (`typecheck`, `lint`, `test`, `build`); `eslint-config-prettier`; удалён `.eslintignore`.
- **2026-03-30** — Правки под `vue-tsc`: `User.role`, `getRoleClaims`, `getAuthUserId`; ESLint/Prettier; мелкие правки линта.
- **2026-03-30** — Vitest + `happy-dom`: `vitest.config.ts`, `imageUtils.test.ts`, `roles.test.ts`.
- **2026-03-30** — Добавлены корневые `**TODO.md`** (приоритетный бэклог + журнал) и `**DESIGN.md**` (UI/тема); правило в `.cursor/rules/memory-bank.mdc` ссылается на `TODO.md`.
- **2026-03-30** — P0 security: по `security-refactor-memory-bank.md` шаги 1–9 уже в коде; добавлен операционный чеклист `docs/memory-bank/security-deploy-checklist.md`, пункт бэклога закрыт, открыт явный follow-up на первый прод-выкат.
- **2026-03-30** — E2E: каталог `e2e/` с `@playwright/test`, `playwright.config.ts`, smoke `tests/smoke.spec.ts` (главная + `data-testid`), `README.md` с портами и порядком запуска API/фронта.
- **2026-03-30** — E2E: разделы в `docs/memory-bank/workflow.md` и `docs/memory-bank/frontend/workflow.md` (порты 3000/5184, Docker Postgres, порядок запуска, тестовый пользователь через env `E2E_`*, `PLAYWRIGHT_BASE_URL`); оглавления memory bank обновлены.
- **2026-03-30** — E2E: `e2e/tests/smoke-journey.spec.ts` — регистрация через UI или вход по `E2E_EMAIL`/`E2E_PASSWORD`, хьюмидоры/форма, сигары, обзоры, `/cigar-bases`; описание в `e2e/README.md`.
- **2026-03-30** — Фронт: `@vue/test-utils` + `@vitejs/plugin-vue` в `vitest.config.ts`, smoke `src/vue-test-utils.smoke.test.ts`; закрыт пункт `index.html` `lang="ru"` (уже было); E2E CI в бэклоге помечен как временно пропущенный; `docs/memory-bank/frontend/workflow.md` обновлён.
- **2026-03-30** — NuGet NU1510: удалены прямые ссылки `Microsoft.Extensions.Configuration.Json` из `CigarHelper.Data` и `CigarHelper.Import` (остаётся транзитивно); в API прямой ссылки не было. `dotnet build` / `dotnet test` по solution — ок.
- **2026-03-30** — Tailwind v4: в `CigarBaseEditDialog.vue` заменены `bg-opacity-`* на синтаксис `/opacity`; в `docs/memory-bank/frontend/workflow.md` — заметка по v4 и `@reference` для будущих `@apply` в SFC.
- **2026-03-30** — Lightning CSS / шел: стили из `App.vue` перенесены в `src/assets/app-shell.css` + импорт из `main.css`; `App.vue` без `<style>`. Предупреждений по сборке для шела нет; workflow обновлён.
- **2026-03-30** — PrimeIcons: импорт `primeicons.css` перенесён из `main.css` в `main.ts`, шрифты попадают в `dist/assets` с хешами, предупреждения Vite при `vite build` убраны; `docs/memory-bank/frontend/workflow.md` обновлён.
- **2026-03-30** — Node **24.x**: эталон **24.14.1** из окружения — `CigarHelper.Web/.nvmrc`, `engines` во фронте и `e2e/`, CI `node-version-file`, разделы в `docs/memory-bank/workflow.md`, `frontend/workflow.md`, `e2e/README.md`.
- **2026-03-30** — Vite 8 + **vite-plugin-vue-devtools**: `npm overrides` подняли транзитивный **vite-plugin-inspect** до **12.0.0-beta.1** (peer Vite 8), убраны ложные `invalid` в `npm ls`; комментарий в `vite.config.js`, раздел в `frontend/workflow.md`, обновлён `package-lock.json`.
- **2026-03-30** — Frontend: откат Vite до **7.3.1** из‑за падения `vite-plugin-vue-devtools` на Vite 8; перевод флага `VITE_ENABLE_DEVTOOLS` на `loadEnv(mode, ...)` и добавлен `CigarHelper.Web/.env.development`.
- **2026-03-30** — В бэклог добавлены идеи развития продукта (экспорт коллекции, дашборд, онбординг, поиск, история сигар, наблюдаемость; OpenAPI/каталог/медиа; P3 — соц-слой, оси отзывов, PWA, тарифы).
- **2026-03-31** — UX: глобальный поиск — `SearchController` (`GET /api/search?q=&limit=`), DTO `GlobalSearchResultDto` + вложенные; composable `useGlobalSearch` (дебаунс 300 мс, клавиатурная навигация ↑↓ Enter Esc, шорткат Ctrl+K); компонент `GlobalSearch.vue` (модал PrimeVue Dialog, группированные результаты по типам); кнопка поиска в шапке `App.vue`.
- **2026-03-31** — Frontend: реализовано переключение тёмной/светлой темы. `@custom-variant dark` в Tailwind v4 (класс-based по `.dark` на `<html>`); `darkModeSelector: '.dark'` в PrimeVue; семантические CSS-переменные (`--c-bg`, `--c-surface`, `--c-border`, `--c-text`, `--c-accent`) в `:root`/`.dark` + Tailwind-утилиты `bg-theme-`* / `text-theme-*`; composable `useTheme` (useDark из @vueuse/core, localStorage `cigar-color-scheme`); компонент `ThemeToggle.vue` добавлен в шапку приложения.
- **2026-03-31** — Домен/UX: добавлен lifecycle сигары (`PurchasedAt`, `SmokedAt`, `LastTouchedAt`) и API-операция `POST /api/cigars/{id}/smoked`; `DashboardSummary` расширен блоками истории по месяцам (купил/выкурил), среднего срока до выкуривания и мягких напоминаний по «давно не трогал»; `Dashboard.vue` и `CigarList.vue` обновлены под новый сценарий, добавлены/обновлены unit-тесты `DashboardServiceTests`, `Dashboard.test.ts`, `dashboardService.test.ts`, создана миграция `AddUserCigarLifecycleDates`.
- **2026-03-31** — Глобальная обработка ошибок (TDD): доменные исключения (`NotFoundException`→404, `ForbiddenException`→403, `ConflictException`→409, `ArgumentException`→422, `UnauthorizedAccessException`→403); `GlobalExceptionHandlerMiddleware` — RFC 7807 Problem Details (`application/problem+json`), correlation ID в теле, стек-трейс скрыт в Production; `ReviewService.ArgumentException` → `NotFoundException`; `AddProblemDetails()` в DI. 130/130 тестов.
- **2026-03-31** — Наблюдаемость (TDD): `CorrelationIdMiddleware` (X-Correlation-ID в response + HttpContext.Items, 3 теста), `RequestMetricsMiddleware` + `IMetricsCollector`/`InMemoryMetricsCollector` (счётчики запросов, ошибок 5xx, суммарная длительность, 5 тестов); Serilog.AspNetCore (JSON-логи через `UseSerilogRequestLogging` с enrichment correlation id), endpoint `GET /metrics` (JSON-снимок). 121/121 тестов зелёные.
- **2026-04-01** — MinIO-провайдер: `MinioImageStorageProvider : IImageStorageProvider` (Minio SDK 7.x); `MinioOptions` в `ImageStorageOptions`; `docker-compose.yml` расширен сервисом `minio` (9000/9001); бакет создаётся при старте; `appsettings.json` + инструкции в `workflow.md`.
- **2026-04-01** — Фикс плейсхолдера при ошибке загрузки в `CigarBases.vue`: реактивный `failedImageIds` (`ref<Set<number>>`); `handleImageError` добавляет ID в Set вместо DOM-скрытия; `cigarBaseThumbnailSrc` возвращает `''` для упавших ID → `v-else`-плейсхолдер корректно показывается; `memoKey` включает флаг ошибки для инвалидации `v-memo`; `Cache-Control: public, max-age=86400` для CigarBase-миниатюр.
- **2026-04-01** — Фикс отображения MinIO-изображений в `CigarBases.vue`: `[AllowAnonymous]` на `GET /api/cigarimages/{id}/thumbnail` и `/data` для публичных CigarBase-картинок (UserCigar по-прежнему проверяет owner/staff); `primaryCigarBaseImage` теперь возвращает изображение по `id` без инлайн-байт; `cigarBaseThumbnailSrc` использует API-URL как fallback при MinIO; интерфейс `CigarImage` расширен полями `hasThumbnail`, `fileName`, `fileSize`.
- **2026-04-01** — Медиа / хранилище изображений: `IImageStorageProvider` с реализациями `DatabaseImageStorage` (default, bytea в Postgres) и `LocalFileImageStorage` (диск); `IThumbnailGenerator` + `ImageSharpThumbnailGenerator` (WebP 320×320); `IImageService`/`ImageService` — оркестрирует validate → store → thumbnail; модель `CigarImage` расширена полями `StoragePath`, `ThumbnailData`, `ThumbnailPath`; EF-миграция `AddImageStorageAndThumbnailFields`; новые эндпоинты `GET /api/cigar-images/{id}/data` и `GET /api/cigar-images/{id}/thumbnail`; конфиг `ImageStorage` в `appsettings.json`; 14 новых тестов `ThumbnailGeneratorTests` + `LocalFileImageStorageTests`; всего 144 теста зелёных. `docs/memory-bank/code-map.md` обновлён политикой хранения.
- **2026-04-03** — Убрано хранение bytea у `CigarImage` (`ImageData`/`ThumbnailData`): миграция `RemoveCigarImageByteaColumns`; `ImageService` и провайдеры только внешнее хранилище; удалён `DatabaseImageStorage`, default `ImageStorage:Provider` = Minio; интеграционные тесты — `LocalFile` в temp через `AuthIntegrationWebAppFactory`; Import — `ImportImagePersistence` (MinIO/LocalFile + WebP-thumb), пакеты Minio/ImageSharp; DTO/OpenAPI без инлайн `data` у `CigarImageDto`; фронт `CigarImage` без legacy полей. **Внимание:** применение миграции на проде с существующими bytea удалит бинарники, пока их не перелили в объектное хранилище.
- **2026-03-31** — OpenAPI как источник правды (TDD): `Swashbuckle.AspNetCore.Cli` (dotnet local tool `dotnet-tools.json`) генерирует `openapi.json` из сборки API; `openapi-typescript` генерирует `src/types/api.generated.ts`; фасад `src/types/index.ts` с дружелюбными алиасами (`ApiCigarResponse`, `ApiDashboardSummary`, …); TDD-тест контракта `src/types/api.types.test.ts` (type-level проверки через `expectTypeOf`); `dashboardService.ts` и `searchService.ts` мигрированы на сгенерированные типы (сервис нормализует optional API → definite компонентные типы); pre-existing ошибки `Onboarding.vue`/`Onboarding.test.ts` исправлены; `useGlobalSearch.ts` исправлен под nullable поля; CI green 35/35.

- **2026-04-03** — `CigarForm.vue`: выбор главного фото среди сохранённых изображений коллекции (кнопка-звезда, `cigarService.setCigarImageMain` → `PATCH .../cigarimages/{id}/set-main`); обновлён `docs/memory-bank/frontend/collection-list-views.md`.

- **2026-04-03** — Фикс фильтра базы сигар: `CigarBases.vue` и `ReviewForm.vue` передают в `GET .../cigars/bases/paginated` параметр `search` (как в `GetCigarBasesPaginated`), а не `name`.
- **2026-04-03** — Плейсхолдер `ConnectionStrings:DefaultConnection` в `appsettings*.json` (API, Data, Import): `cigarhelper`/`cigarhelper` + `Port=5432`, в соответствии с `docker-compose` и `.env.example` (раньше было `cigar_helper`/`postgres`, из‑за чего при Docker локалке импорт и EF падали на несуществующей БД/пользователе).
- **2026-04-03** — `workflow.md`: раздел «Локальный PostgreSQL (без Docker)» — строка через secrets/env под реальную роль/БД; пояснение `28P01` и что Import/API нужна та же строка.
- **2026-04-03** — Import: `UseEnvironment` с fallback `Development`, если не заданы `DOTNET_ENVIRONMENT`/`ASPNETCORE_ENVIRONMENT` — User Secrets снова подхватываются (раньше консоль по умолчанию шла в Production); `Properties/launchSettings.json`; примечание в `workflow.md`.
- **2026-04-03** — Import: `ImportImagePersistence` на `IImageStorageProvider` из `CigarHelper.Api` (регистрация `MinioImageStorageProvider`/`LocalFileImageStorage` в `Import/Program.cs`); расширен контракт провайдера: `ExistsAsync`, `PutAtKeyAsync`, `TryDescribeAsync`, `ImageStorageObjectInfo`; убран прямой MinIO SDK из Import; тесты `LocalFileImageStorageTests` на вложенные ключи; `code-map.md` обновлён.
- **2026-04-03** — Импорт картинок без отдельных ключей `import/…`: общий `CigarImageStorageWriter` с `ImageService`; `IImageStorageProvider` снова только `Save`/`Read`/`Delete`; CSV пишет в `CigarImage.StoragePath`/`ThumbnailPath` как UI; удалён `ImportImagePersistence`, пакет ImageSharp из Import; комментарии в `CigarImage`.
- **2026-04-03** — Список/деталь коллекции: `GET /api/cigars`, `GetCigar`, создание/обновление/«выкурена» возвращают объединённую галерею (фото UserCigar + фото CigarBase); `CigarList`/`CigarDetail` — порядок слайдов в `orderUserCigarGalleryImages`; обновлены `CigarResponseDto`, `collection-list-views.md`, `code-map.md`.
- **2026-04-04** — `POST /api/cigars/bases` (`CreateCigarBase`): `CigarBase.IsModerated` = true только для JWT с ролью `Admin` или `Moderator` (`User.IsInRole(nameof(Role.*))`); обычные пользователи создают запись с `IsModerated = false` (как в `CigarImagesController.IsStaff`).
- **2026-04-04** — Оценка коллекции (1–10): `Rating` в `CreateCigarRequest` и `UserCigarUpdateRequest`, запись в `CreateCigar`/`UpdateCigar`; `cigarService` шлёт `rating` в POST/PUT; `CigarForm.vue` и `CigarCollectionEdit.vue` — PrimeVue `Rating` (:stars=10), сброс повторным кликом по звезде.
- **2026-04-04** — PrimeVue v4: заменён устаревший `Dropdown` на `Select` в `CigarForm`, `CigarCollectionEdit`, `CigarBases`, `Brands`, `AdminUsers`, `CigarBaseEditDialog`; `label-id` / `label-class` вместо `input-id` / `input-class`; пересобран `components.d.ts` (unplugin-vue-components).
- **2026-04-04** — `npm run typecheck` (vue-tsc): `CigarImage` — опциональные `imageData`/`data` + нормализация `null`→`undefined` в хелперах превью; удалён несуществующий в OpenAPI алиас `ApiCreateCigarBaseRequest`; `Rating` — `:model-value` / `@update:model-value` из‑за `number | undefined` у PrimeVue при `number | null` в формах.
- **2026-04-04** — Vitest `Onboarding.test.ts`: заглушки `IconField` и `InputIcon` в `PrimeStubs` (убрано Vue warn «Failed to resolve component: InputIcon» без auto-import в тестовой среде).
- **2026-04-04** — CI: `global.json` — SDK `10.0.100` + `rollForward: latestFeature` вместо несуществующего `10.0.0` (fix `setup-dotnet` / dotnet-install на GitHub Actions).
- **2026-04-04** — Фронт CI: в `package.json` явно добавлены `zod`, `@tiptap/extension-placeholder`, `@tiptap/extension-character-count` (импорты в `CigarBaseEditDialog` / `TextEditor`; без них `npm ci` на Actions не подтягивал модули).
- **2026-04-04** — CI dotnet-test: `CigarHelper.Api.Tests.csproj` — `ProjectReference` на `..\CigarHelper.Api\` вместо `..\CigarHelper.API\` (регистр как в git; на Linux иначе ссылка не находилась → CS0234). Документы: пути `CigarHelper.Api/` в memory bank, e2e, docker-compose.

