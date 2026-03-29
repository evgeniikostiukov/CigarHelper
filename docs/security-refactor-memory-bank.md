# Memory bank: security refactor (CigarHelper)

Общий контекст репозитория (продукт, карта кода, запуск): [memory-bank/README.md](./memory-bank/README.md).

Документ для продолжения работы в новом чате. Якорь: аудит безопасности с начала диалога, пошаговые исправления с подтверждением пользователя.

---

## План (из аудита, изначальный порядок)

| Шаг | Тема | Статус |
|-----|------|--------|
| 1 | Секреты из репозитория → placeholders, User Secrets, `.gitignore` для `**/appsettings.*.local.json` | Сделано и закоммичено |
| 2 | IDOR в `CigarImagesController` (владение, роли Admin/Moderator для каталога) | Сделано и закоммичено |
| 3 | Пароли: PBKDF2-SHA256 (310k iter), legacy HMAC-SHA512 + апгрейд при логине | Сделано и закоммичено |
| 4 | Логин: единое сообщение, rate limit login/register | Сделано и закоммичено |
| 5 | `AuthResponse.Expiration` = реальный срок JWT (не парсинг мок-токена; срок из эмиттера JWT) | Сделано и закоммичено |
| 6 | Убрать чувствительный debug из `AuthController` (Console + длина пароля) | Сделано и закоммичено |
| 7 | `AllowedHosts без *`, CORS из конфига, валидация бинарных изображений, `Include Error Detail` для Npgsql | Сделано и закоммичено |
| 8 | HSTS (только Production), базовые security-заголовки ответа (`nosniff`, `X-Frame-Options`, `Referrer-Policy`) | Сделано и закоммичено |

---

## Репозиторий / ветка

- Ветка: `feature/work` (на момент записи **ahead** от `origin`).
- Последние релевантные коммиты (хронология снизу вверх по времени разработки):
  - `chore(security): externalize secrets…`
  - `chore(cursor): extend common workspace rule`
  - `fix(api): enforce ownership on cigar image endpoints`
  - `feat(auth): store passwords with PBKDF2-SHA256`
  - `feat(auth): unify login failures and rate-limit auth`
  - `test(api): add auth integration tests for step 4` — `WebApplicationFactory`, `ProgramPartial`, Testing + InMemory в `Program.cs`
  - `fix(auth): align JWT expiration with token and IJwtService tuple` — шаг 5
  - `fix(api): remove sensitive register debug logging` — шаг 6
  - `chore(security): harden hosts, CORS, image uploads, npgsql error detail` — шаг 7
  - `chore(security): add HSTS and baseline response security headers` — шаг 8

---

## Ключевые технические решения (кратко)

### Секреты

- В коммите только плейсхолдеры в `appsettings*.json`; реальные значения — User Secrets / env (`ConnectionStrings__DefaultConnection`, `Jwt:Key`).
- **Data / Import / API** имеют свои `UserSecretsId` где настроено.

### Шаг 2 — изображения

- Контроллер: `[Authorize]` классом; список без фильтров → `400`; `UserCigar` — владелец или staff; `CigarBase` — mutate только Admin/Moderator.
- **Breaking:** анонимный доступ к `GET` списку всех изображений убран.

### Шаг 3 — пароли

- Новый формат: соль 16 байт, хеш 32 байта PBKDF2; legacy: 64-байтовый HMAC-SHA512 + любая длина ключа.
- `AuthService.LoginAsync` при `rehashWithModernAlgorithm` перезаписывает хеш перед `SaveChanges`.

### Шаг 4 — auth

- Одно сообщение: `AuthService.LoginFailedMessage` = «Неверный email или пароль.»
- Rate limiting (`Program.cs`): политика `auth-login` **20/мин/IP**, `auth-register` **10/мин/IP**; партиция по `RemoteIpAddress`; `UseRateLimiter` после `UseAuthorization`.
- `[EnableRateLimiting]` на `AuthController` для `login` / `register`.

### Интеграционные тесты (шаг 4)

- `CigarHelper.Api/ProgramPartial.cs` — `public partial class Program` для `WebApplicationFactory<Program>`.
- `ASPNETCORE_ENVIRONMENT=Testing`: в `Program.cs` **только EF InMemory** (отдельное имя БД на запуск), **без** `ApplyMigrations`; иначе — Npgsql как раньше.
- Фабрика: `AuthIntegrationWebAppFactory` — `UseEnvironment("Testing")` + `UseSetting` для JWT/connection.
- Тесты: `AuthStep4IntegrationTests` — 429 после лимитов, единое сообщение логина, и т.д.

### Шаг 5 — сделано

- `IJwtService.GenerateToken` возвращает `(string Token, DateTime ExpiresAtUtc)`.
- `JwtService`: `SecurityTokenDescriptor.Expires` задаётся из `DateTime.UtcNow.AddDays(Jwt:AccessTokenDays)` (по умолчанию **7**); в ответе клиенту используется **`JwtSecurityToken.ValidTo`** того же созданного токена (совпадает с `ReadJwtToken(...).ValidTo`, без расхождения по субсекундам).
- `AuthService` не парсит строку JWT; `ProfileService` / `AdminUserService` берут только `Token` из кортежа.
- Интеграционные тесты читают `AuthResponse` с `JsonStringEnumConverter`, как в API.

### Шаг 6 — сделано

- Из `AuthController.Register` убраны `Console.WriteLine`: учётные данные, длины паролей, детали валидации и сообщения об ошибке регистрации не пишутся в stdout (утечки в лог-хосты / контейнеры).

### Шаг 7 — сделано

- **AllowedHosts:** в `appsettings.json` не `*`, а `localhost;127.0.0.1;[::1]`; в проде задать реальные хосты (env / переопределение конфига).
- **CORS:** секция `Cors` — `Origins[]`, опционально `PolicyName` (по умолчанию `DefaultCors`); `WithOrigins` + `AllowCredentials` как раньше.
- **Загрузки изображений:** `ImageUpload:MaxBytes` (по умолчанию 5 MiB), `ImageBinaryValidator` — magic JPEG/PNG/GIF/WebP, согласованность MIME и размера; `CreateCigarImage` сохраняет `ImageData` после проверки; `Update` при новом `ImageData` — те же проверки. Общая логика скачивания URL — через `ImageBinaryValidator` в `ImageDownloader`; `Console.WriteLine` при ошибке скачивания убран.
- **Npgsql:** перед `UseNpgsql` строка собирается через `NpgsqlConnectionStringBuilder` с `IncludeErrorDetail = IsDevelopment()` (в prod меньше деталей в исключениях).

### Шаг 8 — сделано

- **`AddHsts`:** `Preload = false`, `IncludeSubDomains = true`, `MaxAge` 365 дней; **`UseHsts`** только при `IsProduction()` (в dev/testing без HSTS, чтобы не ломать localhost и интеграционные тесты).
- **Заголовки:** `UseSecurityHeaders()` в начале пайплайна — `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: strict-origin-when-cross-origin` (см. `Extensions/SecurityHeadersMiddlewareExtensions.cs`). Если для части эндпоинтов нужен iframe (редко для API), политику придётся ослабить точечно.
- **Порядок:** security-заголовки и HSTS стоят до `UseHttpsRedirection`, как рекомендует шаблон ASP.NET Core.

### Продакшен (выкат)

- Шаблон: `CigarHelper.Api/appsettings.Production.json` — плейсхолдеры `yourdomain.example` (RFC 2606), строка БД `REQUIRED`, JWT key с пометкой задать через env / secret store. Перед выкатом заменить на реальные хосты и **не** коммитить секреты.
- Окружение: `ASPNETCORE_ENVIRONMENT=Production`.
- Типичные переопределения через переменные (имеют приоритет над JSON):
  - `ConnectionStrings__DefaultConnection`
  - `Jwt__Key`, при необходимости `Jwt__Issuer`, `Jwt__Audience`, `Jwt__AccessTokenDays`
  - `AllowedHosts` — один список через `;`, как в JSON
  - CORS: `Cors__Origins__0`, `Cors__Origins__1`, … для каждого HTTPS origin фронта (с `AllowCredentials` wildcard нельзя).
- За обратным прокси убедиться, что фактический **Host** запроса к Kestrel совпадает с перечислением в `AllowedHosts` (или настроить `ForwardedHeaders` и доверенные прокси — иначе возможны 400 Bad Request от host filtering).

---

## Файлы «рядом с темой»

| Область | Пути |
|--------|------|
| JWT / auth | `CigarHelper.API/Services/JwtService.cs`, `IJwtService.cs`, `AuthService.cs`, `Controllers/AuthController.cs` |
| Rate limit | `CigarHelper.API/Program.cs` |
| Изображения | `CigarHelper.API/Controllers/CigarImagesController.cs` |
| Тесты unit | `CigarHelper.Api.Tests/AuthServiceTests.cs`, `JwtServiceTests.cs`, … |
| Тесты integration | `CigarHelper.Api.Tests/AuthIntegrationWebAppFactory.cs`, `AuthStep4IntegrationTests.cs` |
| Program / Testing | `CigarHelper.API/Program.cs`, `ProgramPartial.cs` |
| Security headers / HSTS | `CigarHelper.API/Extensions/SecurityHeadersMiddlewareExtensions.cs`, `Program.cs` (пайплайн) |
| Прод конфиг API | `CigarHelper.Api/appsettings.Production.json` |

---

## Команды проверки

```bash
dotnet test CigarHelper.sln
```

Ожидаемо: все тесты зелёные.

---

## Язык и правила проекта

- Ответы пользователю на русском; коммиты в репозитории в стиле conventional commits (англ. summary), по желанию body на русском.
- В `.cursor/rules/common.mdc` добавлена строка про прямую обратную связь без лишних похвал.

---

*Обновляйте этот файл после завершения шага 5 и дальнейших шагов, чтобы следующий чат не расходился с репозиторием.*
