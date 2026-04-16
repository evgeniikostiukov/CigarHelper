# Запуск CigarHelper в Docker

Инструкция для **полного стека** в Docker: PostgreSQL, MinIO, **CigarHelper.Api**, статика **CigarHelper.Web** (nginx проксирует `/api` в контейнер API). Для сценария «только БД и объектное хранилище» по-прежнему достаточно `docker compose up -d postgres minio` (см. [memory-bank/workflow.md](./memory-bank/workflow.md)).

## Требования

- [Docker Engine](https://docs.docker.com/engine/install/) и Docker Compose v2
- Файл **`.env`** в корне репозитория (шаблон — [`.env.example`](../.env.example))

## Подготовка `.env`

1. Скопируйте `.env.example` в `.env`.
2. Задайте пароли Postgres и MinIO (`POSTGRES_*`, `MINIO_ROOT_*`).
3. Задайте **`JWT_KEY`** — строка **не короче 32 символов** (секрет подписи JWT); в репозиторий не коммитить.
4. При необходимости измените **`WEB_PORT`** (порт SPA в браузере, по умолчанию `8080`). CORS для API подставляется как `http://localhost:<WEB_PORT>` — открывайте приложение с тем же хостом и портом.
5. Если заходите с **`http://127.0.0.1:<порт>`** (или другого origin), добавьте его в CORS. Удобно скопировать шаблон **`docker-compose.override.example.yml`** → **`docker-compose.override.yml`** (этот файл в [`.gitignore`](../.gitignore) и не попадает в git):

```bash
cp docker-compose.override.example.yml docker-compose.override.yml
```

При несовпадении порта с `WEB_PORT` отредактируйте значения в override. Для произвольного IP в LAN см. закомментированный пример в том же файле.

## Первый запуск: миграции БД

Образ API **не содержит EF Tools** — миграции удобно применить **с хоста**, пока поднят Postgres:

```bash
docker compose up -d postgres
# подставьте значения из .env:
set ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=cigarhelper;Username=cigarhelper;Password=ВАШ_ПАРОЛЬ
dotnet ef database update --project CigarHelper.Data/CigarHelper.Data.csproj --startup-project CigarHelper.API/CigarHelper.Api.csproj
```

(В PowerShell переменная окружения: `$env:ConnectionStrings__DefaultConnection="..."`.)

После этого можно останавливать только postgres не обязательно — переходите к полному профилю.

## Запуск полного стека

Убедитесь, что в `.env` задан **`JWT_KEY`** (иначе Compose выдаст предупреждение, а API не сгенерирует валидные токены).

Из **корня репозитория**:

```bash
docker compose --profile full up -d --build
```

- **SPA:** `http://localhost:<WEB_PORT>` (по умолчанию `http://localhost:8080`)
- **API напрямую (Swagger и отладка):** `http://localhost:<API_HOST_PORT>` (по умолчанию `5184`), путь Swagger обычно `/swagger`
- **MinIO Console:** `http://localhost:9001`

Проверка API: `GET http://localhost:5184/health` и `GET http://localhost:5184/health/ready` (порты с учётом `API_HOST_PORT`).

## Остановка и данные

```bash
docker compose --profile full down
```

Тома **`cigarhelper-pg`** и **`cigarhelper-minio`** сохраняют данные между перезапусками. Удалить их: `docker compose down -v` (осторожно: БД и файлы MinIO обнулятся).

## Переменные окружения (обзор)

| Переменная | Назначение |
|------------|------------|
| `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB` | БД для API |
| `MINIO_ROOT_USER`, `MINIO_ROOT_PASSWORD` | Ключи MinIO; те же значения передаются в API как `ImageStorage:Minio:AccessKey` / `SecretKey` |
| `JWT_KEY` | Секрет JWT (≥32 символов) |
| `WEB_PORT` | Порт на хосте для nginx со SPA; также участвует в `Cors__Origins__0` |
| `API_HOST_PORT` | Порт на хосте для прямого доступа к Kestrel (по умолчанию `5184`) |

В профиле `full` для API выставляются: `ImageStorage__Minio__Endpoint=minio:9000`, `ForwardedHeaders__Enabled=false` (запросы к Kestrel идут из сети compose без внешнего reverse proxy).

## Сборка отдельных образов

```bash
docker build -f Dockerfile.api -t cigarhelper-api .
docker build -t cigarhelper-web ./CigarHelper.Web
```

## Образы в GHCR (без сборки на сервере)

Workflow **[`.github/workflows/publish-ghcr.yml`](../.github/workflows/publish-ghcr.yml)** собирает и пушит в **GitHub Container Registry** два образа:

- `ghcr.io/<владелец-в-нижнем-регистре>/cigarhelper-api`
- `ghcr.io/<владелец-в-нижнем-регистре>/cigarhelper-web`

Триггеры: push в ветку **`main`**, теги вида **`v*`**, ручной запуск **Actions → Publish images to GHCR**. Для ветки `main` дополнительно выставляется тег **`latest`**, плюс теги по **git SHA** и semver (если пушите релизный тег).

На сервере:

1. Создать [Personal Access Token](https://github.com/settings/tokens) с правом **`read:packages`** (классический PAT) или использовать подходящий fine-grained token.
2. Войти в реестр: `echo <TOKEN> | docker login ghcr.io -u <github-username> --password-stdin`
3. В **`.env`** задать **`GHCR_IMAGE_NAMESPACE`** — тот же владелец пакетов, **строчными буквами**, что и в URL на GitHub Packages, и **`IMAGE_TAG`** (например `latest`, имя ветки или короткий SHA из Actions).
4. Поднять стек **без** `--build`, с подмешиванием **[`docker-compose.ghcr.example.yml`](../docker-compose.ghcr.example.yml)**:

```bash
docker compose -f docker-compose.yml -f docker-compose.ghcr.example.yml --profile full pull
docker compose -f docker-compose.yml -f docker-compose.ghcr.example.yml --profile full up -d
```

То же из скрипта (удобно на сервере): **[`scripts/deploy-docker-ghcr.sh`](../scripts/deploy-docker-ghcr.sh)** — `chmod +x scripts/deploy-docker-ghcr.sh`, затем из корня каталога с compose: `./scripts/deploy-docker-ghcr.sh`. Прод: `./scripts/deploy-docker-ghcr.sh --production`. Без `pull`: `--no-pull`. Явный каталог: `DEPLOY_REPO_ROOT=/path/to/compose ./scripts/deploy-docker-ghcr.sh`.

**Без полного репозитория:** на сервер достаточно скопировать (scp/rsync/artefact) минимум: `docker-compose.yml`, `docker-compose.ghcr.example.yml` (или ваш копипаст), при проде — `docker-compose.production.yml`, файл **`.env`**, по желанию **`scripts/deploy-docker-ghcr.sh`**. Исходники .NET/Vue в рантайме не нужны. Скрипт можно положить **в ту же папку**, что и `docker-compose.yml` (плоская раскладка) — тогда корень определится автоматически. Миграции EF с хоста по-прежнему требуют отдельно checkout/SDK или другой способ наката БД (см. раздел про миграции выше).

С прод-оверлеем (как в разделе ниже) порядок файлов такой:

```bash
docker compose -f docker-compose.yml -f docker-compose.production.yml -f docker-compose.ghcr.example.yml --profile full pull
docker compose -f docker-compose.yml -f docker-compose.production.yml -f docker-compose.ghcr.example.yml --profile full up -d
```

Файл **`docker-compose.ghcr.example.yml`** задаёт **`image`** для `api` и `web` и снимает унаследованный **`build`** через **`build: null`** (поведение merge в Compose v2 по спецификации). Если локальный Compose очень старый и ругается, обновите Docker Engine / Compose plugin.

Пакеты по умолчанию могут быть **приватными**; при необходимости в настройках пакета на GitHub задайте видимость или привяжите репозиторий.

## Продакшен на одном сервере (вариант A)

Когда **фронт и API** крутятся в Docker на одной машине, а снаружи стоит **Caddy/Nginx с HTTPS**, удобно подключить второй файл compose (шаблон в git, рабочая копия не коммитится):

```bash
cp docker-compose.production.example.yml docker-compose.production.yml
```

В **`.env`** задайте минимум:

- те же секреты, что и для локального `full`-стека;
- **`PUBLIC_WEB_ORIGIN`** — точный origin из адресной строки браузера, например `https://31-177-83-239.sslip.io` (схема + хост, без пути; порт указывайте только если он нестандартный).

Запуск:

```bash
docker compose -f docker-compose.yml -f docker-compose.production.yml --profile full up -d --build
```

Что меняет `docker-compose.production.yml` по сравнению с базовым файлом:

- `ASPNETCORE_ENVIRONMENT=Production` (подхватывается `appsettings.Production.json`, в т.ч. `AllowedHosts`/CORS-шаблон);
- `ForwardedHeaders` включены, `ForwardLimit=2` (типично: edge → nginx → Kestrel), **`TrustPrivateNetworks`** — чтобы заголовки учитывались для запросов с контейнера `web` по внутренней сети Docker;
- **CORS:** основной origin — `PUBLIC_WEB_ORIGIN`, дополнительно `http://localhost` и `http://127.0.0.1` с портом `PUBLIC_WEB_PORT` (по умолчанию как `WEB_PORT`) для проверок с самого сервера.

Конфиг **nginx** в образе фронта пробрасывает в API вычисленный `X-Forwarded-Proto` (с учётом того, что прислал edge) и `X-Forwarded-Host`. Для тел больших JSON (обзоры с фото в виде `data:` URL) задан **`client_max_body_size`** (см. `CigarHelper.Web/nginx.docker.conf`), в паре с **`MaxRequestBodySize`** у Kestrel в `Program.cs`.

### 400 «Invalid Hostname» на `/api/*`

Ответ от **ASP.NET Core** (host filtering): заголовок **`Host`**, который видит приложение, не входит в **`AllowedHosts`**.

Проверьте на сервере:

1. Контейнер API в **Production**: `docker exec <api> printenv ASPNETCORE_ENVIRONMENT` → должно быть `Production`. Если `Development`, подключается базовый `appsettings.json` с `AllowedHosts` только для `localhost`.
2. Фактический список хостов: `docker exec <api> printenv AllowedHosts` (в прод-примере задаётся через compose / переменную **`ALLOWED_HOSTS`** в `.env`).
3. Подняли стек с **обоими** файлами: `docker compose -f docker-compose.yml -f docker-compose.production.yml …`.
4. После смены `appsettings` или compose выполнен **`--build`** и пересоздание контейнера API.

Если публичное имя не sslip, в `.env` задайте, например: `ALLOWED_HOSTS=example.com;www.example.com`.

## Продакшен (общие замечания)

Базовый `docker-compose.yml` рассчитан на **локальную** и **демо**-среду. Для боя используйте сильные секреты, HTTPS на edge, при необходимости чеклисты [security-refactor-memory-bank.md](./security-refactor-memory-bank.md) и [memory-bank/security-deploy-checklist.md](./memory-bank/security-deploy-checklist.md).

## Countly (отдельный Compose)

В репозитории есть **`docker-compose.countly.yml`** — Community Edition (MongoDB, API, frontend, nginx). Проект Compose задан как **`cigarhelper-countly`**, чтобы контейнеры не смешивались в `docker compose ps` с основным стеком из того же каталога.

Запуск из корня:

```bash
docker compose -f docker-compose.countly.yml up -d
```

- **Панель и базовый URL для SDK:** `http://localhost:8888` (порт задаётся переменной **`COUNTLY_HOST_PORT`**, по умолчанию `8888`).
- **Vue SPA:** переменные `VITE_COUNTLY_*` и прокси в dev — [memory-bank/frontend/workflow.md](./memory-bank/frontend/workflow.md) (раздел Countly), шаблон **`CigarHelper.Web/.env.example`**.
- **Первый заход:** мастер создания администратора и настройки сервера.
- **Остановка:** `docker compose -f docker-compose.countly.yml down` (данные MongoDB в томе `cigarhelper-countly_countly-mongodb-data`; полное удаление данных: `down -v`).
- Конфиг nginx для прокси: **`docker/countly/nginx.server.conf`**.

На Windows, если `docker compose` ругается на **`docker-credential-desktop`**, добавьте в `PATH` каталог `C:\Program Files\Docker\Docker\resources\bin` (или запускайте Compose из **Docker Desktop** / терминала, где PATH уже настроен).
