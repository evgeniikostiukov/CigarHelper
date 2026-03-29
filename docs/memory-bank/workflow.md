# Рабочий процесс

## Секреты и конфиг

- В репозитории — плейсхолдеры; реальные строки подключения и `Jwt:Key` задаются через **User Secrets** и/или **environment variables** (`ConnectionStrings__DefaultConnection`, `Jwt__Key`, и т.д.).
- Проекты **Api**, **Data**, **Import** имеют свои `UserSecretsId` где это настроено.
- Локальные переопределения JSON: игнорировать коммитами файлы вида `**/appsettings.*.local.json` (см. историю security-рефакторинга).

## Запуск API

Из корня репозитория:

```bash
dotnet run --project CigarHelper.API/CigarHelper.Api.csproj
```

Убедиться, что задана строка подключения к PostgreSQL и параметры JWT для выдачи токенов.

### Локальная БД через Docker

```bash
docker compose up -d postgres
```

Пример строки подключения (пользователь/БД из `docker-compose.yml`):

`Host=localhost;Port=5432;Database=cigarhelper;Username=cigarhelper;Password=dev`

### Диагностика API

- **`GET /health`** — живость процесса.
- **`GET /health/ready`** — проверка EF к БД (тег `ready`).

### Security-чеклист (прод)

Перед выкатом пройти применимые пункты в [security-refactor-memory-bank.md](../security-refactor-memory-bank.md): **Cors:Origins**, секреты (`Jwt:Key`, строка БД), `appsettings.Production.json`, заголовки и доверие к прокси (см. тот же документ).

## Импорт CSV

```bash
dotnet run --project CigarHelper.Import/CigarHelper.Import.csproj -- путь\к\файлу.csv
```

Первый аргумент — путь к CSV, если файл существует. Иначе ищется `cigarday.csv` рядом с выходной директорией или на уровень выше; при отсутствии — интерактивный ввод пути (`CigarHelper.Import/Program.cs`).

## Миграции EF Core

Контекст: `AppDbContext` в проекте `CigarHelper.Data`. Типовой шаблон (из корня repo):

```bash
dotnet ef migrations add ИмяМиграции --project CigarHelper.Data/CigarHelper.Data.csproj --startup-project CigarHelper.API/CigarHelper.Api.csproj
dotnet ef database update --project CigarHelper.Data/CigarHelper.Data.csproj --startup-project CigarHelper.API/CigarHelper.Api.csproj
```

## Тесты

```bash
dotnet test CigarHelper.sln
```

Окружение **Testing** для API включает InMemory EF и отдельную инициализацию в `Program.cs` (без Npgsql и без применения миграций к реальной БД).

## E2E (Playwright)

Каталог **[`e2e/`](../../e2e/)** в корне репозитория — отдельный `package.json`, подробности в **[`e2e/README.md`](../../e2e/README.md)**.

**Порядок для локального прогона:**

1. PostgreSQL (локально или `docker compose up -d postgres` из корня — см. выше).
2. API: `dotnet run --project CigarHelper.API/CigarHelper.Api.csproj` — Kestrel должен совпадать с прокси Vite (**по умолчанию `http://localhost:5184`**).
3. Фронт: из `CigarHelper.Web` — `npm run dev` (**порт 3000**).
4. В `e2e/`: `npm ci`, `npx playwright install chromium`, затем `npm test`.

**Тестовый пользователь:** в репозитории нет зашитого E2E-аккаунта. Для будущих сценариев с логином: один раз зарегистрировать пользователя через UI/API, либо завести запись в БД и передавать учётные данные через **переменные окружения** (рекомендуемый префикс `E2E_`, например `E2E_EMAIL`, `E2E_PASSWORD`) — конкретные имена задаются в коде спеков по мере добавления тестов.

**Базовый URL фронта:** по умолчанию `http://localhost:3000`; переопределение: `PLAYWRIGHT_BASE_URL`.

Детали по фронту и прокси: [frontend/workflow.md](./frontend/workflow.md).

## Продакшен

Чеклист по хостам, CORS, секретам и Npgsql см. в [../security-refactor-memory-bank.md](../security-refactor-memory-bank.md) (раздел про выкат и `appsettings.Production.json`).
