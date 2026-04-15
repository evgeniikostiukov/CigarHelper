import api from './api';

export interface AdminDeletedReviewRow {
  id: number;
  title: string;
  userId: number;
  username: string;
  isAuthorProfilePublic?: boolean;
  cigarBaseId: number;
  cigarName: string;
  cigarBrand: string;
  createdAt: string;
  deletedAt: string | null;
}

export interface AdminDeletedReviewsPage {
  items: AdminDeletedReviewRow[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export async function fetchDeletedReviews(page = 1, pageSize = 20): Promise<AdminDeletedReviewsPage> {
  const { data } = await api.get<AdminDeletedReviewsPage>(`/admin/reviews/deleted?page=${page}&pageSize=${pageSize}`);
  return data ?? { items: [], totalCount: 0, page: 1, pageSize, totalPages: 0 };
}

export async function restoreReview(id: number): Promise<void> {
  await api.post(`/admin/reviews/${id}/restore`);
}
