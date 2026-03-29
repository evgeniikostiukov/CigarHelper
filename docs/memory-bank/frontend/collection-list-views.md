# Коллекция, справочник, обзоры, админ и главная — Home, Login, HumidorList, HumidorDetail, Profile, PublicUserProfile, PublicHumidorDetail, CigarList, CigarForm, CigarDetail, CigarBases, ReviewList, ReviewDetail, ReviewForm, Brands, AdminUsers

Паритетный визуальный язык (градиент, grain, типографика Onest) для **корневой оболочки** (`App.vue`), **главной** (`Home.vue`), **входа** (`Login.vue`), **моих хьюмидоров**, **моих сигар**, **формы сигары** (`CigarForm.vue`), **карточки сигары** (`CigarDetail.vue`), **базы сигар** (`CigarBases.vue`), **ленты обзоров** (`ReviewList.vue`, `ReviewDetail.vue`), **публичного профиля** (`PublicUserProfile.vue`), **публичного состава хьюмидора** (`PublicHumidorDetail.vue`), **брендов** (`Brands.vue`) и **пользователей админки** (`AdminUsers.vue`). У базы на `lg+` сохранена **ленивая таблица** (`DataTable`) с серверной пагинацией и сортировкой; на мобиле — карточки и общий `Paginator`.

## Общее

| Аспект | Где задано |
|--------|------------|
| Шрифт интерфейса | `src/assets/main.css`: импорт `/fonts/onest/onest.css`, `@theme` → `--font-sans`, `body { font-family: var(--font-sans) }` |
| Оболочка страницы | Секция с градиентом `stone`/`amber`, `ring`, слой «grain» (SVG noise, data-URI в scoped CSS) |
| Сетка карточек | `grid-cols-1 sm:grid-cols-2 lg:grid-cols-3`, отступы `gap-5 sm:gap-6` |
| Заголовок | Подпись над `h1`: на **Home** — «Cigar Helper»; в личных списках — «Коллекция»; в **CigarBases** — «Справочник»; в **Brands** и **AdminUsers** — «Администрирование»; `h1` с `id` для `aria-labelledby`, подзаголовок |
| Primary CTA | PrimeVue `Button`: на мобиле `w-full`, `min-h-12`, маршрут через **именованный** route |
| Загрузка | `Skeleton` в сетке, `data-testid="…-loading"`, `min-h-[20rem]`, `aria-busy` |
| Ошибка | `Message` + кнопка «Повторить загрузку», отдельный `data-testid` для блока и retry |
| Пустой список | Иконка, текст, CTA; у **CigarBases** при нуле записей после фильтров — «Ничего не найдено» + «Сбросить фильтры» |
| Карточки | `article`, входная анимация (ступень по индексу), `prefers-reduced-motion`, `v-memo` по полям данных |
| Мобильность | Крупные зоны нажатия (`min-h-11`/`12`), `touch-manipulation`, `aria-label` у иконок-кнопок |
| Логи | `console.error` только при `import.meta.env.DEV` |

## App.vue

Корневая оболочка: `Toast`, `ConfirmDialog`, шапка с `Menubar`, область `main` с `Suspense` / `router-view`. Фон — лёгкий градиент `from-stone-100 via-amber-50/30` (dark: stone/amber), согласованный с карточными страницами. Шапка — `sticky`, полупрозрачность и `backdrop-blur`, бордер `stone`. Пункты меню и профиль — увеличенные зоны нажатия, hover/focus в палитре amber; пункты фильтруются через `menuItemsVisible`. Детали и дублирование с [code-map.md](./code-map.md#appvue-оболочка).

| Пункт | Значение |
|-------|----------|
| Файл | `src/App.vue` |
| Классы layout | `app-container`, `app-shell`, `app-header`, `app-main`, `app-menubar-bar` |
| `data-testid` | `app`, `app-header`, `app-nav`, `app-nav-brand`, `app-nav-profile`, `app-nav-logout`, `app-nav-login-wide`, `app-nav-login-icon`, `app-main`, `app-router-outlet`, `app-suspense-fallback` |

## Home.vue

Публичная точка входа `/` (`name: 'Home'`): та же оболочка, что у списков — `home-root`, grain, `max-w-7xl`. Подпись над заголовком — **Cigar Helper**, `h1` с `id="home-heading"`. Блок героя (`home-hero`) и три карточки возможностей (`article`, иконки в янтарном квадрате, анимация появления с `prefers-reduced-motion`).

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/Home.vue` |
| Auth | `useAuth()` → `isAuthenticated`; гость видит CTA «Начать» → `Login`; пользователь — «Мои хьюмидоры» (`HumidorList`), «Мои сигары» (`CigarList`, secondary outlined) |
| Контент фич | Массив `features` в скрипте (заголовок, текст, `testid`, класс иконки PrimeIcons) |
| `data-testid` | `home`, `home-hero`, `home-cta-row`, `home-cta-humidors`, `home-cta-cigars`, `home-cta-login`, `home-features`, `home-feature-humidors`, `home-feature-cigars`, `home-feature-organized` |
| Scoped классы | `home-root`, `home-grain`, `home-feature-enter` |

## Login.vue

Страница `/login` (`name: 'Login'`, `meta.public: true`): оболочка как у **HumidorList** — `login-root`, тот же grain, градиент stone/amber, `ring`; блок по центру (`flex`, `min-h` с `dvh`), внутри `max-w-md`. Подпись над заголовком — **«Cigar Helper»**, `h1` с `id="login-heading"` переключается между «Вход» и «Регистрация». Форма в панели `rounded-2xl` (`login-panel-enter`, `prefers-reduced-motion`), без Prime `Card`. Поля: подписи `text-xs` stone, инпуты `min-h-11`, `Password` с `toggle-mask`, `input-props` для `autocomplete`; общая ошибка — `Message` (`login-error`). Отправка — `authService.login` / `register`; редирект после успеха: `route.query.redirect` или `/`. Переключение режима — текстовая кнопка (`login-toggle-mode`); «На главную» → `Home` (`login-home`). Логи в `console` — только `DEV` при исключении.

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/Login.vue` |
| `data-testid` | `login`, `login-error`, `login-form`, `login-username`, `login-email`, `login-password`, `login-confirm-password`, `login-submit`, `login-toggle-mode`, `login-home` |
| Scoped классы | `login-root`, `login-grain`, `login-panel-enter` |

## HumidorList.vue

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/HumidorList.vue` |
| Данные | `humidorService.getHumidors()`, тип `Humidor` из `humidorService` |
| Маршруты | Список: `HumidorList`; добавление: `HumidorForm`; деталь: `HumidorDetail` (`:id`); правка: `HumidorEdit` |
| Карточка | Полноразмерный `router-link` оверлей на деталь (z-0), контент `pointer-events-none`, футер z-20 с кнопками |
| Футер карточки | Редактирование, удаление с `useConfirm` + `useToast`, `humidorService.deleteHumidor` |
| Доп. UI | `Badge` влажности через `humidorService.getHumiditySeverity` |
| `data-testid` | `humidor-list`, `humidor-list-loading`, `humidor-list-skeleton`, `humidor-list-error`, `humidor-list-retry`, `humidor-list-empty`, `humidor-list-empty-add`, `humidor-list-add`, `humidor-list-grid`, `humidor-card-{id}`, `humidor-edit-{id}`, `humidor-delete-{id}` |
| Scoped классы | `humidor-list-root`, `humidor-list-grain`, `humidor-card-enter`, `line-clamp-2/3` |

## HumidorDetail.vue

Детальная страница хьюмидора (`name: 'HumidorDetail'`, `params.id`): тот же каркас, что у **HumidorList** / **CigarDetail** — `humidor-detail-root`, grain (тот же data-URI), подпись **«Коллекция»**, `h1` с `id="humidor-detail-heading"`. Загрузка: скелетон (`humidor-detail-loading`, `humidor-detail-skeleton`); ошибка — `Message` + «Повторить загрузку» и «К списку хьюмидоров»; контент — `humidor-detail-content` с анимацией `humidor-detail-enter`. Статистика: три панели `rounded-2xl` без Prime `Card` — вместимость + `ProgressBar`, заглушка «Температура», влажность через `Badge` + `humidorService.getHumiditySeverity` или «Не указана». Таблица сигар в хьюмидоре: `DataTable` (`humidor-detail-table`), колонка названия — ссылка на **`CigarDetail`**, крепость — подписи из `strengthOptions` (`cigarOptions.ts`), удаление — `useConfirm` с `rejectLabel` / `acceptLabel` как в списке. Блок **добавления**: загрузка отдельным запросом `getCigars` и фильтром «не в текущем списке»; ошибка — `humidor-detail-available-retry`; сетка карточек `article` с `v-memo`, класс `available-card-enter`, кнопки `min-h-11`; предупреждение при полном хьюмидоре — `humidor-detail-full`. Логи ошибок — только в `import.meta.env.DEV`.

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/HumidorDetail.vue` |
| Данные | `humidorService.getHumidor(id)`, `cigarService.getCigars()`; изменение состава — `addCigarToHumidor`, `removeCigarFromHumidor` |
| Маршруты | Список: `HumidorList`; правка: `HumidorEdit` (`params.id`); сигара: `CigarDetail` (`params.id` строка) |
| `data-testid` | `humidor-detail`, `humidor-detail-loading`, `humidor-detail-skeleton`, `humidor-detail-error`, `humidor-detail-retry`, `humidor-detail-back`, `humidor-detail-edit`, `humidor-detail-to-list`, `humidor-detail-content`, `humidor-detail-stats`, `humidor-detail-table`, `humidor-detail-table-empty`, `humidor-detail-available`, `humidor-detail-available-loading`, `humidor-detail-available-error`, `humidor-detail-available-retry`, `humidor-detail-available-empty`, `humidor-detail-available-grid`, `humidor-detail-add-card-{id}`, `humidor-detail-add-{id}`, `humidor-detail-remove-cigar-{id}`, `humidor-detail-full` |
| Scoped классы | `humidor-detail-root`, `humidor-detail-grain`, `humidor-detail-enter`, `available-card-enter`, `line-clamp-2` |

## Profile.vue

Страница настроек аккаунта (`name: 'Profile'`, `meta.requiresAuth`): тот же каркас, что у **HumidorList** — `profile-root`, grain (тот же data-URI), градиент stone/amber. Подпись над заголовком — **«Аккаунт»**, `h1` с `id="profile-heading"`, контент `max-w-3xl`. Состояния: **загрузка** — два `Skeleton` (`profile-loading` / `profile-skeleton`), `aria-busy`; **ошибка** — `Message` + «Повторить загрузку» (`profile-retry`); при успехе — две панели `rounded-xl` (`profile-section-personal`, `profile-section-password`) с лёгкой stagger-анимацией `profile-panel-enter` и `prefers-reduced-motion`. Данные: `profileService.getMyProfile`, `updateProfile` (при `newToken` — `authService.setToken`), `changePassword` (429 — подсказка интервала). Поля профиля: `InputSwitch` публичности с `input-id` для подписи; превью — `PublicUserProfile`. Логи при сбое загрузки — только в `import.meta.env.DEV`.

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/Profile.vue` |
| Маршруты | Страница: `Profile`; публичный просмотр: `PublicUserProfile` (`params.username`) |
| `data-testid` | `profile`, `profile-loading`, `profile-skeleton`, `profile-error`, `profile-retry`, `profile-content`, `profile-section-personal`, `profile-username`, `profile-email`, `profile-public`, `profile-meta`, `profile-save`, `profile-preview`, `profile-section-password`, `profile-password-current`, `profile-password-new`, `profile-password-confirm`, `profile-password-error`, `profile-password-submit` |
| Scoped классы | `profile-root`, `profile-grain`, `profile-panel-enter` |

## PublicUserProfile.vue

Публичная страница `/u/:username` (`name: 'PublicUserProfile'`): каркас как у **HumidorList** — `public-profile-root`, тот же grain, контент **`max-w-5xl`**. Данные: `profileService.getPublicProfile(username)`; при смене `params.username` — перезагрузка через `watch`. Загрузка: сетка скелетонов (`public-profile-loading`, `public-profile-skeleton`); ошибка — `Message` + «Повторить загрузку» и «На главную» (`public-profile-retry`, `public-profile-home`); лог в `console` только в `DEV`. Заголовок: подпись **«Публичный профиль»**, `h1` с `id="public-profile-heading"` (= username), даты регистрации / активности. Блок **Хьюмидоры**: пусто — dashed-плашка (`public-profile-empty`); иначе сетка `article` как у `HumidorList`: оверлей `router-link` на **`PublicHumidorDetail`** (`username`, `humidorId` строка), контент `pointer-events-none`, `Badge` влажности через `humidorService.getHumiditySeverity`, `v-memo`, анимация `public-humidor-card-enter`, футер-подсказка «Смотреть состав».

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/PublicUserProfile.vue` |
| Маршруты | Страница: `PublicUserProfile`; деталь публичного хьюмидора: `PublicHumidorDetail`; домой: `Home` |
| `data-testid` | `public-profile`, `public-profile-loading`, `public-profile-skeleton`, `public-profile-error`, `public-profile-retry`, `public-profile-home`, `public-profile-content`, `public-profile-empty`, `public-profile-humidors`, `public-profile-humidor-{id}` |
| Scoped классы | `public-profile-root`, `public-profile-grain`, `public-profile-enter`, `public-humidor-card-enter`, `line-clamp-2/3` |

## PublicHumidorDetail.vue

Публичная деталь хьюмидора (`name: 'PublicHumidorDetail'`, `params.username`, `params.humidorId`): каркас как у **HumidorList** / **HumidorDetail** — `public-humidor-detail-root`, тот же grain, контент **`max-w-7xl`**. Данные: `profileService.getPublicHumidor(username, humidorId)`; при смене параметров маршрута — `watch` + `load()`. Загрузка: скелетоны (`public-humidor-detail-loading`, первый блок с `public-humidor-detail-skeleton`); ошибка — `Message` + «Повторить загрузку» и «К профилю»; контент — `public-humidor-detail-content`, анимация `public-humidor-detail-enter`, `prefers-reduced-motion`. Заголовок: подпись **«Публичный просмотр · {username}»**, `h1` с `id="public-humidor-detail-heading"`; кнопка «К профилю» → `PublicUserProfile`. Статистика: три панели без Prime `Card` — вместимость + `ProgressBar` (процент ограничен 100%), заглушка температуры («не публикуется»), влажность `Badge` + `humidorService.getHumiditySeverity` или «Не указана». Таблица сигар: `DataTable` (`public-humidor-detail-table`), крепость — `strengthOptions` из `utils/cigarOptions.ts`, пустое состояние — `public-humidor-detail-table-empty`. Логи — только в `import.meta.env.DEV`.

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/PublicHumidorDetail.vue` |
| Маршруты | Профиль владельца: `PublicUserProfile` (`params.username`) |
| `data-testid` | `public-humidor-detail`, `public-humidor-detail-loading`, `public-humidor-detail-skeleton`, `public-humidor-detail-error`, `public-humidor-detail-retry`, `public-humidor-detail-profile`, `public-humidor-detail-to-profile`, `public-humidor-detail-content`, `public-humidor-detail-stats`, `public-humidor-detail-table`, `public-humidor-detail-table-empty` |
| Scoped классы | `public-humidor-detail-root`, `public-humidor-detail-grain`, `public-humidor-detail-enter` |

## ReviewList.vue

Публичный список `/reviews` (`name: 'ReviewList'`): оболочка `review-list-root` + grain, подпись **«Обзоры»**, `h1` с `id="review-list-heading"`. Данные: `reviewService.getReviews({ pageSize: 100 })`, тип `Review` из `reviewService`; фильтрация и сортировка **на клиенте**. Auth: `useAuth()` — кнопка «Написать обзор» и CTA в пустом состоянии только для авторизованных. Фильтры в панели `rounded-2xl` (`review-list-filters`): бренд (`Select` с `filter`, опции из множества `cigarBrand` ответа), минимальный рейтинг (1–10), сортировка (дата / оценка). Состояния: скелетоны в сетке `lg:grid-cols-2` (карточки выше, чем у хьюмидоров); ошибка — `Message` + «Повторить загрузку»; если ответ пустой — `review-list-empty`; если после фильтров ничего не осталось — `review-list-filter-empty` + «Сбросить фильтры». Карточки: `article`, оверлей `router-link` на `ReviewDetail`, превью base64 как `data:image/jpeg;base64,`, без фото — градиент-заглушка; в футере кнопка «Читать полностью» с `@click.stop`; `v-memo`, входная анимация, `loading="lazy"` у изображений.

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/ReviewList.vue` |
| Маршруты | Список: `ReviewList`; создание: `ReviewCreate` (`requiresAuth`); деталь: `ReviewDetail` (`params.id`) |
| `data-testid` | `review-list`, `review-list-create`, `review-list-loading`, `review-list-skeleton`, `review-list-error`, `review-list-retry`, `review-list-empty`, `review-list-empty-create`, `review-list-filters`, `review-list-filter-brand`, `review-list-filter-rating`, `review-list-filter-sort`, `review-list-filter-empty`, `review-list-filter-reset`, `review-list-grid`, `review-card-{id}`, `review-open-{id}` |
| Scoped классы | `review-list-root`, `review-list-grain`, `review-card-enter`, `line-clamp-2/3` |

## ReviewDetail.vue

Страница обзора (`name: 'ReviewDetail'`, `params.id`): оболочка как у **ReviewList** — `review-detail-root`, тот же grain, контент **`max-w-4xl`**. Состояния: скелетон (`review-detail-loading`, `review-detail-skeleton`), ошибка — `Message` + «Повторить загрузку» и «К списку обзоров»; контент — `review-detail-content`, анимация `review-detail-enter`, `prefers-reduced-motion`. Навигация текстовая: **Главная** → **Обзоры** → заголовок; без Prime `Breadcrumb`. Заголовок: подпись **«Обзоры»**, `h1` с `id="review-detail-heading"`, блок автора и **Tag** оценки; баннер сигары (`review-detail-cigar-banner`). Для владельца (`authService.state.user.id` === `review.userId`) — правка (`ReviewEdit`) и удаление с `useConfirm` + `reviewService.deleteReview` → `ReviewList`. Галерея: `Galleria`, превью `data:image/jpeg;base64,`, у `img` `loading="lazy"`, `decoding="async"`, заданные `width`/`height`. Секции характеристик / дегустации и текст обзора — панели `rounded-2xl` без `Card`; текст через **`v-html`** после **`DOMPurify.sanitize`** (разбивка по строкам в `<p>` как раньше). Футер: `ReviewList`, переход к **`CigarDetail`** (`cigarId`). Логи при сбоях — только в **`import.meta.env.DEV`**.

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/ReviewDetail.vue` |
| Данные | `reviewService.getReview(id)`, `deleteReview` |
| Маршруты | Список: `ReviewList`; правка: `ReviewEdit`; сигара: `CigarDetail` (`id` строка); главная: `Home` |
| `data-testid` | `review-detail`, `review-detail-loading`, `review-detail-skeleton`, `review-detail-error`, `review-detail-retry`, `review-detail-back-list`, `review-detail-content`, `review-detail-edit`, `review-detail-delete`, `review-detail-cigar-banner`, `review-detail-gallery`, `review-detail-body`, `review-detail-back`, `review-detail-cigar` |
| Scoped классы | `review-detail-root`, `review-detail-grain`, `review-detail-enter`, `line-clamp-2`, `:deep(.review-detail-prose …)` |

## HumidorForm.vue

Та же оболочка, что у списка: `humidor-form-root`, grain, подпись «Коллекция», заголовок с `id="humidor-form-heading"`, панель формы `rounded-2xl` stone/amber. Режимы: маршрут `HumidorForm` (создание) и `HumidorEdit` (`isEdit` через `route.name`). Ошибка загрузки при редактировании: `Message` + «Повторить загрузку» (`humidor-form-retry`); ошибка сохранения — отдельный блок `saveError` над полями. `data-testid`: `humidor-form`, `humidor-form-back`, `humidor-form-loading`, `humidor-form-error`, `humidor-form-retry`, `humidor-form-error-back`, `humidor-form-save-error`, `humidor-form-fields`, `humidor-form-name`, …, `humidor-form-submit`, `humidor-form-cancel`. После успеха — `router.push({ name: 'HumidorList' })`.

## CigarList.vue

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/CigarList.vue` |
| Данные | `cigarService.getCigars()`, тип `Cigar` из `cigarService` |
| Маршруты | Список: `CigarList`; добавление: `CigarNew`; деталь: `CigarDetail` (`id` как строка в `params`); правка: `CigarEdit` |
| Карточка | **Карусель** (`Carousel`, PrimeVue, автоимпорт) вверху z-20, фикс. высота ~192px; клик по слайду/заглушке ведёт в деталь через `router.push`. Ниже — `router-link` на текстовый блок (рейтинг, цена, «В хьюмидоре»). Футер: правка + удаление (`cigarService.deleteCigar`, confirm + toast) |
| Изображения | `imageData` в API: строка base64 или массив байт — через `arrayBufferToBase64` в `utils/imageUtils.ts`, префикс `data:image/jpeg;base64,` |
| Рейтинг | Шкала 0–10; 5 звёзд: звезда `i` активна, если `(rating ?? 0) >= i * 2 - 1` |
| Производительность | `v-memo` через `memoKey(cigar)` (идентификаторы и метаданные без полного base64 в ключе); у `<img>` `loading="lazy"`, `decoding="async"`, заданные `width`/`height` под контейнер |
| `data-testid` | `cigar-list`, `cigar-list-loading`, `cigar-list-skeleton`, `cigar-list-error`, `cigar-list-retry`, `cigar-list-empty`, `cigar-list-empty-add`, `cigar-list-add`, `cigar-list-grid`, `cigar-card-{id}`, `cigar-edit-{id}`, `cigar-delete-{id}` |
| Scoped классы | `cigar-list-root`, `cigar-list-grain`, `cigar-card-enter`, `:deep` для мин. размера стрелок карусели |

## CigarForm.vue

Та же оболочка, что у списков и `HumidorForm`: `cigar-form-root`, grain, подпись «Коллекция», заголовок с `id="cigar-form-heading"`, контейнер формы `max-w-4xl` (шире, чем хьюмидор — больше секций). Режимы: `CigarNew` (создание) и `CigarEdit` (ред. при наличии `route.params.id`). Состояния: **`pageLoading`** — только при открытии существующей сигары (`cigarService.getCigar`); **`saving`** — отправка формы; **`loadError`** — блок с `Message` + «Повторить загрузку» (`cigar-form-retry`) и «К списку сигар»; **`saveError`** — `Message` над полями (`cigar-form-save-error`).

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/CigarForm.vue` |
| Данные | Бренды: `cigarService.getBrands()`; хьюмидоры: `humidorService.getHumidors()`; при редактировании — `getCigar(id)`; поиск по базе — `getCigarBasesPaginated` + debounce 300 ms, кэш ключей запроса |
| Маршруты | Создание: `CigarNew`; правка: `CigarEdit`; «К списку» / отмена / успех с коллекцией → `CigarList`; сценарий «только база» (создание без флага коллекции) → `CigarBases` |
| UI | Секции во внутренних панелях `rounded-xl` stone/amber; `AutoComplete` с группировкой по бренду (`optionGroupLabel` / `optionGroupChildren`); чекбокс «в мою коллекцию» только при создании; хьюмидор `Dropdown` заблокирован, если не edit и не отмечен коллекционный флажок |
| `data-testid` | `cigar-form`, `cigar-form-back`, `cigar-form-loading`, `cigar-form-error`, `cigar-form-retry`, `cigar-form-error-back`, `cigar-form-save-error`, `cigar-form-fields`, `cigar-form-name`, `cigar-form-country`, `cigar-form-price`, `cigar-form-wrapper`, `cigar-form-binder`, `cigar-form-filler`, `cigar-form-size`, `cigar-form-strength`, `cigar-form-rating`, `cigar-form-description`, `cigar-form-image-url`, `cigar-form-add-to-collection`, `cigar-form-humidor`, `cigar-form-cancel`, `cigar-form-submit` |
| Scoped классы | `cigar-form-root`, `cigar-form-grain`, `cigar-form-enter`, `prefers-reduced-motion` |

## CigarDetail.vue

Та же оболочка страницы, что у списков: `cigar-detail-root`, grain, подпись «Коллекция», `h1` с `id="cigar-detail-heading"`. Контент — сетка `lg:grid-cols-3` (фото слева, блоки справа), панели `rounded-2xl` stone/amber без Prime `Card`.

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/CigarDetail.vue` |
| Данные | `cigarService.getCigar(id)`; при `cigar.humidorId` — `humidorService.getHumidor` (при ошибке загрузки хьюмидора показ fallback с переходом по `humidorId`) |
| Маршруты | Назад: `CigarList`; правка: `CigarEdit` (`params.id` — строка); хьюмидор: `HumidorDetail`; после удаления — `CigarList` |
| Действия | «К списку», «Редактировать», «Удалить» (`useConfirm`: `rejectLabel` / `acceptLabel`, как в `HumidorList`) |
| Изображение | `computed` `primaryImageSrc`: поля `images[0].imageData` или `.data`; префикс `data:image/jpeg;base64,` если строка без `data:`; у `<img>` `loading="lazy"`, `decoding="async"` |
| Хранение | Если есть `humidor` — имя, вместимость `currentCount`/`capacity`, влажность `Badge` + `humidorService.getHumiditySeverity`, описание; если только `humidorId` без объекта — предупреждение и кнопка на деталь хьюмидора; если нет привязки — пустое состояние (как у списков, dashed border) + CTA в редактирование |
| Прочее | Подписи крепости через `strengthOptions` из `utils/cigarOptions.ts`; отдельный запрос брендов на странице не выполняется |
| `data-testid` | `cigar-detail`, `cigar-detail-loading`, `cigar-detail-error`, `cigar-detail-retry`, `cigar-detail-back`, `cigar-detail-edit`, `cigar-detail-delete`, `cigar-detail-content`, `cigar-detail-no-image`, `cigar-detail-storage`, `cigar-detail-open-humidor`, `cigar-detail-humidor-fallback`, `cigar-detail-no-humidor`, `cigar-detail-add-humidor` |
| Scoped классы | `cigar-detail-root`, `cigar-detail-grain`, `cigar-detail-enter`, анимация входа с `prefers-reduced-motion` |

## CigarBases.vue

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/CigarBases.vue` |
| Данные | `cigarService.getCigarBasesPaginated`, тип `CigarBase`; бренды для фильтра — `getBrands()` → `brandOptions` |
| Маршруты | Страница: `CigarBases`; в коллекцию / новая сигара: `CigarNew`; отзыв: `ReviewCreate` (query: `cigarBaseId`, `brandName`, `cigarName`) |
| Фильтры | Поиск с debounce 300 ms; бренд и крепость — `Dropdown`; без `watch(filters)` (избегаем двойных запросов на каждый символ) |
| Десктоп | `DataTable` lazy, `virtualScrollerOptions`: `itemSize`, `scrollHeight`; действия в строке: просмотр (диалог), отзыв, в коллекцию, похожая |
| Мобилка | Скелетоны при загрузке (`lg:hidden`), сетка `article` с `v-memo`, футер из четырёх icon-кнопок, `Paginator` снизу |
| Диалоги | `CigarDetailDialog`, `CigarBaseEditDialog` (как раньше) |
| `data-testid` | `cigar-bases`, `cigar-bases-filters`, `cigar-bases-search`, `cigar-bases-filter-brand`, `cigar-bases-filter-strength`, `cigar-bases-loading`, `cigar-bases-skeleton`, `cigar-bases-error`, `cigar-bases-retry`, `cigar-bases-empty`, `cigar-bases-empty-reset`, `cigar-bases-add`, `cigar-bases-table`, `cigar-bases-row-*-{id}`, `cigar-bases-mobile-grid`, `cigar-bases-mobile-summary`, `cigar-base-card-{id}`, `cigar-base-*-{id}`, `cigar-bases-paginator` |
| Scoped классы | `cigar-bases-root`, `cigar-bases-grain`, `cigar-base-card-enter` |

## Brands.vue

Страница админа (`name: 'Brands'`, `meta.requiresAdmin`): оболочка `brands-root` + grain, подпись **«Администрирование»**, `id="brands-heading"`. Данные: `cigarService.getBrands()`; CRUD через `createBrand`, `updateBrand`, `deleteBrand`; фильтрация клиентская (`applyFilters`) — поиск, страна, статус модерации. Состояния: скелетоны при загрузке, `Message` + «Повторить» при `loadError`, пустой список (dashed) если брендов нет; при наличии данных — панель с фильтрами и `DataTable` (пагинация 10/25/50, `#empty` + «Сбросить фильтры» если фильтры дали ноль строк). Статусы в таблице — `Tag` (`success` / `warn`). Диалоги формы и деталей — адаптивная ширина, футеры с `min-h-12`, подтверждение удаления как у `HumidorList`.

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/Brands.vue` |
| `data-testid` | `brands-page`, `brands-add`, `brands-loading`, `brands-skeleton-filters`, `brands-skeleton-row`, `brands-error`, `brands-retry`, `brands-empty`, `brands-empty-add`, `brands-content`, `brands-filter-search`, `brands-filter-country`, `brands-filter-status`, `brands-table`, `brands-filter-empty`, `brands-filter-reset`, `brands-view-{id}`, `brands-edit-{id}`, `brands-delete-{id}`, `brands-dialog-form`, `brands-dialog-cancel`, `brands-dialog-submit`, `brands-dialog-detail`, `brands-detail-close`, `brands-detail-edit` |
| Scoped классы | `brands-root`, `brands-grain`, `brands-panel-enter`, `line-clamp-2` |

## AdminUsers.vue

Страница админа (`name: 'AdminUsers'`, `meta.requiresAdmin`): оболочка `admin-users-root` + grain, подпись **«Администрирование»**, `id="admin-users-heading"`. Данные: `adminApi.fetchAdminUsers` (страница, размер, поиск); сохранение роли — `updateUserRole`, при смене **своей** роли — подтверждение `useConfirm` с `rejectLabel` / `acceptLabel`, обновление JWT через `authService.setToken`; при потере роли Admin — редирект на `Home`. Флаг `hasEverLoaded`: первый запрос показывает **скелетон**; после успешной загрузки при пагинации таблица с `:loading`; при ошибке до первого успеха — `loadError` + «Повторить» (при последующих сбоях только toast). Пусто: `totalCount === 0` — блок с иконкой; при активном поиске — «Сбросить поиск». `Paginator` при `totalCount > 0`.

| Пункт | Значение |
|-------|----------|
| Файл | `src/views/AdminUsers.vue` |
| `data-testid` | `admin-users-page`, `admin-users-loading`, `admin-users-skeleton-search`, `admin-users-skeleton-row`, `admin-users-error`, `admin-users-retry`, `admin-users-content`, `admin-users-search`, `admin-users-search-submit`, `admin-users-empty`, `admin-users-clear-search`, `admin-users-table`, `admin-users-role-{id}`, `admin-users-save-{id}`, `admin-users-paginator` |
| Scoped классы | `admin-users-root`, `admin-users-grain`, `admin-users-panel-enter` |

## Автотесты (Playwright и т.п.)

Стабильные селекторы — префиксы `app*`, `home*`, `login*`, `humidor-list*`, `humidor-detail*`, `profile*`, `public-profile*`, `public-humidor-detail*`, `humidor-form*`, `cigar-list*`, `cigar-form*`, `cigar-detail*`, `cigar-bases*`, `review-list*`, `review-detail*`, `review-form*`, `review-card-{id}`, `brands-*`, `admin-users-*` и карточек `*-card-{id}` / `cigar-base-card-{id}`; ждать гидратации (`networkidle` / появление `app-main`, `home-features`, `login-form`, `*-grid`, `humidor-detail-content`, `public-profile-content`, `public-profile-humidors`, `public-humidor-detail-content`, `review-list-grid`, `review-detail-content`, `review-form-fields`, `profile-content`, `brands-table`, `admin-users-table`, `*-table`, `cigar-detail-content`, `cigar-form-fields` или `*-empty`).

## Связанные файлы

- Шрифты Onest: `public/fonts/onest/` + `onest.css` (пути `url('/fonts/onest/...')`).
- Корень SPA и навигация: `src/App.vue` — см. раздел [App.vue](#appvue) и [code-map.md — App.vue (оболочка)](./code-map.md#appvue-оболочка).
- Подтверждения и тосты: `App.vue` подключает `ConfirmDialog` и `Toast`; на страницах используются `useConfirm` / `useToast`.
