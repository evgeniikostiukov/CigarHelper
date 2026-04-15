# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased](https://github.com/evgeniikostiukov/CigarHelper/compare/v2.1.0...HEAD)

## [v2.1.0](https://github.com/evgeniikostiukov/CigarHelper/compare/v2.0.0...v2.1.0) - 2026-04-15

### Commits

- feat(reviews): moderated comments on reviews [`01949a6`](https://github.com/evgeniikostiukov/CigarHelper/commit/01949a6dcf3aa07248cc3f34a85fc392f8cebe57)
- chore(web): синхронизация OpenAPI и правки онбординга [`1855891`](https://github.com/evgeniikostiukov/CigarHelper/commit/18558916ab859a3cd4689a2408373371c11e90d7)
- feat(api): мягкое удаление обзоров и восстановление staff [`cc5a89c`](https://github.com/evgeniikostiukov/CigarHelper/commit/cc5a89cde9427798180940eed3469713e1d0ed9b)
- feat(profile): author links to public profile when visible [`15b99dd`](https://github.com/evgeniikostiukov/CigarHelper/commit/15b99dd93677351313948dffabd4dec17503e2d9)
- feat(web): админка удалённых обзоров и DTO профиля автора [`c038c55`](https://github.com/evgeniikostiukov/CigarHelper/commit/c038c55e112b3841c5767450c2a50a75f54ec3d5)
- fix(web): корректный вывод HTML/текста обзоров [`afbbebe`](https://github.com/evgeniikostiukov/CigarHelper/commit/afbbebea57bb667a7d614d3edf7614dbc4701e9c)
- feat(dashboard): author row for recent reviews [`4b64080`](https://github.com/evgeniikostiukov/CigarHelper/commit/4b64080ffe6b20fbc54063ea6bbf47c4b8f1a56d)
- docs(changelog): sync for v2.0.0 [`418f5d8`](https://github.com/evgeniikostiukov/CigarHelper/commit/418f5d873510183fac1ec9bbf3271d2380f324dc)
- fix(import,tests): resolve nullable and xUnit analyzer warnings [`ea5134c`](https://github.com/evgeniikostiukov/CigarHelper/commit/ea5134cbc6a42775bc459e0a8c72ac707ac0c970)
- fix(web): перенос длинных строк в тексте обзора [`744626b`](https://github.com/evgeniikostiukov/CigarHelper/commit/744626b194a5d4a9441f971807134dfe7910186e)
- chore(repo): release 2.1.0 [`790ae40`](https://github.com/evgeniikostiukov/CigarHelper/commit/790ae40c2c4b30a52f9dbe86a40354f528b9704c)
- docs(memory-bank): sync ReviewDetail routes and testids [`49d4128`](https://github.com/evgeniikostiukov/CigarHelper/commit/49d41280dd438654c94939f2f3af88c0777f1203)
- fix(api): apply EXIF orientation in thumbnail generation [`366bf31`](https://github.com/evgeniikostiukov/CigarHelper/commit/366bf31f48f9e55d9c9f2316be50bd41f3c90c76)

## [v2.0.0](https://github.com/evgeniikostiukov/CigarHelper/compare/v1...v2.0.0) - 2026-04-13

### Commits

- feat(cigar-base): store length mm and ring gauge separately [`b6caa54`](https://github.com/evgeniikostiukov/CigarHelper/commit/b6caa543a236e80b57a217eb95e48dfae068ca16)
- refactor(cigar-form): add catalog entry from autocomplete panel [`ac96a5a`](https://github.com/evgeniikostiukov/CigarHelper/commit/ac96a5a001c0101d4ce8b7a42e261c0b430853a6)
- feat(cigar-form): let users submit unmoderated catalog entries [`0efa452`](https://github.com/evgeniikostiukov/CigarHelper/commit/0efa4528aeb65c5f0b20f9f92183ede2eef2f829)
- feat(cigar-form): full cigar base fields in new catalog dialog [`769b29f`](https://github.com/evgeniikostiukov/CigarHelper/commit/769b29fbe5cce7b2c902c72e229f5652c492baaf)
- feat(web): brands grid cards and staff-only status filters [`e273933`](https://github.com/evgeniikostiukov/CigarHelper/commit/e2739339fdf1e21086cb35630175420b3653f6d1)
- chore(repo): add auto-changelog and initial CHANGELOG [`8c133d2`](https://github.com/evgeniikostiukov/CigarHelper/commit/8c133d20811f6725f54494a98fc31536ee55b427)
- feat(auth): require 18+ confirmation on registration [`f629cd5`](https://github.com/evgeniikostiukov/CigarHelper/commit/f629cd5d58dcf745a10e86f7416185b7db7e007b)
- feat(api,web): create cigar base images by file and url for all jwt users [`2d80032`](https://github.com/evgeniikostiukov/CigarHelper/commit/2d8003293a15cee82042d07621cc02a31d10643f)
- feat(web): open details from card click on humidor and cigar lists [`4fde446`](https://github.com/evgeniikostiukov/CigarHelper/commit/4fde44655a88be2b0ce146a11d118f08041247ff)
- chore(cursor): add releases rule and link in workflow [`b2a93c0`](https://github.com/evgeniikostiukov/CigarHelper/commit/b2a93c0cb6afd95314be642a18b7cc5d73a2bafe)
- fix(web): автор обзора — getAuthUserId для JWT id [`44c7414`](https://github.com/evgeniikostiukov/CigarHelper/commit/44c74147d35bea1c09733cdeaa9f2987128bde76)
- fix(api): allow adding collection items for unmoderated cigar bases [`9faf5e6`](https://github.com/evgeniikostiukov/CigarHelper/commit/9faf5e6b1b60b02a6d2400234d51731565e49606)
- feat(web): shared catalog photo hint for cigar base forms [`984b44c`](https://github.com/evgeniikostiukov/CigarHelper/commit/984b44c17e2f6b0e0a1b688f97736813c999199b)
- chore(web): nav label spacing, home copy, onboarding capacity UI [`32efbc7`](https://github.com/evgeniikostiukov/CigarHelper/commit/32efbc7c1bdd5bcadea4cb6097e6f15a95f7526b)
- style(web): login mode toggle and registration password hint [`7e6a9d0`](https://github.com/evgeniikostiukov/CigarHelper/commit/7e6a9d083225e7f34b1f1bad1dbca272bc7f4cd8)
- chore(repo): release 2.0.0 [`a92700a`](https://github.com/evgeniikostiukov/CigarHelper/commit/a92700ab35a1e44ca8dbe3be309b7cfdf992a5f3)
- chore(repo): set root package version to match tag v1.0.0 [`ad1f3b2`](https://github.com/evgeniikostiukov/CigarHelper/commit/ad1f3b2696fa3d6e033ec738ed9a40ad778982b0)

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
