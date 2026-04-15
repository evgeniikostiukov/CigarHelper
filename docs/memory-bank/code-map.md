# Карта кода

## Точки входа

- API: `CigarHelper.Api/Program.cs` (+ `ProgramPartial.cs` для интеграционных тестов).
- Import: `CigarHelper.Import/Program.cs` → `ImportCigarsFromCsv`; картинки CSV — `CigarImageStorageWriter` + `IImageStorageProvider` / `IThumbnailGenerator` (те же ключи `StoragePath` / `ThumbnailPath`, что при загрузке из UI).

## API — контроллеры (`CigarHelper.Api/Controllers/`)

| Контроллер | Назначение |
|------------|------------|
| `AuthController` | Регистрация, логин, JWT |
| `ProfileController` | Профиль текущего пользователя |
| `AdminUsersController` | Админ-операции с пользователями |
| `AdminCigarImagesController` | `GET /api/admin/cigar-images` — пагинированный список всех `CigarImages` (только роль Admin) |
| `PublicUsersController` | Публичные данные пользователей |
| `BrandsController` | Бренды: `GET` — любой JWT; `POST` / `PUT` / `DELETE` — только **Admin**, **Moderator** |
| `CigarsController` | Каталог / сигары; чтение баз (`GET .../bases`, `.../paginated`, `.../bases/{id}`, `.../brands`) — любой JWT; **`POST .../bases`** — любой JWT: карточка `IsModerated` только у **Admin**/**Moderator**; обычный пользователь может создать запись с **промодерированным** брендом (`GET .../brands`); **`PUT .../bases/{id}`** — только **Admin**, **Moderator**; `unmoderatedOnly=true` только для staff; **`POST /api/cigars`** — коллекция по любому существующему `CigarBaseId`; `CigarResponseDto.Images` — merged (`LoadMergedUserCigarGalleriesAsync`) |
| `HumidorsController` | Хьюмидоры пользователя |
| `ReviewsController` | Обзоры: `POST` с `cigarBaseId` и опционально `userCigarId` (запись коллекции); `GET` списка — `cigarBaseId` / `userCigarId` / `userId` (без строк с `DeletedAt`). Автор: `DELETE` — мягкое удаление (`DeletedAt`). |
| `AdminReviewsController` | Staff (`Admin`, `Moderator`): `GET /api/admin/reviews/deleted` — удалённые обзоры; `POST .../{id}/restore` — снять удаление. |
| `CigarCommentsController` | Комментарии к `CigarBase` и к чужим `UserCigar` в публичной коллекции: `GET/POST/DELETE api/cigarcomments` (публичный список только **одобренные**; у обычных пользователей новые — **на модерации**) |
| `AdminCigarCommentsController` | Очередь модерации: `GET/POST …/api/admin/cigar-comments` и `…/{id}/approve` / `reject` (роли **Admin**, **Moderator**) |
| `CigarImagesController` | Изображения сигар (авторизация, владение, роли) |

## API — сервисы и прочее

- `Program.cs`: эндпоинты **`GET /health`** (liveness) и **`GET /health/ready`** (готовность + EF к БД).
- `CigarHelper.Api/Services/` — `AuthService`, `JwtService`, `ProfileService`, `AdminUserService`, `HumidorService`, `ReviewService`, **`ImageService`** (оркестрация: validate → store → thumbnail) и др.
- `CigarHelper.Api/Storage/` — **`IImageStorageProvider`** + реализации **`MinioImageStorageProvider`** / `LocalFileImageStorage`; **`IThumbnailGenerator`** + `ImageSharpThumbnailGenerator`; **`CigarImageStorageWriter`** — общая запись оригинала + миниатюры (API и CSV-импорт).
- `CigarHelper.Api/Helpers/` — `ImageBinaryValidator` (проверка бинарных изображений), `ImageDownloader`.
- `CigarHelper.Api/Options/` — сильно типизированные опции конфигурации (`ImageUploadOptions`, **`ImageStorageOptions`**).
- `CigarHelper.Api/Extensions/` — расширения DI/приложения.

### Политика хранения изображений

Конфигурируется секцией `ImageStorage` в `appsettings.json`:

| `Provider` | Где хранятся данные | Когда использовать |
|---|---|---|
| `Minio` (default в `appsettings`) | Объекты в бакете (`ImageStorage:Minio`) | Основной режим API и импорта |
| `LocalFile` | Папка `ImageStorage:LocalPath` | Интеграционные тесты API (`WebApplicationFactory`), один инстанс без MinIO |

В таблице `CigarImages` хранятся только ключи `StoragePath` / `ThumbnailPath`, не bytea.

При добавлении провайдера — реализовать `IImageStorageProvider` и зарегистрировать в `Program.cs`.

### Миниатюры

- Генерируются автоматически при загрузке/обновлении изображения через `IImageService`.
- Формат: WebP, ограничение 320×320 px (конфиг: `ImageStorage:ThumbnailMaxWidth/Height`).
- Эндпоинты: `GET /api/cigar-images/{id}/data` (оригинал) и `GET /api/cigar-images/{id}/thumbnail` (WebP).
- `CigarImageDto.HasThumbnail = true` означает наличие миниатюры.

## Data (`CigarHelper.Data/`)

- `Data/AppDbContext.cs` — DbSets и Fluent API.
- `Models/` — сущности; таблица **`CigarComments`** — комментарии (ровно одна из связей: `CigarBaseId` или `UserCigarId`).
- `Migrations/` — EF Core миграции.
- `Data/DesignTimeDbContextFactory.cs` — design-time для `dotnet ef`.

## Import (`CigarHelper.Import/`)

- Хост generic host, регистрация `AppDbContext`, запуск импорта по пути к CSV из аргументов/логики в `Program`.

## Frontend (`CigarHelper.Web/`)

Не часть `.sln`. Страницы `src/views/`, API-обёртки `src/services/`, маршруты `src/router/index.ts`. Подробнее: [frontend/code-map.md](./frontend/code-map.md).

## Тесты (`CigarHelper.Api.Tests/`)

- Фабрика: `AuthIntegrationWebAppFactory.cs`.
- Интеграция: `AuthStep4IntegrationTests.cs`, `CigarsBasesPaginatedIntegrationTests.cs`, `ApiAuthorizationAndContractsIntegrationTests.cs` (границы доступа и контракт пагинации).
- Unit: `AuthServiceTests`, `JwtServiceTests`, `ImageBinaryValidatorTests`, сервисные тесты для профиля, админа, хьюмидора, отзывов.

## Конфигурация API

- `CigarHelper.Api/appsettings.json`, окружения, `appsettings.Production.json` (плейсхолдеры для выката).
- Секреты: User Secrets / переменные окружения (не коммитить реальные значения).
