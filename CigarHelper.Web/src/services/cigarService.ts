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
  /** Инлайн-байты (только DB-хранилище, при MinIO = null) */
  imageData?: string;
  /** Устаревшее поле: CigarImageDto.Data */
  data?: string | number[];
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

/** Тело POST/PUT /cigars: API ждёт brandId, а не вложенный brand. */
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
  /** Скачивается на API и сохраняется как CigarImage (UserCigar). */
  imageUrl?: string | null;
}

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
  };
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

  async createCigar(cigarData: Omit<Cigar, 'id' | 'brandName' | 'imageUrl'>, imageUrl?: string | null): Promise<Cigar> {
    const payload = toCreateCigarPayload(cigarData, imageUrl);
    const response = await api.post<Cigar>('/cigars', payload);
    return response.data;
  },

  async updateCigar(id: number, cigarData: Partial<Cigar>, imageUrl?: string | null): Promise<void> {
    const payload = toUpdateCigarPayload(cigarData, imageUrl);
    await api.put(`/cigars/${id}`, payload);
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
    const payload: { url: string; cigarBaseId?: number } = { url };
    if (cigarBaseId != null) payload.cigarBaseId = cigarBaseId;
    const response = await api.post('/cigarimages/upload-by-url', payload);
    return response.data;
  },
};

export default cigarService;
