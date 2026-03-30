import api from './api';

export interface BrandBreakdownItem {
  brandId: number;
  brandName: string;
  cigarCount: number;
  averageRating?: number | null;
}

export interface RecentReview {
  id: number;
  title: string;
  cigarName: string;
  cigarBrand: string;
  rating: number;
  createdAt: string;
}

export interface DashboardSummary {
  totalHumidors: number;
  totalCigars: number;
  totalCapacity: number;
  averageFillPercent: number;
  brandBreakdown: BrandBreakdownItem[];
  recentReviews: RecentReview[];
}

async function getDashboardSummary(): Promise<DashboardSummary> {
  const response = await api.get<DashboardSummary>('/dashboard/summary');
  return response.data;
}

const dashboardService = {
  getDashboardSummary,
};

export default dashboardService;

