import api from './api';

export interface CigarCommentDto {
  id: number;
  body: string;
  createdAt: string;
  authorUserId: number;
  authorUsername: string;
  cigarBaseId?: number | null;
  userCigarId?: number | null;
}

export async function fetchComments(params: {
  cigarBaseId?: number;
  userCigarId?: number;
}): Promise<CigarCommentDto[]> {
  const sp = new URLSearchParams();
  if (params.cigarBaseId != null) {
    sp.set('cigarBaseId', String(params.cigarBaseId));
  }
  if (params.userCigarId != null) {
    sp.set('userCigarId', String(params.userCigarId));
  }
  const { data } = await api.get<CigarCommentDto[]>(`/cigarcomments?${sp.toString()}`);
  return data ?? [];
}

export async function createComment(payload: {
  cigarBaseId?: number;
  userCigarId?: number;
  body: string;
}): Promise<CigarCommentDto> {
  const { data } = await api.post<CigarCommentDto>('/cigarcomments', {
    cigarBaseId: payload.cigarBaseId ?? null,
    userCigarId: payload.userCigarId ?? null,
    body: payload.body,
  });
  return data;
}

export async function deleteComment(id: number): Promise<void> {
  await api.delete(`/cigarcomments/${id}`);
}
