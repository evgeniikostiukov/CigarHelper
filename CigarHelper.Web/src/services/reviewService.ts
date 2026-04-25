import type { AxiosInstance } from 'axios';
import api from './api';

export interface ReviewImage {
  id: number;
  imageData: string;
  caption?: string;
}

export interface ReviewListItem {
  id: number;
  title: string;
  summary?: string | null;
  rating: number;
  userId: number;
  username: string;
  /** С сервера: виден ли публичный профиль автора. */
  isAuthorProfilePublic?: boolean;
  userAvatarUrl?: string | null;
  cigarBaseId: number;
  cigarName: string;
  cigarBrand: string;
  mainImageBytes?: string | null;
  imageCount?: number | null;
  createdAt: string;
}

export interface Review {
  id: number;
  userId: number;
  title: string;
  content: string;
  rating: number;
  cigarBaseId: number;
  /** Запись коллекции, если обзор с неё; иначе отсутствует. */
  userCigarId?: number | null;
  cigarName: string;
  cigarBrand: string;
  cigarCountry?: string | null;
  cigarLengthMm?: number | null;
  cigarDiameter?: number | null;
  cigarWrapper?: string | null;
  cigarBinder?: string | null;
  cigarFiller?: string | null;
  username: string;
  isAuthorProfilePublic?: boolean;
  userAvatarUrl?: string;
  createdAt: string;
  smokingDate: string;
  images: ReviewImage[];
  smokingExperience?: string;
  aroma?: string;
  taste?: string;
  construction?: number;
  burnQuality?: number;
  draw?: number;
  venue?: string;
  bodyStrengthScore?: number | null;
  aromaScore?: number | null;
  pairingsScore?: number | null;
  smokingDurationMinutes?: number | null;
}

export interface CreateReviewDto {
  cigarBaseId: number;
  userCigarId?: number | null;
  title: string;
  content: string;
  rating: number;
  smokingDate?: string;
  smokingExperience?: string;
  aroma?: string;
  taste?: string;
  construction?: number;
  burnQuality?: number;
  draw?: number;
  venue?: string;
  bodyStrengthScore?: number | null;
  aromaScore?: number | null;
  pairingsScore?: number | null;
  smokingDurationMinutes?: number | null;
  images?: { imageData: string; caption?: string }[];
}

class ReviewService {
  private api: AxiosInstance;

  constructor(apiInstance: AxiosInstance) {
    this.api = apiInstance;
  }

  async getReviews(params: Record<string, unknown> = {}): Promise<ReviewListItem[]> {
    const response = await this.api.get<ReviewListItem[]>('/reviews', { params });
    return response.data;
  }

  async getReview(id: string): Promise<Review> {
    const response = await this.api.get<Review>(`/reviews/${id}`);
    return response.data;
  }

  async createReview(data: CreateReviewDto): Promise<Review> {
    const response = await this.api.post<Review>('/reviews', data);
    return response.data;
  }

  async updateReview(id: string, data: CreateReviewDto): Promise<void> {
    await this.api.put(`/reviews/${id}`, data);
  }

  async deleteReview(id: number): Promise<void> {
    await this.api.delete(`/reviews/${id}`);
  }
}

const reviewService = new ReviewService(api);
export default reviewService;
