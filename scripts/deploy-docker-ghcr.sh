#!/usr/bin/env bash
# Деплой на сервере: Postgres + MinIO + API + Web из образов GHCR (без сборки на хосте).
#
# Требования: Docker Engine + Compose v2, в корне репозитория файл .env с секретами и
# GHCR_IMAGE_NAMESPACE / IMAGE_TAG (см. .env.example и docs/docker.md).
# Один раз: docker login ghcr.io
#
# Использование (из любого каталога):
#   bash scripts/deploy-docker-ghcr.sh
#   bash scripts/deploy-docker-ghcr.sh --production
#   bash scripts/deploy-docker-ghcr.sh --no-pull
#
# Переменные окружения:
#   DEPLOY_REPO_ROOT — корень репо с compose-файлами (по умолчанию родитель каталога scripts/)
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
ROOT="${DEPLOY_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd)}"
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
