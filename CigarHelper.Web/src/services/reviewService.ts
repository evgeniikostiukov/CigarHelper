import type { AxiosInstance } from 'axios';
import api from './api';

export interface ReviewImage {
  id: number;
  imageData: string;
  caption?: string;
}

export interface Review {
  id: number;
  userId: number;
  title: string;
  content: string;
  rating: number;
  cigarId: number;
  cigarName: string;
  cigarBrand: string;
  username: string;
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
}

// export interface ReviewListItemDto{
//   id: number;
//   title: string;
//   summary: string;
//   rating: number;
//   userId: number;
//   userName: string;
//   cigarId: number;
//   cigarName: string;
//   cigarBrand: string;
//   mainImageUr
// }

export interface CreateReviewDto {
  cigarId: number;
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
  images?: { imageData: string; caption?: string }[];
}

class ReviewService {
  private api: AxiosInstance;

  constructor(apiInstance: AxiosInstance) {
    this.api = apiInstance;
  }

  async getReviews(params: any = {}): Promise<Review[]> {
    const response = await this.api.get<Review[]>('/reviews', { params });
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
