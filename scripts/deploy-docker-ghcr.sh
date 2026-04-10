#!/usr/bin/env bash
# Деплой на сервере: Postgres + MinIO + API + Web из образов GHCR (без сборки на хосте).
#
# Целый git-клон не обязателен: достаточно каталога с compose-файлами, .env и (по желанию) этого скрипта.
# Варианты раскладки:
#   - как в репо: .../docker-compose.yml и .../scripts/deploy-docker-ghcr.sh
#   - «плоско»: всё в одной папке, включая этот скрипт и docker-compose.yml
#
# Требования: Docker Compose v2, .env с секретами и GHCR_IMAGE_NAMESPACE / IMAGE_TAG (см. .env.example).
# Один раз: docker login ghcr.io
#
# Использование:
#   ./scripts/deploy-docker-ghcr.sh
#   ./scripts/deploy-docker-ghcr.sh --production
#   ./scripts/deploy-docker-ghcr.sh --no-pull
#
# Переменные окружения:
#   DEPLOY_REPO_ROOT — явный каталог с compose (если не задан: ищется рядом со скриптом или на уровень выше)
#   GHCR_COMPOSE_FILE — файл с image: для api/web (по умолчанию docker-compose.ghcr.example.yml)

set -euo pipefail

USE_PRODUCTION=0
DO_PULL=1

while [[ $# -gt 0 ]]; do
  case "$1" in
    --production | -p)
      USE_PRODUCTION=1
      shift
      ;;
    --no-pull)
      DO_PULL=0
      shift
      ;;
    -h | --help)
      sed -n '1,20p' "$0"
      exit 0
      ;;
    *)
      echo "Неизвестный аргумент: $1 (см. --help)" >&2
      exit 1
      ;;
  esac
done

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ -n "${DEPLOY_REPO_ROOT:-}" ]]; then
  ROOT="$(cd "$DEPLOY_REPO_ROOT" && pwd)"
elif [[ -f "$SCRIPT_DIR/docker-compose.yml" ]]; then
  ROOT="$SCRIPT_DIR"
elif [[ -f "$SCRIPT_DIR/../docker-compose.yml" ]]; then
  ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
else
  echo "Ошибка: не найден docker-compose.yml рядом со скриптом и не задан DEPLOY_REPO_ROOT." >&2
  echo "Скопируйте на сервер compose-файлы и .env или укажите: DEPLOY_REPO_ROOT=/path/to/compose ./deploy-docker-ghcr.sh" >&2
  exit 1
fi

cd "$ROOT"

GHCR_FILE="${GHCR_COMPOSE_FILE:-docker-compose.ghcr.example.yml}"

if [[ ! -f "$ROOT/.env" ]]; then
  echo "Ошибка: нет файла $ROOT/.env (скопируйте из .env.example и задайте значения)." >&2
  exit 1
fi

if [[ ! -f "$ROOT/$GHCR_FILE" ]]; then
  echo "Ошибка: нет файла $ROOT/$GHCR_FILE (задайте GHCR_COMPOSE_FILE или положите compose-файл)." >&2
  exit 1
fi

if ! docker compose version >/dev/null 2>&1; then
  echo "Ошибка: команда «docker compose» недоступна." >&2
  exit 1
fi

COMPOSE_ARGS=(compose -f docker-compose.yml)

if [[ "$USE_PRODUCTION" -eq 1 ]]; then
  if [[ ! -f "$ROOT/docker-compose.production.yml" ]]; then
    echo "Ошибка: запрошен --production, но нет docker-compose.production.yml." >&2
    echo "Скопируйте шаблон: cp docker-compose.production.example.yml docker-compose.production.yml" >&2
    exit 1
  fi
  COMPOSE_ARGS+=(-f docker-compose.production.yml)
fi

COMPOSE_ARGS+=(-f "$GHCR_FILE" --profile full)

echo "Каталог: $ROOT"
echo "Compose: ${COMPOSE_ARGS[*]}"

if [[ "$DO_PULL" -eq 1 ]]; then
  docker "${COMPOSE_ARGS[@]}" pull
fi

docker "${COMPOSE_ARGS[@]}" up -d --remove-orphans

echo "Готово. Состояние контейнеров:"
docker "${COMPOSE_ARGS[@]}" ps
