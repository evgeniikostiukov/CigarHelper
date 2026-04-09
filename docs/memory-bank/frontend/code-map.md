# Карта кода — CigarHelper.Web

## Структура `src/`

| Путь | Назначение |
|------|------------|
| `main.ts` | Bootstrap приложения |
| `App.vue` | Корневая оболочка SPA: см. раздел [ниже](#appvue-оболочка) |
| `router/index.ts` | Маршруты и `beforeEach` |
| `services/api.ts` | Axios instance, interceptors |
| `services/authService.ts` | Логин, регистрация, токен, состояние |
| `services/useAuth.ts` | Композабл для Vue: `isAuthenticated`, `user`, `logout` |
| `services/*Service.ts` | Доменные вызовы API (профиль, сигары, хьюмидоры, отзывы, админ) |
| `utils/roles.ts` | Роли из JWT |
| `utils/imageUtils.ts`, `cigarOptions.ts` | Вспомогательные утилиты |
| `views/*.vue` | Страницы |
| `views/Home.vue` | Главная `/`: тот же визуальный каркас, см. [collection-list-views.md](./collection-list-views.md) |
| `views/Login.vue` | Вход и регистрация `/login` (`meta.public: true`), см. [collection-list-views.md](./collection-list-views.md) |
| `views/Dashboard.vue`, `views/HumidorList.vue`, `views/HumidorDetail.vue`, `views/Profile.vue`, `views/PublicUserProfile.vue`, `views/PublicHumidorDetail.vue`, `views/HumidorForm.vue`, `views/CigarList.vue`, `views/CigarForm.vue`, `views/CigarCollectionEdit.vue`, `views/CigarDetail.vue`, `views/CigarBases.vue`, `views/ReviewList.vue`, `views/ReviewDetail.vue`, `views/ReviewForm.vue`, `views/Brands.vue`, `views/AdminLayout.vue`, `views/AdminDashboard.vue`, `views/AdminUsers.vue`, `views/AdminImages.vue` | Дашборд-сводка, коллекция (добавление из базы / правка личных полей), профиль, публичный профиль и публичный хьюмидор, база, обзоры, админ-панель (`/admin`: обзор, пользователи, изображения в хранилище), бренды: единый стиль, см. [collection-list-views.md](./collection-list-views.md) |
| `components/*.vue` | Переиспользуемые блоки (диалоги, редактор, загрузка изображений) |
| `assets/main.css` | Глобальные стили |

## App.vue (оболочка)

| Пункт | Значение |
|-------|----------|
| Файл | `src/App.vue` |
| Контейнер | `app-container` + `app-shell`: градиент фона `stone`/`amber` (как у страниц коллекции), safe-area inset |
| Шапка | `sticky` `app-header`: полупрозрачный фон, `backdrop-blur`, бордер `stone`; внутри PrimeVue `Menubar` (`app-menubar-bar`, `max-w-7xl`) |
| Старт | Ссылка бренда на `Home`, подпись в стиле коллекции (uppercase tracking, amber/stone) |
| Меню | Пункты через `menuItems` + вычислимый `menuItemsVisible` (`visible()`); переходы **именованными** маршрутами: `Dashboard`, `HumidorList`, `CigarList`, `CigarBases`, `Brands`, `AdminDashboard`, `ReviewList` |
| Правая колонка | Профиль → `Profile`; выход — `Login`; гость — «Войти» (текст на `sm+`, иконка на мобиле); `min-h-11`, `touch-manipulation` |
| Контент | `main` (`app-main`): `Suspense` + `router-view`; fallback — панель «Загрузка экрана…», `app-suspense-fallback` |
| Глобально | `Toast`, `ConfirmDialog` в корне |
| Стили | Неглобальные переопределения `:deep(.app-menubar-bar …)` — hover/focus amber, `prefers-reduced-motion` для переходов пунктов |
| `data-testid` | `app`, `app-header`, `app-nav`, `app-nav-brand`, `app-nav-profile`, `app-nav-logout`, `app-nav-login-wide`, `app-nav-login-icon`, `app-main`, `app-router-outlet`, `app-suspense-fallback` |

Подробнее про визуальный паритет с экранами коллекции: [collection-list-views.md](./collection-list-views.md) (раздел **App.vue**).

## Маршруты (сжато)

- Публичные: `/`, `/login` (`meta.public`), `/u/:username`, `/u/:username/humidors/:humidorId`, список/деталь отзывов `/reviews`, `/reviews/:id`.
- Требуют auth: `/dashboard` (сводка), профиль, хьюмидоры, сигары, формы, каталог-базы `/cigar-bases`, справочник брендов `/brands` (просмотр всем залогиненным; кнопки создания/редактирования/удаления — только **Admin**/**Moderator** в UI), создание/редактирование отзывов.
- Требуют **Admin** (или Moderator для части `/admin`): вложенные `/admin`, `/admin/users`, `/admin/images` (родитель `/admin` с `AdminLayout`); `/brands` к этому не относится.

Guards: неавторизованный → `/login?redirect=…`; авторизованный на `/login` → `/`; без нужной роли на admin-маршруты → `/`.

## Сервисы API (именование)

Имена файлов по домену: `dashboardService`, `profileService`, `adminUsersService`, `humidorService`, `reviewService`, `cigarService`, `authService`. Точные пути REST дублировать в документе не обязательно — смотреть вызовы `api.get/post/...` в соответствующих файлах.

## Компоненты

- **CigarDetailDialog**, **CigarBaseEditDialog** — работа с данными сигар/каталога.
- **TextEditor** — TipTap.
- **ImageUploader** — скрытый `input` файлов; обычно используется внутри **FormImageGallerySection**.
- **FormImageGallerySection** — общая секция галереи (файлы + URL одной строкой или **несколько полей URL** + «Добавить в галерею», превью, лимит, soft-delete, опционально подписи и «главное» фото); `ReviewForm`, `CigarBaseEditDialog`.
