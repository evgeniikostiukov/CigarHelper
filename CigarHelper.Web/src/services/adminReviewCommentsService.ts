import api from './api';

export interface AdminReviewCommentRow {
  id: number;
  body: string;
  createdAt: string;
  authorUsername: string;
  isAuthorProfilePublic?: boolean;
  reviewId: number;
  targetSummary: string;
}

export interface AdminReviewCommentsPage {
  items: AdminReviewCommentRow[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export async function fetchPendingReviewComments(page = 1, pageSize = 20): Promise<AdminReviewCommentsPage> {
  const { data } = await api.get<AdminReviewCommentsPage>(`/admin/review-comments?page=${page}&pageSize=${pageSize}`);
  return data ?? { items: [], totalCount: 0, page: 1, pageSize, totalPages: 0 };
}

export async function approveReviewComment(id: number): Promise<void> {
  await api.post(`/admin/review-comments/${id}/approve`);
}

export async function rejectReviewComment(id: number): Promise<void> {
  await api.post(`/admin/review-comments/${id}/reject`);
}
