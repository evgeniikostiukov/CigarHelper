import api from './api';

export type ReviewCommentModerationStatus = 'Pending' | 'Approved' | 'Rejected';

export interface ReviewCommentDto {
  id: number;
  reviewId: number;
  body: string;
  createdAt: string;
  authorUserId: number;
  authorUsername: string;
  isAuthorProfilePublic?: boolean;
  moderationStatus: ReviewCommentModerationStatus;
}

export async function fetchReviewComments(reviewId: number): Promise<ReviewCommentDto[]> {
  const { data } = await api.get<ReviewCommentDto[]>(`/reviewcomments?reviewId=${reviewId}`);
  return data ?? [];
}

export async function createReviewComment(payload: { reviewId: number; body: string }): Promise<ReviewCommentDto> {
  const { data } = await api.post<ReviewCommentDto>('/reviewcomments', {
    reviewId: payload.reviewId,
    body: payload.body,
  });
  return data;
}

export async function deleteReviewComment(id: number): Promise<void> {
  await api.delete(`/reviewcomments/${id}`);
}
