import api from './api';

export interface CigarImage {
  id: number;
  isMain: boolean;
  contentType?: string | null;
  fileName?: string | null;
  fileSize?: number | null;
  hasThumbnail?: boolean;
  cigarBaseId?: number | null;
  userCigarId?: number | null;
  /** Base64 или сырой массив байт, если DTO пришёл с вложенными данными (не в схеме OpenAPI). */
  imageData?: string | null;
  data?: string | number[] | null;
}

export interface CigarBase {
  id: number;
  name: string;
  /** Модерация записи справочника CigarBase (не путать с brand.isModerated). */
  isModerated?: boolean;
  brand: Brand;
  country: string;
  strength: string;
  size: string;
  wrapper?: string;
  binder?: string;
  filler?: string;
  description?: string;
  images?: CigarImage[];
  brandLogoBytes?: string;
  brandLogoUrl?: string;
}

export interface Cigar {
  id: number;
  name: string;
  brand: Brand;
  country: string | null;
  size: string | null;
  strength: string | null;
  price: number | null;
  rating: number | null;
  description: string | null;
  wrapper: string | null;
  binder: string | null;
  filler: string | null;
  humidorId: number | null;
  taste?: string | null;
  aroma?: string | null;
  /** Количество сигар (шт.) в записи коллекции; при отсутствии в ответе API считать 1. */
  quantity?: number | null;
  purchasedAt?: string;
  smokedAt?: string | null;
  lastTouchedAt?: string;
  isSmoked?: boolean;
  images?: CigarImage[];
}

export interface Brand {
  id: number;
  name: string;
  country?: string | null;
  description?: string | null;
  logoUrl?: string | null;
  isModerated: boolean;
  createdAt: string;
  logoBytes?: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
}

/** POST /api/cigars — только по CigarBaseId (модерированный справочник). */
export interface CreateUserCigarPayload {
  cigarBaseId: number;
  price?: number | null;
  humidorId?: number | null;
  taste?: string | null;
  aroma?: string | null;
  rating?: number | null;
  /** 1–9999; не задано — на сервере 1. */
  quantity?: number | null;
  imageUrl?: string | null;
  imageUrls?: string[] | null;
}

/** PUT /api/cigars/{id} — личные поля (все четыре обязательны в теле, чтобы не затирать вкус/аромат случайно) и опционально галерея. */
export interface PatchUserCigarPayload {
  price: number | null;
  humidorId: number | null;
  taste: string | null;
  aroma: string | null;
  rating: number | null;
  quantity: number;
  imageUrl?: string | null;
  imageUrlsToAdd?: string[];
  imageIdsToRemove?: number[];
}

export interface CigarUpdateImageOptions {
  imageUrl?: string | null;
  imageUrlsToAdd?: string[];
  imageIdsToRemove?: number[];
}

function normalizeImageUrlList(urls: string[] | null | undefined): string[] {
  return (urls ?? []).map((u) => u.trim()).filter(Boolean);
}

function normalizeQuantity(value: number | null | undefined): number {
  if (value == null || !Number.isFinite(value)) return 1;
  const n = Math.trunc(value);
  return Math.min(9999, Math.max(1, n));
}

function buildCreateCigarBody(payload: CreateUserCigarPayload): Record<string, unknown> {
  const body: Record<string, unknown> = {
    cigarBaseId: payload.cigarBaseId,
    price: payload.price ?? null,
    humidorId: payload.humidorId ?? null,
    taste: payload.taste?.trim() ? payload.taste.trim() : null,
    aroma: payload.aroma?.trim() ? payload.aroma.trim() : null,
    rating: payload.rating != null && payload.rating >= 1 && payload.rating <= 10 ? payload.rating : null,
    quantity: normalizeQuantity(payload.quantity ?? undefined),
  };
  const list = normalizeImageUrlList(payload.imageUrls ?? []);
  const legacy = payload.imageUrl?.trim();
  if (list.length > 0) {
    body.imageUrls = list;
  } else if (legacy) {
    body.imageUrl = legacy;
  }
  return body;
}

function buildPatchCigarBody(
  fields: Pick<PatchUserCigarPayload, 'price' | 'humidorId' | 'taste' | 'aroma' | 'rating' | 'quantity'>,
  image?: CigarUpdateImageOptions,
): Record<string, unknown> {
  const body: Record<string, unknown> = {
    price: fields.price ?? null,
    humidorId: fields.humidorId ?? null,
    taste: fields.taste != null && fields.taste.trim() !== '' ? fields.taste.trim() : null,
    aroma: fields.aroma != null && fields.aroma.trim() !== '' ? fields.aroma.trim() : null,
    rating: fields.rating != null && fields.rating >= 1 && fields.rating <= 10 ? fields.rating : null,
    quantity: normalizeQuantity(fields.quantity),
  };
  if (!image) {
    return body;
  }
  const trimmedUrl = image.imageUrl?.trim();
  const toAdd = normalizeImageUrlList(image.imageUrlsToAdd);
  const toRemove = (image.imageIdsToRemove ?? []).filter((id) => Number.isFinite(id));
  if (trimmedUrl) {
    body.imageUrl = trimmedUrl;
  }
  if (toAdd.length > 0) {
    body.imageUrlsToAdd = toAdd;
  }
  if (toRemove.length > 0) {
    body.imageIdsToRemove = toRemove;
  }
  return body;
}

const cigarService = {
  async getCigars(params: any = {}) {
    const response = await api.get<Cigar[]>('/cigars', { params });
    return response.data;
  },

  async getCigar(id: string): Promise<Cigar> {
    const response = await api.get(`/cigars/${id}`);
    return response.data;
  },

  async createCigar(payload: CreateUserCigarPayload): Promise<Cigar> {
    const response = await api.post<Cigar>('/cigars', buildCreateCigarBody(payload));
    return response.data;
  },

  async updateCigar(id: number, payload: PatchUserCigarPayload): Promise<void> {
    const { imageUrl, imageUrlsToAdd, imageIdsToRemove, price, humidorId, taste, aroma, rating, quantity } = payload;
    const hasImageOpts =
      (imageUrl != null && imageUrl !== '') ||
      (imageUrlsToAdd != null && imageUrlsToAdd.length > 0) ||
      (imageIdsToRemove != null && imageIdsToRemove.length > 0);
    const body = buildPatchCigarBody(
      { price, humidorId, taste, aroma, rating, quantity },
      hasImageOpts ? { imageUrl, imageUrlsToAdd, imageIdsToRemove } : undefined,
    );
    await api.put(`/cigars/${id}`, body);
  },

  async deleteCigar(id: number): Promise<void> {
    await api.delete(`/cigars/${id}`);
  },

  async markCigarAsSmoked(id: number): Promise<Cigar> {
    const response = await api.post<Cigar>(`/cigars/${id}/smoked`, {});
    return response.data;
  },

  async getBrands(): Promise<Brand[]> {
    const response = await api.get('/brands');
    return response.data;
  },

  async getBrand(id: number): Promise<Brand> {
    const response = await api.get<Brand>(`/brands/${id}`);
    return response.data;
  },

  async getAllBrands(params: any = {}) {
    const response = await api.get<Brand[]>('/cigars/brands', { params });
    return response.data;
  },

  async createBrand(brandData: Omit<Brand, 'id' | 'createdAt'>): Promise<Brand> {
    const response = await api.post('/brands', brandData);
    return response.data;
  },

  async updateBrand(id: number, brandData: Partial<Omit<Brand, 'id' | 'createdAt'>>): Promise<void> {
    await api.put(`/brands/${id}`, brandData);
  },

  async deleteBrand(id: number): Promise<void> {
    await api.delete(`/brands/${id}`);
  },

  async getCigarBasesPaginated(params: any = {}): Promise<PaginatedResult<CigarBase>> {
    const response = await api.get('/cigars/bases/paginated', { params });
    return response.data;
  },

  async getCigarBase(id: number): Promise<CigarBase> {
    const response = await api.get<CigarBase>(`/cigars/bases/${id}`);
    return response.data;
  },

  async createCigarBase(formData: FormData): Promise<CigarBase> {
    const response = await api.post('/cigars/bases', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  async updateCigarBase(id: number, formData: FormData): Promise<CigarBase> {
    const response = await api.put(`/cigars/bases/${id}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  async uploadImageByUrl(url: string, cigarBaseId?: number | null): Promise<{ id: number }> {
    const p: { url: string; cigarBaseId?: number } = { url };
    if (cigarBaseId != null) p.cigarBaseId = cigarBaseId;
    const response = await api.post('/cigarimages/upload-by-url', p);
    return response.data;
  },

  async setCigarImageMain(imageId: number): Promise<void> {
    await api.patch(`/cigarimages/${imageId}/set-main`);
  },
};

export default cigarService;
