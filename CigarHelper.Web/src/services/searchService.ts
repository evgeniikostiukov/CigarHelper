import api from './api';
import type {
  ApiGlobalSearchResult,
  ApiSearchCigar,
  ApiSearchCigarBase,
  ApiSearchHumidor,
  ApiSearchBrand,
} from '@/types';

// ── Re-export для обратной совместимости ──────────────────────────────────
export type SearchCigarResult = ApiSearchCigar;
export type SearchHumidorResult = ApiSearchHumidor;
export type SearchCigarBaseResult = ApiSearchCigarBase;
export type SearchBrandResult = ApiSearchBrand;
export type GlobalSearchResult = ApiGlobalSearchResult;

export async function globalSearch(q: string, limit = 5): Promise<GlobalSearchResult> {
  const { data } = await api.get<GlobalSearchResult>('/search', { params: { q, limit } });
  return data;
}
