# Memory bank — CigarHelper.Web (frontend)

SPA на **Vue 3**, сборка **Vite**, UI **PrimeVue** (тема Aura), стили **Tailwind 4**. Общается с backend по префиксу `/api` (в dev — прокси на Kestrel).

| Файл | Содержание |
|------|------------|
| [overview.md](./overview.md) | Стек, точка входа, auth, CORS/прокси |
| [code-map.md](./code-map.md) | Маршруты, сервисы, views, компоненты |
| [workflow.md](./workflow.md) | Установка, dev/build, согласование порта с API |

Связанный backend: [../code-map.md](../code-map.md), [../workflow.md](../workflow.md). Безопасность API: [../../security-refactor-memory-bank.md](../../security-refactor-memory-bank.md) (для CORS и прод-хостов важно совпадение origin с настройкой `Cors:Origins`).
