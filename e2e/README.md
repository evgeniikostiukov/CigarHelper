# E2E (Playwright)

## Предусловия

1. **PostgreSQL** (или `docker compose up -d postgres` из корня репозитория).
2. **API:** из корня репозитория  
   `dotnet run --project CigarHelper.API/CigarHelper.Api.csproj`  
   Убедиться, что Kestrel слушает порт **5184** (или измените `CigarHelper.Web/vite.config.js` → `server.proxy['/api'].target`).
3. **Фронт:** из `CigarHelper.Web`  
   `npm run dev` → **http://localhost:3000**

## Установка

```bash
cd e2e
npm ci
npx playwright install chromium
```

## Сценарии

- **`tests/smoke.spec.ts`** — главная без логина (`data-testid="app"`).
- **`tests/smoke-journey.spec.ts`** — регистрация через UI (уникальный email/username) **или** вход по переменным **`E2E_EMAIL`** и **`E2E_PASSWORD`** (обе заданы — регистрация не выполняется). Далее: хьюмидоры → форма создания → назад, «Мои сигары», «Обзоры», прямой переход на `/cigar-bases` (таблица / пусто / ошибка API).

## Запуск

```bash
npm test
```

Только journey-smoke:

```bash
npx playwright test tests/smoke-journey.spec.ts
```

Отчёт: `npx playwright show-report`

Переопределить URL фронта:

```bash
set PLAYWRIGHT_BASE_URL=http://127.0.0.1:3000
npm test
```
