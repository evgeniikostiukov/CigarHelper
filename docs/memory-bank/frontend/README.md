# Memory bank — CigarHelper.Web (frontend)

SPA на **Vue 3**, сборка **Vite**, UI **PrimeVue** (тема Aura), стили **Tailwind 4**. Общается с backend по префиксу `/api` (в dev — прокси на Kestrel).

| Файл | Содержание |
|------|------------|
| [overview.md](./overview.md) | Стек, точка входа, auth, CORS/прокси; **тексты в UI** (краткие пояснения без тех. деталей) |
| [code-map.md](./code-map.md) | Маршруты, сервисы, views, компоненты |
| [collection-list-views.md](./collection-list-views.md) | **App** (оболочка), **Home**, **Login**, **HumidorList**, **HumidorDetail**, **Profile**, **PublicUserProfile**, **PublicHumidorDetail**, **HumidorForm**, **CigarList**, **CigarForm**, **CigarDetail**, **CigarBases**, **ReviewList**, **ReviewDetail**, **ReviewForm**, **Brands**, **AdminUsers**: общий паттерн, маршруты, `data-testid`, таблица vs карточки |
| [workflow.md](./workflow.md) | Установка, dev/build, согласование порта с API, ссылка на E2E |

Связанный backend: [../code-map.md](../code-map.md), [../workflow.md](../workflow.md). Безопасность API: [../../security-refactor-memory-bank.md](../../security-refactor-memory-bank.md) (для CORS и прод-хостов важно совпадение origin с настройкой `Cors:Origins`).
