# Overview

## Назначение

Backend **Cigar Helper** — ASP.NET Core Web API для учёта сигар, брендов, хьюмидоров, отзывов и изображений (каталог и пользовательский контент). Консольный проект импортирует данные каталога из CSV в ту же PostgreSQL через EF Core.

## Стек

- **.NET / C#** (target `net10.0` в проектах solution).
- **ASP.NET Core** — API, JWT Bearer, Swagger, rate limiting (см. `Program.cs`).
- **EF Core + Npgsql** — `AppDbContext`, миграции в `CigarHelper.Data/Migrations`.
- **Тесты** — `CigarHelper.Api.Tests` (unit + интеграция с `WebApplicationFactory` в `ASPNETCORE_ENVIRONMENT=Testing`).

## Решение `CigarHelper.sln`

| Проект | Роль |
|--------|------|
| `CigarHelper.Api` | HTTP API, бизнес-логика в `Services/`, контроллеры |
| `CigarHelper.Data` | Модели, `AppDbContext`, миграции, TypeGen |
| `CigarHelper.Import` | Импорт CSV → БД |
| `CigarHelper.Api.Tests` | Тесты |

Папка на диске API: `CigarHelper.Api` (имя сборки/`.csproj`: `CigarHelper.Api`; на Linux регистр пути должен совпадать с git).

**Фронтенд** в solution не входит: каталог **`CigarHelper.Web`** (Vue 3 + Vite). Контекст: [memory-bank/frontend/README.md](./frontend/README.md).

## Домен (сущности в `AppDbContext`)

- `User`, `Brand`, `CigarBase`, `UserCigar`, `Humidor`, `Review`, `ReviewImage`, `CigarImage`.

Уникальные индексы: email/username у пользователя, имя бренда, пара (имя сигары, бренд) у каталога.

## Безопасность

Актуальные решения (JWT, пароли, rate limit, CORS, изображения, хосты и т.д.) задокументированы в [../security-refactor-memory-bank.md](../security-refactor-memory-bank.md). Этот memory bank их не дублирует.

## Репозиторий

- Основная ветка разработки на момент создания bank: `feature/work` (может быть ahead от `origin`).
- Сообщения коммитов: conventional commits (англ.), правила чата проекта — русский для общения с пользователем.
