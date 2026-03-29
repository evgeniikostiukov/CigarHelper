import api from './api';
import type { Humidor } from './humidorService';

export interface MyProfile {
  id: number;
  username: string;
  email: string;
  role: string;
  isProfilePublic: boolean;
  createdAt: string;
  lastLogin: string | null;
}

export interface UpdateProfilePayload {
  username: string;
  email: string;
  isProfilePublic: boolean;
}

export interface UpdateProfileResponse {
  success: boolean;
  message: string;
  profile?: MyProfile;
  newToken?: string | null;
}

export interface PublicProfile {
  username: string;
  createdAt: string;
  lastLogin: string | null;
  humidors: Humidor[];
}

export interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface ChangePasswordResponse {
  success: boolean;
  message: string;
  retryAfterSeconds?: number | null;
}

export async function getMyProfile(): Promise<MyProfile> {
  const { data } = await api.get<MyProfile>('/profile');
  return data;
}

export async function updateProfile(payload: UpdateProfilePayload): Promise<UpdateProfileResponse> {
  const { data } = await api.patch<UpdateProfileResponse>('/profile', payload);
  return data;
}

export async function changePassword(payload: ChangePasswordPayload): Promise<ChangePasswordResponse> {
  const { data } = await api.post<ChangePasswordResponse>('/profile/change-password', payload);
  return data;
}

export async function getPublicProfile(username: string): Promise<PublicProfile> {
  const { data } = await api.get<PublicProfile>(`/public/users/${encodeURIComponent(username)}`);
  return data;
}

export async function getPublicHumidor(username: string, humidorId: number): Promise<Humidor> {
  const { data } = await api.get<Humidor>(`/public/users/${encodeURIComponent(username)}/humidors/${humidorId}`);
  return data;
}
