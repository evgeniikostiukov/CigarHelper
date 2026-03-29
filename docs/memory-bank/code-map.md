# Карта кода

## Точки входа

- API: `CigarHelper.API/Program.cs` (+ `ProgramPartial.cs` для интеграционных тестов).
- Import: `CigarHelper.Import/Program.cs` → `ImportCigarsFromCsv`.

## API — контроллеры (`CigarHelper.API/Controllers/`)

| Контроллер | Назначение |
|------------|------------|
| `AuthController` | Регистрация, логин, JWT |
| `ProfileController` | Профиль текущего пользователя |
| `AdminUsersController` | Админ-операции с пользователями |
| `PublicUsersController` | Публичные данные пользователей |
| `BrandsController` | Бренды |
| `CigarsController` | Каталог / сигары |
| `HumidorsController` | Хьюмидоры пользователя |
| `ReviewsController` | Отзывы |
| `CigarImagesController` | Изображения сигар (авторизация, владение, роли) |

## API — сервисы и прочее

- `CigarHelper.API/Services/` — `AuthService`, `JwtService`, `ProfileService`, `AdminUserService`, `HumidorService`, `ReviewService` и др.
- `CigarHelper.API/Helpers/` — например `ImageBinaryValidator` (проверка бинарных изображений).
- `CigarHelper.API/Options/` — сильно типизированные опции конфигурации.
- `CigarHelper.API/Extensions/` — расширения DI/приложения.

## Data (`CigarHelper.Data/`)

- `Data/AppDbContext.cs` — DbSets и Fluent API.
- `Models/` — сущности.
- `Migrations/` — EF Core миграции.
- `Data/DesignTimeDbContextFactory.cs` — design-time для `dotnet ef`.

## Import (`CigarHelper.Import/`)

- Хост generic host, регистрация `AppDbContext`, запуск импорта по пути к CSV из аргументов/логики в `Program`.

## Frontend (`CigarHelper.Web/`)

Не часть `.sln`. Страницы `src/views/`, API-обёртки `src/services/`, маршруты `src/router/index.ts`. Подробнее: [frontend/code-map.md](./frontend/code-map.md).

## Тесты (`CigarHelper.Api.Tests/`)

- Фабрика: `AuthIntegrationWebAppFactory.cs`.
- Интеграция: `AuthStep4IntegrationTests.cs` и др.
- Unit: `AuthServiceTests`, `JwtServiceTests`, `ImageBinaryValidatorTests`, сервисные тесты для профиля, админа, хьюмидора, отзывов.

## Конфигурация API

- `CigarHelper.API/appsettings.json`, окружения, `appsettings.Production.json` (плейсхолдеры для выката).
- Секреты: User Secrets / переменные окружения (не коммитить реальные значения).
