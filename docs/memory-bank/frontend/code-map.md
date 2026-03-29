# Карта кода — CigarHelper.Web

## Структура `src/`

| Путь | Назначение |
|------|------------|
| `main.ts` | Bootstrap приложения |
| `App.vue` | Оболочка: навигация, auth UI |
| `router/index.ts` | Маршруты и `beforeEach` |
| `services/api.ts` | Axios instance, interceptors |
| `services/authService.ts` | Логин, регистрация, токен, состояние |
| `services/useAuth.ts` | Композабл для Vue: `isAuthenticated`, `user`, `logout` |
| `services/*Service.ts` | Доменные вызовы API (профиль, сигары, хьюмидоры, отзывы, админ) |
| `utils/roles.ts` | Роли из JWT |
| `utils/imageUtils.ts`, `cigarOptions.ts` | Вспомогательные утилиты |
| `views/*.vue` | Страницы |
| `components/*.vue` | Переиспользуемые блоки (диалоги, редактор, загрузка изображений) |
| `assets/main.css` | Глобальные стили |

## Маршруты (сжато)

- Публичные: `/`, `/login` (`meta.public`), `/u/:username`, `/u/:username/humidors/:humidorId`, список/деталь отзывов `/reviews`, `/reviews/:id`.
- Требуют auth: профиль, хьюмидоры, сигары, формы, каталог-базы `/cigar-bases`, создание/редактирование отзывов.
- Требуют **Admin**: `/brands`, `/admin/users`.

Guards: неавторизованный → `/login?redirect=…`; авторизованный на `/login` → `/`; без роли Admin на admin-маршруты → `/`.

## Сервисы API (именование)

Имена файлов по домену: `profileService`, `adminUsersService`, `humidorService`, `reviewService`, `cigarService`, `authService`. Точные пути REST дублировать в документе не обязательно — смотреть вызовы `api.get/post/...` в соответствующих файлах.

## Компоненты

- **CigarDetailDialog**, **CigarBaseEditDialog** — работа с данными сигар/каталога.
- **TextEditor** — TipTap.
- **ImageUploader** — загрузка изображений в связке с API.
