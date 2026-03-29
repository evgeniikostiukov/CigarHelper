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

---

## P2 — техдолг, тесты, сборка

- [ ] При изменении контрактов API — обновлять или добавлять тесты в `CigarHelper.Api.Tests` до или вместе с правкой фронта.
- [ ] По мере выноса логики из крупных `.vue` — точечные тесты на чистые функции/composables (Vitest).
- [x] Подключить **Vue Test Utils** совместно с Vitest (если ещё не на этапе unit-тестов).
- [ ] Компонентные тесты для 1–2 переиспользуемых или сложных компонентов (диалоги, нестандартная логика).
- [ ] Периодически обновлять зависимости (`npm audit`, Browserslist) и устранять критичные уязвимости без ломки сборки.
- [x] Разобраться с предупреждением Tailwind при `vite build` (`unknown utility class`, при необходимости `@reference` / конфиг v4).
- [x] **NuGet `NU1510` / `Microsoft.Extensions.Configuration.Json`:** убраны избыточные прямые ссылки в `CigarHelper.Data` и `CigarHelper.Import` (пакет транзитивен; в `CigarHelper.Api` отдельной ссылки не было).
- [ ] По мере роста бандла — точечный code-splitting тяжёлых экранов (например формы с редактором).
- [ ] Актуализировать `docs/memory-bank/` после крупных изменений API, маршрутов или обязательных секретов.
- [x] Убрать или сузить предупреждения Lightning CSS при `vite build` про `:deep` / `:global` в `App.vue`.
- [ ] Согласовать **Vite 8** и **vite-plugin-vue-devtools** (обновить плагин или зафиксировать версии и описать в workflow).
- [x] Разобраться с предупреждениями сборки про шрифты **PrimeIcons** (пути в Vite/CSS или задокументировать как ожидаемое).
- [ ] Когда **typescript-eslint** поддерживает TypeScript 6 — поднять TS и снять потолок **5.9.x** во фронте.
- [ ] Рассмотреть **Dependabot / Renovate** (или регламент) для автоматических PR по зависимостям с лимитами на major.
- [x] **`index.html`:** выставить `lang="ru"` (или иной язык интерфейса), если контент не на английском.
- [x] В `docs/memory-bank/workflow.md` (и при желании `engines` в `CigarHelper.Web/package.json`) зафиксировать **целевую ветку Node**, совпадающую с CI/локальной средой (**24.x**, эталон **24.14.1** в `.nvmrc`).

---

## P3 — по необходимости

- [ ] Визуальная регрессия (скриншоты) — при жёсткой дизайн-системе или частых визуальных регрессиях.
- [ ] Контрактные тесты фронт↔бэк (Pact и т.п.) — если интеграционных тестов API станет недостаточно.
- [ ] Сценарии доступности (axe в Playwright) для публичных страниц.

---

## Завершённые этапы (кратко)

- **Этап 0 (CI):** фронт `build` / `lint` / typecheck; `dotnet test` на чистой машине.
- **Этап 1 (бэкенд-тесты):** критичные контроллеры, `ApiAuthorizationAndContractsIntegrationTests`, InMemory в `Program.cs` для сидов.
- **Этап 2 (Vitest):** конфиг, тесты `imageUtils`, `roles`.

---

## Журнал выполненного

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
