import api from './api';
import type { ApiDashboardSummary } from '@/types';

// ── Нормализованные типы для компонентного слоя ────────────────────────────
// API возвращает все поля optional (Swashbuckle без [Required]).
// Сервис нормализует ответ → компоненты получают гарантированно определённые поля.

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
  username: string;
  isAuthorProfilePublic: boolean;
  userAvatarUrl: string | null;
}

export interface CigarTimelinePoint {
  period: string;
  purchasedCount: number;
  smokedCount: number;
}

export interface StaleCigarReminder {
  cigarId: number;
  cigarName: string;
  brandName: string;
  daysUntouched: number;
  lastTouchedAt: string;
}

export interface DashboardSummary {
  totalHumidors: number;
  totalCigars: number;
  totalCapacity: number;
  averageFillPercent: number;
  averageDaysToSmoke: number;
  /** Средняя оценка по полю Rating у сигар коллекции (только с заполненной оценкой). */
  averageCigarRating: number | null;
  brandBreakdown: BrandBreakdownItem[];
  recentReviews: RecentReview[];
  timeline: CigarTimelinePoint[];
  staleCigarReminders: StaleCigarReminder[];
}

function normalizeSummary(d: ApiDashboardSummary): DashboardSummary {
  return {
    totalHumidors: d.totalHumidors ?? 0,
    totalCigars: d.totalCigars ?? 0,
    totalCapacity: d.totalCapacity ?? 0,
    averageFillPercent: d.averageFillPercent ?? 0,
    averageDaysToSmoke: d.averageDaysToSmoke ?? 0,
    averageCigarRating: d.averageCigarRating ?? null,
    brandBreakdown: (d.brandBreakdown ?? []).map((b) => ({
      brandId: b.brandId ?? 0,
      brandName: b.brandName,
      cigarCount: b.cigarCount ?? 0,
      averageRating: b.averageRating,
    })),
    recentReviews: (d.recentReviews ?? []).map((r) => ({
      id: r.id ?? 0,
      title: r.title,
      cigarName: r.cigarName,
      cigarBrand: r.cigarBrand,
      rating: r.rating ?? 0,
      createdAt: r.createdAt ?? '',
      username: r.username ?? '',
      isAuthorProfilePublic: r.isAuthorProfilePublic === true,
      userAvatarUrl: r.userAvatarUrl ?? null,
    })),
    timeline: (d.timeline ?? []).map((t) => ({
      period: t.period ?? '',
      purchasedCount: t.purchasedCount ?? 0,
      smokedCount: t.smokedCount ?? 0,
    })),
    staleCigarReminders: (d.staleCigarReminders ?? []).map((s) => ({
      cigarId: s.cigarId ?? 0,
      cigarName: s.cigarName,
      brandName: s.brandName,
      daysUntouched: s.daysUntouched ?? 0,
      lastTouchedAt: s.lastTouchedAt ?? '',
    })),
  };
}

async function getDashboardSummary(): Promise<DashboardSummary> {
  const response = await api.get<ApiDashboardSummary>('/dashboard/summary');
  return normalizeSummary(response.data);
}

const dashboardService = {
  getDashboardSummary,
};

export default dashboardService;
