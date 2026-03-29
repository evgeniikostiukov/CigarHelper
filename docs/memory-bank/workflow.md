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

## Продакшен

Чеклист по хостам, CORS, секретам и Npgsql см. в [../security-refactor-memory-bank.md](../security-refactor-memory-bank.md) (раздел про выкат и `appsettings.Production.json`).
