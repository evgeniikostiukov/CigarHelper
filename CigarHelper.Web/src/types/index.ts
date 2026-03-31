/**
 * Публичный фасад типов, сгенерированных из OpenAPI-спецификации API.
 *
 * Прямой импорт из api.generated — нежелателен: имена могут меняться.
 * Весь код приложения должен импортировать из этого файла.
 *
 * Регенерация после изменений в API:
 *   1. npm run generate:spec   — скачивает свежий openapi.json (нужен запущенный API)
 *      или dotnet swagger tofile с env Development (см. docs/memory-bank/workflow.md)
 *   2. npm run generate:api    — перегенерирует api.generated.ts
 *   3. При TypeScript-ошибках — поправить алиасы ниже и тест api.types.test.ts
 */

// ── Сырой интерфейс сгенерированных схем (для сложных случаев) ────────────
export type { components, paths } from './api.generated';

// ── DTO ответов API ────────────────────────────────────────────────────────
export type ApiCigarResponse = import('./api.generated').components['schemas']['CigarResponseDto'];
export type ApiCigarBase = import('./api.generated').components['schemas']['CigarBaseDto'];
export type ApiBrand = import('./api.generated').components['schemas']['BrandDto'];
export type ApiHumidor = import('./api.generated').components['schemas']['HumidorDto'];
export type ApiHumidorResponse = import('./api.generated').components['schemas']['HumidorResponseDto'];
export type ApiHumidorDetail = import('./api.generated').components['schemas']['HumidorDetailResponseDto'];
export type ApiReview = import('./api.generated').components['schemas']['ReviewDto'];
export type ApiReviewListItem = import('./api.generated').components['schemas']['ReviewListItemDto'];
export type ApiCigarImage = import('./api.generated').components['schemas']['CigarImageDto'];

// ── Dashboard ──────────────────────────────────────────────────────────────
export type ApiDashboardSummary = import('./api.generated').components['schemas']['DashboardSummaryDto'];
export type ApiBrandBreakdownItem = import('./api.generated').components['schemas']['BrandBreakdownItemDto'];
export type ApiRecentReview = import('./api.generated').components['schemas']['RecentReviewDto'];
export type ApiCigarTimelinePoint = import('./api.generated').components['schemas']['CigarTimelinePointDto'];
export type ApiStaleCigarReminder = import('./api.generated').components['schemas']['StaleCigarReminderDto'];

// ── Search ─────────────────────────────────────────────────────────────────
export type ApiGlobalSearchResult = import('./api.generated').components['schemas']['GlobalSearchResultDto'];
export type ApiSearchCigar = import('./api.generated').components['schemas']['SearchCigarDto'];
export type ApiSearchCigarBase = import('./api.generated').components['schemas']['SearchCigarBaseDto'];
export type ApiSearchHumidor = import('./api.generated').components['schemas']['SearchHumidorDto'];
export type ApiSearchBrand = import('./api.generated').components['schemas']['SearchBrandDto'];

// ── Auth ───────────────────────────────────────────────────────────────────
export type ApiAuthResponse = import('./api.generated').components['schemas']['AuthResponse'];
export type ApiMyProfile = import('./api.generated').components['schemas']['MyProfileDto'];
export type ApiPublicProfile = import('./api.generated').components['schemas']['PublicProfileDto'];

// ── Request-тела (входящие DTO) ────────────────────────────────────────────
export type ApiMarkCigarSmokedRequest = import('./api.generated').components['schemas']['MarkCigarSmokedRequest'];
export type ApiCreateCigarBaseRequest = import('./api.generated').components['schemas']['CreateCigarBaseRequest'];
export type ApiUserCigarUpdateRequest = import('./api.generated').components['schemas']['UserCigarUpdateRequest'];
export type ApiCreateReviewRequest = import('./api.generated').components['schemas']['CreateReviewRequest'];
export type ApiUpdateReviewRequest = import('./api.generated').components['schemas']['UpdateReviewRequest'];
export type ApiHumidorCreateDto = import('./api.generated').components['schemas']['HumidorCreateDto'];
export type ApiHumidorUpdateDto = import('./api.generated').components['schemas']['HumidorUpdateDto'];
export type ApiCreateBrandRequest = import('./api.generated').components['schemas']['CreateBrandRequest'];
export type ApiUpdateBrandRequest = import('./api.generated').components['schemas']['UpdateBrandRequest'];

// ── Pagination ─────────────────────────────────────────────────────────────
export type ApiCigarBasePaginatedResult =
  import('./api.generated').components['schemas']['CigarBaseDtoPaginatedResult'];
