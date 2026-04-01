import api from './api';
import type {
  ApiCigarBase,
  ApiCigarResponse,
  ApiBrand,
  ApiCigarImage,
  ApiCigarBasePaginatedResult,
} from '@/types';

// ── Нормализованные типы для компонентного слоя ─────────────────────────────
// OpenAPI-схема объявляет большинство полей optional; компонентный слой
// работает с гарантированно определёнными полями после нормализации.

export interface CigarImage {
  id: number;
  isMain: boolean;
  contentType?: string | null;
  fileName?: string | null;
  fileSize?: number | null;
  hasThumbnail?: boolean;
  cigarBaseId?: number | null;
  userCigarId?: number | null;
  imageData?: string;
  /** @deprecated Use imageData */
  data?: string | number[];
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

export interface CigarBase {
  id: number;
  name: string;
  brand: Brand;
  country: string;
  strength: string;
  size: string;
  wrapper?: string;
  binder?: string;
  filler?: string;
  description?: string;
  isModerated?: boolean;
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
  purchasedAt?: string;
  smokedAt?: string | null;
  lastTouchedAt?: string;
  isSmoked?: boolean;
  /** Остаток сигар в записи коллекции. */
  quantity?: number;
  images?: CigarImage[];
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
}

export interface CigarWriteApiPayload {
  name: string;
  brandId: number;
  country?: string | null;
  description?: string | null;
  strength?: string | null;
  size?: string | null;
  wrapper?: string | null;
  binder?: string | null;
  filler?: string | null;
  price?: number | null;
  rating?: number | null;
  humidorId?: number | null;
  imageUrl?: string | null;
  quantity?: number | null;
}

// ── Нормализация API → компонентные типы ────────────────────────────────────

function normalizeBrand(b: ApiBrand | undefined): Brand {
  return {
    id: b?.id ?? 0,
    name: b?.name ?? '',
    country: b?.country,
    description: b?.description,
    isModerated: b?.isModerated ?? false,
    createdAt: b?.createdAt ?? '',
    logoBytes: b?.logoBytes ?? undefined,
  };
}

function normalizeCigarImage(img: ApiCigarImage): CigarImage {
  return {
    id: img.id ?? 0,
    isMain: img.isMain ?? false,
    contentType: img.contentType,
    fileName: img.fileName,
    fileSize: img.fileSize,
    hasThumbnail: img.hasThumbnail,
    cigarBaseId: img.cigarBaseId,
    userCigarId: img.userCigarId,
    imageData: img.imageData ?? undefined,
    data: img.data ?? undefined,
  };
}

function normalizeCigarBase(cb: ApiCigarBase): CigarBase {
  return {
    id: cb.id ?? 0,
    name: cb.name ?? '',
    brand: normalizeBrand(cb.brand),
    country: cb.country ?? '',
    strength: cb.strength ?? '',
    size: cb.size ?? '',
    wrapper: cb.wrapper ?? undefined,
    binder: cb.binder ?? undefined,
    filler: cb.filler ?? undefined,
    description: cb.description ?? undefined,
    isModerated: cb.isModerated,
    images: cb.images?.map(normalizeCigarImage),
  };
}

function normalizeCigar(uc: ApiCigarResponse): Cigar {
  return {
    id: uc.id ?? 0,
    name: uc.name ?? '',
    brand: normalizeBrand(uc.brand),
    country: uc.country ?? null,
    size: uc.size ?? null,
    strength: uc.strength ?? null,
    price: uc.price ?? null,
    rating: uc.rating ?? null,
    description: uc.description ?? null,
    wrapper: uc.wrapper ?? null,
    binder: uc.binder ?? null,
    filler: uc.filler ?? null,
    humidorId: uc.humidorId ?? null,
    purchasedAt: uc.purchasedAt,
    smokedAt: uc.smokedAt,
    lastTouchedAt: uc.lastTouchedAt,
    isSmoked: uc.isSmoked,
    quantity: uc.quantity ?? 0,
    images: uc.images?.map(normalizeCigarImage),
  };
}

function normalizePaginatedCigarBases(raw: ApiCigarBasePaginatedResult): PaginatedResult<CigarBase> {
  return {
    items: (raw.items ?? []).map(normalizeCigarBase),
    totalCount: raw.totalCount ?? 0,
  };
}

// ── Helpers ─────────────────────────────────────────────────────────────────

function resolveBrandId(brand: Brand | undefined): number {
  const id = brand?.id;
  return typeof id === 'number' && !Number.isNaN(id) ? id : 0;
}

function toCreateCigarPayload(
  data: Omit<Cigar, 'id' | 'brandName' | 'imageUrl'>,
  imageUrl?: string | null,
): CigarWriteApiPayload {
  const brandId = resolveBrandId(data.brand);
  if (brandId <= 0) {
    throw new Error('BRAND_REQUIRED');
  }
  const trimmedUrl = imageUrl?.trim();
  return {
    name: data.name,
    brandId,
    country: data.country,
    description: data.description,
    strength: data.strength,
    size: data.size,
    wrapper: data.wrapper,
    binder: data.binder,
    filler: data.filler,
    price: data.price,
    rating: data.rating,
    humidorId: data.humidorId ?? null,
    ...(trimmedUrl ? { imageUrl: trimmedUrl } : {}),
    ...(data.quantity != null && data.quantity >= 1 ? { quantity: data.quantity } : { quantity: 1 }),
  };
}

function toUpdateCigarPayload(data: Partial<Cigar>, imageUrl?: string | null): CigarWriteApiPayload {
  const brandId = resolveBrandId(data.brand);
  if (brandId <= 0) {
    throw new Error('BRAND_REQUIRED');
  }
  if (!data.name?.trim()) {
    throw new Error('NAME_REQUIRED');
  }
  const trimmedUrl = imageUrl?.trim();
  return {
    name: data.name,
    brandId,
    country: data.country,
    description: data.description,
    strength: data.strength,
    size: data.size,
    wrapper: data.wrapper,
    binder: data.binder,
    filler: data.filler,
    price: data.price,
    rating: data.rating,
    humidorId: data.humidorId ?? null,
    ...(trimmedUrl ? { imageUrl: trimmedUrl } : {}),
    ...(data.quantity != null && data.quantity >= 1 ? { quantity: data.quantity } : {}),
  };
}

// ── Service ─────────────────────────────────────────────────────────────────

const cigarService = {
  async getCigars(params: Record<string, unknown> = {}): Promise<Cigar[]> {
    const response = await api.get<ApiCigarResponse[]>('/cigars', { params });
    return response.data.map(normalizeCigar);
  },

  async getCigar(id: string): Promise<Cigar> {
    const response = await api.get<ApiCigarResponse>(`/cigars/${id}`);
    return normalizeCigar(response.data);
  },

  async createCigar(
    cigarData: Omit<Cigar, 'id' | 'brandName' | 'imageUrl'>,
    imageUrl?: string | null,
  ): Promise<Cigar> {
    const payload = toCreateCigarPayload(cigarData, imageUrl);
    const response = await api.post<ApiCigarResponse>('/cigars', payload);
    return normalizeCigar(response.data);
  },

  async updateCigar(id: number, cigarData: Partial<Cigar>, imageUrl?: string | null): Promise<void> {
    const payload = toUpdateCigarPayload(cigarData, imageUrl);
    await api.put(`/cigars/${id}`, payload);
  },

  async deleteCigar(id: number): Promise<void> {
    await api.delete(`/cigars/${id}`);
  },

  async markCigarAsSmoked(id: number): Promise<Cigar> {
    const response = await api.post<ApiCigarResponse>(`/cigars/${id}/smoked`, {});
    return normalizeCigar(response.data);
  },

  async getBrands(): Promise<Brand[]> {
    const response = await api.get<ApiBrand[]>('/brands');
    return response.data.map(normalizeBrand);
  },

  async getBrand(id: number): Promise<Brand> {
    const response = await api.get<ApiBrand>(`/brands/${id}`);
    return normalizeBrand(response.data);
  },

  async getAllBrands(params: Record<string, unknown> = {}): Promise<Brand[]> {
    const response = await api.get<ApiBrand[]>('/cigars/brands', { params });
    return response.data.map(normalizeBrand);
  },

  async createBrand(brandData: Omit<Brand, 'id' | 'createdAt'>): Promise<Brand> {
    const response = await api.post<ApiBrand>('/brands', brandData);
    return normalizeBrand(response.data);
  },

  async updateBrand(id: number, brandData: Partial<Omit<Brand, 'id' | 'createdAt'>>): Promise<void> {
    await api.put(`/brands/${id}`, brandData);
  },

  async deleteBrand(id: number): Promise<void> {
    await api.delete(`/brands/${id}`);
  },

  async getCigarBasesPaginated(params: Record<string, unknown> = {}): Promise<PaginatedResult<CigarBase>> {
    const response = await api.get<ApiCigarBasePaginatedResult>('/cigars/bases/paginated', { params });
    return normalizePaginatedCigarBases(response.data);
  },

  async getCigarBase(id: number): Promise<CigarBase> {
    const response = await api.get<ApiCigarBase>(`/cigars/bases/${id}`);
    return normalizeCigarBase(response.data);
  },

  async createCigarBase(formData: FormData): Promise<CigarBase> {
    const response = await api.post<ApiCigarBase>('/cigars/bases', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return normalizeCigarBase(response.data);
  },

  async updateCigarBase(id: number, formData: FormData): Promise<CigarBase> {
    const response = await api.put<ApiCigarBase>(`/cigars/bases/${id}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return normalizeCigarBase(response.data);
  },

  async uploadImageByUrl(url: string, cigarBaseId?: number | null): Promise<{ id: number }> {
    const payload: { url: string; cigarBaseId?: number } = { url };
    if (cigarBaseId != null) payload.cigarBaseId = cigarBaseId;
    const response = await api.post('/cigarimages/upload-by-url', payload);
    return response.data;
  },
};

export default cigarService;
