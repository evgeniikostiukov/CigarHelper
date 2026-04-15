import type { RouteLocationRaw } from 'vue-router';

export const PUBLIC_USER_PROFILE_ROUTE_NAME = 'PublicUserProfile' as const;

/** Маршрут на публичный профиль по имени пользователя (совпадает с `/u/:username`). */
export function publicUserProfileLocation(username: string): RouteLocationRaw {
  const u = username.trim();
  return { name: PUBLIC_USER_PROFILE_ROUTE_NAME, params: { username: u } };
}
