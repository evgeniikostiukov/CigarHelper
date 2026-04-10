import api from './api';

export type AdminRole = 'User' | 'Moderator' | 'Admin';

export interface AdminUserListItem {
  id: number;
  username: string;
  email: string | null;
  role: AdminRole;
  createdAt: string;
  lastLogin: string | null;
}

export interface PagedAdminUsersResponse {
  items: AdminUserListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface UpdateUserRoleResponse {
  success: boolean;
  message: string;
  newToken?: string | null;
}

export async function fetchAdminUsers(
  page: number,
  pageSize: number,
  search?: string,
): Promise<PagedAdminUsersResponse> {
  const { data } = await api.get<PagedAdminUsersResponse>('/admin/users', {
    params: { page, pageSize, search: search?.trim() || undefined },
  });
  return data;
}

export async function updateUserRole(
  userId: number,
  role: AdminRole,
  confirmSelfChange: boolean,
): Promise<UpdateUserRoleResponse> {
  const { data } = await api.put<UpdateUserRoleResponse>(`/admin/users/${userId}/role`, {
    role,
    confirmSelfChange,
  });
  return data;
}
