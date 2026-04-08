import api from './api';

export interface CigarImageListItem {
  id: number;
  fileName: string | null;
  contentType: string | null;
  fileSize: number | null;
  description: string | null;
  isMain: boolean;
  cigarBaseId: number | null;
  userCigarId: number | null;
  createdAt: string;
  hasThumbnail: boolean;
}

export interface PagedCigarImagesAdminResponse {
  items: CigarImageListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export async function fetchAdminCigarImages(page: number, pageSize: number): Promise<PagedCigarImagesAdminResponse> {
  const { data } = await api.get<PagedCigarImagesAdminResponse>('/admin/cigar-images', {
    params: { page, pageSize },
  });
  return data;
}
