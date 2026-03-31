import api from './api';

export interface SearchCigarResult {
  id: number;
  name: string;
  brandName: string;
  humidorName?: string | null;
}

export interface SearchHumidorResult {
  id: number;
  name: string;
  description?: string | null;
}

export interface SearchCigarBaseResult {
  id: number;
  name: string;
  brandName: string;
}

export interface SearchBrandResult {
  id: number;
  name: string;
  country?: string | null;
}

export interface GlobalSearchResult {
  cigars: SearchCigarResult[];
  humidors: SearchHumidorResult[];
  cigarBases: SearchCigarBaseResult[];
  brands: SearchBrandResult[];
}

export async function globalSearch(q: string, limit = 5): Promise<GlobalSearchResult> {
  const { data } = await api.get<GlobalSearchResult>('/search', { params: { q, limit } });
  return data;
}
