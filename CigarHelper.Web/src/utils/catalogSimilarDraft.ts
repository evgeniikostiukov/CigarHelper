import type { CigarBase } from '@/services/cigarService';

/** Временное хранилище при переходе «Создать похожую» → форма новой карточки справочника (без привязки к существующей записи в коллекции). */
export const CATALOG_SIMILAR_DRAFT_STORAGE_KEY = 'cigarHelper:newCatalogSimilarDraft';

/** Поля новой базовой сигары, копируемые с выбранной карточки (без id и без изображений). */
export interface CatalogSimilarDraftSnapshot {
  name: string;
  brandId: number;
  country?: string | null;
  strength?: string | null;
  lengthMm?: number | null;
  diameter?: number | null;
  wrapper?: string | null;
  binder?: string | null;
  filler?: string | null;
  description?: string | null;
}

export function buildCatalogSimilarDraftSnapshot(c: CigarBase): CatalogSimilarDraftSnapshot {
  return {
    name: c.name,
    brandId: c.brand.id,
    country: c.country,
    strength: c.strength,
    lengthMm: c.lengthMm ?? null,
    diameter: c.diameter ?? null,
    wrapper: c.wrapper,
    binder: c.binder,
    filler: c.filler,
    description: c.description,
  };
}
