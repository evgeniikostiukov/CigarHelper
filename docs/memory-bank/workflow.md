# Рабочий процесс

## Секреты и конфиг

- В репозитории — плейсхолдеры; реальные строки подключения и `Jwt:Key` задаются через **User Secrets** и/или **environment variables** (`ConnectionStrings__DefaultConnection`, `Jwt__Key`, и т.д.).
- Проекты **Api**, **Data**, **Import** имеют свои `UserSecretsId` где это настроено.
- Локальные переопределения JSON: игнорировать коммитами файлы вида `**/appsettings.*.local.json` (см. историю security-рефакторинга).

## Node.js (фронт, e2e, CI)

- Целевая ветка **24.x**; эталонная версия среды: **24.14.1** (файл **`CigarHelper.Web/.nvmrc`**, поле **`engines.node`** в `CigarHelper.Web/package.json`, GitHub Actions **`setup-node`** с `node-version-file` на этот путь).
- Для локалки: `nvm use` / `fnm use` в каталоге фронта или указать ту же версию вручную.

## Запуск API

Из корня репозитория:

```bash
dotnet run --project CigarHelper.Api/CigarHelper.Api.csproj
```

Убедиться, что задана строка подключения к PostgreSQL и параметры JWT для выдачи токенов.

### Полный стек в Docker (Postgres + MinIO + API + SPA)

Пошаговая инструкция: **[`docs/docker.md`](../docker.md)** (`docker compose --profile full`). Переменные `JWT_KEY`, `WEB_PORT`, `API_HOST_PORT` — в `.env` (шаблон `.env.example`).

### Локальная БД через Docker

```bash
docker compose up -d postgres
```

Имена БД и пользователя задаются в `.env` (шаблон — `.env.example`: `POSTGRES_DB` / `POSTGRES_USER` = `cigarhelper`). Плейсхолдеры в `appsettings.json` (API, Data, Import) приведены к тем же значениям; пароль должен совпадать с `POSTGRES_PASSWORD` (через User Secrets или `ConnectionStrings__DefaultConnection`).

Пример строки подключения:

`Host=localhost;Port=5432;Database=cigarhelper;Username=cigarhelper;Password=<как в .env>`

### Локальный PostgreSQL (без Docker)

Плейсхолдеры в `appsettings.json` ориентированы на сценарий с **docker compose** (`cigarhelper` / БД `cigarhelper`). Если Postgres установлен локально (инсталлятор, пакетный менеджер и т.д.), задайте **свою** строку через User Secrets или `ConnectionStrings__DefaultConnection`: `Host`, `Port`, `Database`, `Username`, `Password` должны совпадать с тем, что вы создали в кластере (часто это роль `postgres` и отдельная БД под проект).

Для **Import** и **API** удобно дублировать одну и ту же строку в secrets обоих проектов (или одна переменная окружения перед запуском).

Ошибка **`28P01`** от Npgsql означает неверный пароль или несовпадение пользователя с тем, что настроено на сервере — это не проблема кода, а конфигурации строки подключения.

### Локальный MinIO через Docker

```bash
docker compose up -d minio
```

- S3 API: `http://localhost:9000`
- Веб-консоль: `http://localhost:9001` (логин/пароль: `minioadmin` / `minioadmin`)

По умолчанию в репозитории `ImageStorage:Provider` = `Minio`. Секреты (`AccessKey`/`SecretKey`) и при необходимости endpoint — через User Secrets или env. Для ручной подстройки пример:

```json
{
  "ImageStorage": {
    "Provider": "Minio",
    "Minio": {
      "AccessKey": "minioadmin",
      "SecretKey": "minioadmin"
    }
  }
}
```

Команда для User Secrets:

```bash
dotnet user-secrets set "ImageStorage:Provider" "Minio" --project CigarHelper.Api/CigarHelper.Api.csproj
dotnet user-secrets set "ImageStorage:Minio:AccessKey" "minioadmin" --project CigarHelper.Api/CigarHelper.Api.csproj
dotnet user-secrets set "ImageStorage:Minio:SecretKey" "minioadmin" --project CigarHelper.Api/CigarHelper.Api.csproj
```

Бакет `cigar-images` создаётся автоматически при старте API (если не существует).

### Диагностика API

- **`GET /health`** — живость процесса.
- **`GET /health/ready`** — проверка EF к БД (тег `ready`).

### Security-чеклист (прод)

Перед выкатом пройти применимые пункты в [security-refactor-memory-bank.md](../security-refactor-memory-bank.md): **Cors:Origins**, секреты (`Jwt:Key`, строка БД), `appsettings.Production.json`, заголовки и доверие к прокси (см. тот же документ).

Консольный **Import** использует ту же секцию `ImageStorage` (`Import/appsettings.json` + user-secrets): изображения из CSV пишутся в MinIO или LocalFile, в БД — только ключи.

## Импорт CSV

```bash
dotnet run --project CigarHelper.Import/CigarHelper.Import.csproj -- путь\к\файлу.csv
```

Первый аргумент — путь к CSV, если файл существует. Иначе ищется `cigarday.csv` рядом с выходной директорией или на уровень выше; при отсутствии — интерактивный ввод пути (`CigarHelper.Import/Program.cs`).

**User Secrets для строки БД:** если не заданы `DOTNET_ENVIRONMENT` / `ASPNETCORE_ENVIRONMENT`, хост по умолчанию считал бы окружение Production и **не подгружал бы** секреты. В `Program.cs` для Import явно выставляется `Development`, когда переменные не заданы; при необходимости прод-подобного запуска задайте `DOTNET_ENVIRONMENT=Production` и передайте строку через `ConnectionStrings__DefaultConnection`.

## Миграции EF Core

Контекст: `AppDbContext` в проекте `CigarHelper.Data`. Типовой шаблон (из корня repo):

```bash
dotnet ef migrations add ИмяМиграции --project CigarHelper.Data/CigarHelper.Data.csproj --startup-project CigarHelper.Api/CigarHelper.Api.csproj
dotnet ef database update --project CigarHelper.Data/CigarHelper.Data.csproj --startup-project CigarHelper.Api/CigarHelper.Api.csproj
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
2. API: `dotnet run --project CigarHelper.Api/CigarHelper.Api.csproj` — Kestrel должен совпадать с прокси Vite (**по умолчанию `http://localhost:5184`**).
3. Фронт: из `CigarHelper.Web` — `npm run dev` (**порт 3000**).
4. В `e2e/`: `npm ci`, `npx playwright install chromium`, затем `npm test`.

**Тестовый пользователь:** в репозитории нет зашитого E2E-аккаунта. Для сценариев с логином: зарегистрировать пользователя через UI/API или завести запись в БД и передавать **логин и пароль** через env с префиксом `E2E_` (в `smoke-journey`: `E2E_USERNAME`, `E2E_PASSWORD`).

**Базовый URL фронта:** по умолчанию `http://localhost:3000`; переопределение: `PLAYWRIGHT_BASE_URL`.

Детали по фронту и прокси: [frontend/workflow.md](./frontend/workflow.md).

## Продакшен

Чеклист по хостам, CORS, секретам и Npgsql см. в [../security-refactor-memory-bank.md](../security-refactor-memory-bank.md) (раздел про выкат и `appsettings.Production.json`).
