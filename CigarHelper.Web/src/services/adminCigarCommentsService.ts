import api from './api';

export interface AdminCigarCommentRow {
  id: number;
  body: string;
  createdAt: string;
  authorUsername: string;
  cigarBaseId?: number | null;
  userCigarId?: number | null;
  targetSummary: string;
}

export interface AdminCigarCommentsPage {
  items: AdminCigarCommentRow[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export async function fetchPendingComments(page = 1, pageSize = 20): Promise<AdminCigarCommentsPage> {
  const { data } = await api.get<AdminCigarCommentsPage>(`/admin/cigar-comments?page=${page}&pageSize=${pageSize}`);
  return data ?? { items: [], totalCount: 0, page: 1, pageSize, totalPages: 0 };
}

export async function approveCigarComment(id: number): Promise<void> {
  await api.post(`/admin/cigar-comments/${id}/approve`);
}

export async function rejectCigarComment(id: number): Promise<void> {
  await api.post(`/admin/cigar-comments/${id}/reject`);
}
