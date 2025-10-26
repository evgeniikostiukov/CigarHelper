import api from './api';

export interface CigarImage {
  id: number;
  isMain: boolean;
  imageData?: string; // Base64 string
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

const cigarService = {
  async getCigars(params: any = {}) {
    const response = await api.get<Cigar[]>('/cigars', { params });
    return response.data;
  },

  async getCigar(id: string): Promise<Cigar> {
    const response = await api.get(`/cigars/${id}`);
    return response.data;
  },

  async createCigar(cigarData: Omit<Cigar, 'id' | 'brandName' | 'imageUrl'>): Promise<Cigar> {
    const response = await api.post('/cigars', cigarData);
    return response.data;
  },

  async updateCigar(id: number, cigarData: Partial<Cigar>): Promise<void> {
    await api.put(`/cigars/${id}`, cigarData);
  },

  async deleteCigar(id: number): Promise<void> {
    await api.delete(`/cigars/${id}`);
  },

  async getBrands(): Promise<Brand[]> {
    const response = await api.get('/brands');
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

  async uploadImageByUrl(url: string): Promise<{ id: number }> {
    const response = await api.post('/cigarimages/upload-by-url', { url });
    return response.data;
  },
};

export default cigarService;
