import api from './api';
import type { Cigar } from './cigarService';

export interface Humidor {
  id?: number;
  name: string;
  description?: string | null;
  capacity?: number | null;
  currentCount?: number;
  humidity?: number | null;
  userId?: number;
  cigars?: Cigar[]; // Добавим для полноты
}

export interface HumidorCigar {
  humidorId: number;
  cigarId: number;
  quantity: number;
  addedAt: string;
}

type Severity = 'secondary' | 'danger' | 'warn' | 'success';

const humidorService = {
  // Get all humidors for the current user
  async getHumidors(): Promise<Humidor[]> {
    const response = await api.get<Humidor[]>('/humidors');
    return response.data;
  },

  // Get a specific humidor by ID
  async getHumidor(id: number): Promise<Humidor> {
    const response = await api.get<Humidor>(`/humidors/${id}`);
    return response.data;
  },

  // Create a new humidor
  async createHumidor(humidor: Omit<Humidor, 'id' | 'currentCount' | 'cigars'>): Promise<Humidor> {
    const response = await api.post<Humidor>('/humidors', humidor);
    return response.data;
  },

  // Update an existing humidor
  async updateHumidor(id: number, humidor: Partial<Omit<Humidor, 'id' | 'currentCount' | 'cigars'>>): Promise<Humidor> {
    const response = await api.put<Humidor>(`/humidors/${id}`, humidor);
    return response.data;
  },

  // Delete a humidor
  async deleteHumidor(id: number): Promise<void> {
    await api.delete(`/humidors/${id}`);
  },

  // Get all cigars in a humidor with pagination
  async getCigarsInHumidor(humidorId: number, params: any = {}) {
    const response = await api.get<Cigar[]>(`/humidors/${humidorId}/cigars`, { params });
    return response.data;
  },

  // Add a cigar to a humidor (backend: POST .../humidors/{id}/cigars/{cigarId})
  async addCigarToHumidor(humidorId: number, cigarId: number): Promise<void> {
    await api.post(`/humidors/${humidorId}/cigars/${cigarId}`);
  },

  // Remove a cigar from a humidor
  async removeCigarFromHumidor(humidorId: number, cigarId: number): Promise<void> {
    await api.delete(`/humidors/${humidorId}/cigars/${cigarId}`);
  },

  getHumiditySeverity(humidity: number | null | undefined): Severity {
    if (humidity === null || humidity === undefined) return 'secondary';
    if (humidity < 60 || humidity > 72) return 'danger';
    if (humidity < 63 || humidity > 70) return 'warn';
    return 'success';
  },
};

export default humidorService;
