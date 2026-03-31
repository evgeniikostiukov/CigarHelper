/**
 * TDD-тест контракта сгенерированных типов API.
 *
 * RED: файл не компилируется пока api.generated.ts не создан (import не найден).
 * GREEN: после `npm run generate:api` typecheck и тесты проходят.
 *
 * Цель — убедиться, что сгенерированные схемы содержат ожидаемые поля.
 * Если поле переименовали в DTO на бэке — typecheck здесь упадёт раньше, чем
 * сломается vue-компонент.
 *
 * Обновлять при: переименовании DTO-классов, добавлении/удалении полей,
 * смене nullable-семантики.
 */
import { describe, it, expectTypeOf } from 'vitest';
import type { components } from './api.generated';

// ── Вспомогательные алиасы из сгенерированных схем ────────────────────────
type CigarResponseDto = components['schemas']['CigarResponseDto'];
type CigarBaseDto = components['schemas']['CigarBaseDto'];
type BrandDto = components['schemas']['BrandDto'];
type DashboardSummaryDto = components['schemas']['DashboardSummaryDto'];
type BrandBreakdownItemDto = components['schemas']['BrandBreakdownItemDto'];
type RecentReviewDto = components['schemas']['RecentReviewDto'];
type HumidorDto = components['schemas']['HumidorDto'];

// ── CigarResponseDto ───────────────────────────────────────────────────────
describe('CigarResponseDto', () => {
  it('содержит числовые поля id и userId', () => {
    // Swashbuckle генерирует все поля как опциональные (нет [Required] на id/userId)
    expectTypeOf<CigarResponseDto['id']>().toEqualTypeOf<number | undefined>();
    expectTypeOf<CigarResponseDto['userId']>().toEqualTypeOf<number | undefined>();
  });

  it('содержит строковое поле name (nullable)', () => {
    expectTypeOf<CigarResponseDto['name']>().toEqualTypeOf<string | null | undefined>();
  });

  it('содержит вложенный объект brand типа BrandDto', () => {
    expectTypeOf<CigarResponseDto['brand']>().toEqualTypeOf<BrandDto | undefined>();
  });

  it('smokedAt — nullable (сигара может быть не выкурена)', () => {
    expectTypeOf<CigarResponseDto['smokedAt']>().toEqualTypeOf<string | null | undefined>();
  });

  it('isSmoked — readonly boolean computed-флаг', () => {
    expectTypeOf<CigarResponseDto['isSmoked']>().toEqualTypeOf<boolean | undefined>();
  });

  it('содержит lifecycle-поля purchasedAt и lastTouchedAt', () => {
    expectTypeOf<CigarResponseDto['purchasedAt']>().toEqualTypeOf<string | undefined>();
    expectTypeOf<CigarResponseDto['lastTouchedAt']>().toEqualTypeOf<string | undefined>();
  });
});

// ── CigarBaseDto ───────────────────────────────────────────────────────────
describe('CigarBaseDto', () => {
  it('содержит id и name', () => {
    expectTypeOf<CigarBaseDto['id']>().toEqualTypeOf<number | undefined>();
    // Swashbuckle без [Required] генерирует string как string | null | undefined
    expectTypeOf<CigarBaseDto['name']>().toEqualTypeOf<string | null | undefined>();
  });

  it('содержит вложенный brand', () => {
    expectTypeOf<CigarBaseDto['brand']>().toEqualTypeOf<BrandDto | undefined>();
  });
});

// ── BrandDto ───────────────────────────────────────────────────────────────
describe('BrandDto', () => {
  it('содержит id, name и флаг isModerated', () => {
    expectTypeOf<BrandDto['id']>().toEqualTypeOf<number | undefined>();
    expectTypeOf<BrandDto['name']>().toEqualTypeOf<string | null | undefined>();
    expectTypeOf<BrandDto['isModerated']>().toEqualTypeOf<boolean | undefined>();
  });
});

// ── DashboardSummaryDto ────────────────────────────────────────────────────
describe('DashboardSummaryDto', () => {
  it('содержит числовые счётчики коллекции', () => {
    expectTypeOf<DashboardSummaryDto['totalHumidors']>().toEqualTypeOf<number | undefined>();
    expectTypeOf<DashboardSummaryDto['totalCigars']>().toEqualTypeOf<number | undefined>();
    expectTypeOf<DashboardSummaryDto['totalCapacity']>().toEqualTypeOf<number | undefined>();
    expectTypeOf<DashboardSummaryDto['averageDaysToSmoke']>().toEqualTypeOf<number | undefined>();
  });

  it('содержит массив brandBreakdown (nullable)', () => {
    expectTypeOf<DashboardSummaryDto['brandBreakdown']>().toEqualTypeOf<BrandBreakdownItemDto[] | null | undefined>();
  });

  it('содержит массив recentReviews (nullable)', () => {
    expectTypeOf<DashboardSummaryDto['recentReviews']>().toEqualTypeOf<RecentReviewDto[] | null | undefined>();
  });
});

// ── BrandBreakdownItemDto ──────────────────────────────────────────────────
describe('BrandBreakdownItemDto', () => {
  it('brandName — non-optional (есть [Required])', () => {
    // [Required] + [MaxLength] → Swashbuckle выставляет поле non-optional
    expectTypeOf<BrandBreakdownItemDto['brandName']>().toEqualTypeOf<string>();
  });

  it('brandId и cigarCount опциональны', () => {
    expectTypeOf<BrandBreakdownItemDto['brandId']>().toEqualTypeOf<number | undefined>();
    expectTypeOf<BrandBreakdownItemDto['cigarCount']>().toEqualTypeOf<number | undefined>();
  });
});

// ── RecentReviewDto ────────────────────────────────────────────────────────
describe('RecentReviewDto', () => {
  it('title и cigarName — non-optional (есть [Required])', () => {
    expectTypeOf<RecentReviewDto['title']>().toEqualTypeOf<string>();
    expectTypeOf<RecentReviewDto['cigarName']>().toEqualTypeOf<string>();
  });

  it('id и rating — опциональны', () => {
    expectTypeOf<RecentReviewDto['id']>().toEqualTypeOf<number | undefined>();
    expectTypeOf<RecentReviewDto['rating']>().toEqualTypeOf<number | undefined>();
  });
});

// ── HumidorDto ─────────────────────────────────────────────────────────────
describe('HumidorDto', () => {
  it('содержит id, name, capacity', () => {
    expectTypeOf<HumidorDto['id']>().toEqualTypeOf<number | undefined>();
    expectTypeOf<HumidorDto['name']>().toEqualTypeOf<string | null | undefined>();
    expectTypeOf<HumidorDto['capacity']>().toEqualTypeOf<number | undefined>();
  });
});
