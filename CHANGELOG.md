# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased](https://github.com/evgeniikostiukov/CigarHelper/compare/v2.0.0...HEAD)

## [2.0.0](https://github.com/evgeniikostiukov/CigarHelper/compare/v1.0.0...v2.0.0) - 2026-04-13

### Breaking changes

- Каталог (API и БД): единое поле размера заменено на длину в миллиметрах и диаметр (кольцо); после миграции колонка `Size` удалена. Клиентам и интеграциям нужно перейти на новый контракт.

### Added

- Регистрация: подтверждение возраста 18+.
- Каталог: добавление позиций в коллекцию для немодерированных баз; отправка новых записей каталога пользователями; полный набор полей в диалоге новой базы; изображения базы файлом и по URL (для JWT-пользователей); подсказка про общее фото каталога.
- Справочник брендов: отображение сеткой карточек с пагинацией; открытие карточки по клику.

### Changed

- Веб: экран входа (переключение режима, подсказка по паролю); списки хьюмидоров, коллекции и базы каталога — переход в детали по клику на карточку (карусель в коллекции не открывает карточку при листании).
- Бренды: статус модерации и фильтр по статусу только для ролей администратора и модератора.
- Онбординг и главная: уточнены формулировки (вместимость опциональна, текст про несколько хьюмидоров); мелкая правка отступа подписи в шапке.

### Fixed

- API: разрешено добавление в коллекцию для немодерированных базовых сигар.

### Commits

- chore(repo): release 2.0.0 [`a92700a`](https://github.com/evgeniikostiukov/CigarHelper/commit/a92700ab35a1e44ca8dbe3be309b7cfdf992a5f3)
- chore(repo): set root package version to match tag v1.0.0 [`ad1f3b2`](https://github.com/evgeniikostiukov/CigarHelper/commit/ad1f3b2696fa3d6e033ec738ed9a40ad778982b0)
- chore(web): nav label spacing, home copy, onboarding capacity UI [`32efbc7`](https://github.com/evgeniikostiukov/CigarHelper/commit/32efbc7)
- feat(web): open details from card click on humidor and cigar lists [`4fde446`](https://github.com/evgeniikostiukov/CigarHelper/commit/4fde446)
- feat(web): brands grid cards and staff-only status filters [`e273933`](https://github.com/evgeniikostiukov/CigarHelper/commit/e273933)
- feat(cigar-base): store length mm and ring gauge separately [`b6caa54`](https://github.com/evgeniikostiukov/CigarHelper/commit/b6caa54)
- feat(auth): require 18+ confirmation on registration [`f629cd5`](https://github.com/evgeniikostiukov/CigarHelper/commit/f629cd5)
- chore(cursor): add releases rule and link in workflow [`b2a93c0`](https://github.com/evgeniikostiukov/CigarHelper/commit/b2a93c0)
- style(web): login mode toggle and registration password hint [`7e6a9d0`](https://github.com/evgeniikostiukov/CigarHelper/commit/7e6a9d0)
- feat(web): shared catalog photo hint for cigar base forms [`984b44c`](https://github.com/evgeniikostiukov/CigarHelper/commit/984b44c)
- feat(api,web): create cigar base images by file and url for all jwt users [`2d80032`](https://github.com/evgeniikostiukov/CigarHelper/commit/2d80032)
- chore(repo): add auto-changelog and initial CHANGELOG [`8c133d2`](https://github.com/evgeniikostiukov/CigarHelper/commit/8c133d2)
- feat(cigar-form): full cigar base fields in new catalog dialog [`769b29f`](https://github.com/evgeniikostiukov/CigarHelper/commit/769b29f)
- refactor(cigar-form): add catalog entry from autocomplete panel [`ac96a5a`](https://github.com/evgeniikostiukov/CigarHelper/commit/ac96a5a)
- feat(cigar-form): let users submit unmoderated catalog entries [`0efa452`](https://github.com/evgeniikostiukov/CigarHelper/commit/0efa452)
- fix(api): allow adding collection items for unmoderated cigar bases [`9faf5e6`](https://github.com/evgeniikostiukov/CigarHelper/commit/9faf5e6)

## [v1](https://github.com/evgeniikostiukov/CigarHelper/compare/v1.0.0...v1) - 2026-04-10

## v1.0.0 - 2026-04-10

### Merged

- The first release [`#2`](https://github.com/evgeniikostiukov/CigarHelper/pull/2)

### Commits

- initial commit [`e477712`](https://github.com/evgeniikostiukov/CigarHelper/commit/e477712bc8c20028c403d1041650369cf4078d44)
- scrapper [`7fffc74`](https://github.com/evgeniikostiukov/CigarHelper/commit/7fffc74da58bb364c3396cd2451ed5fdfb8cc76a)
- scrapper [`1eb694b`](https://github.com/evgeniikostiukov/CigarHelper/commit/1eb694bb03ca60491109cc84f01ad0dcef897501)
- fix(web): доработать редактирование обзоров (маршрут, список, сигара) [`8fe6883`](https://github.com/evgeniikostiukov/CigarHelper/commit/8fe688347012c4cd36255cb9aa12d9ccfe962a1d)
- feat(web): polish review editing flow [`d664e8d`](https://github.com/evgeniikostiukov/CigarHelper/commit/d664e8dff07666ca02b17904db7edfde9396655d)
- fix(web): автор обзора — getAuthUserId для JWT id [`44c7414`](https://github.com/evgeniikostiukov/CigarHelper/commit/44c74147d35bea1c09733cdeaa9f2987128bde76)
- fix(deploy): resolve compose root without repo layout [`e9adab9`](https://github.com/evgeniikostiukov/CigarHelper/commit/e9adab90e001e7f0dd95c17759363d8e2ce1f487)
