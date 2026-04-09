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

Конфиг **nginx** в образе фронта пробрасывает в API вычисленный `X-Forwarded-Proto` (с учётом того, что прислал edge) и `X-Forwarded-Host`.

## Продакшен (общие замечания)

Базовый `docker-compose.yml` рассчитан на **локальную** и **демо**-среду. Для боя используйте сильные секреты, HTTPS на edge, при необходимости чеклисты [security-refactor-memory-bank.md](./security-refactor-memory-bank.md) и [memory-bank/security-deploy-checklist.md](./memory-bank/security-deploy-checklist.md).
