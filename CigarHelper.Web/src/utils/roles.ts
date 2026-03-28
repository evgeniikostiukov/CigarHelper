import type { User } from '@/services/authService';

/** Нормализованный список значений роли из JWT (claim role / ClaimTypes.Role). */
export function getRoleClaims(user: User | null | undefined): string[] {
  if (!user?.role) return [];
  return Array.isArray(user.role) ? user.role : [user.role];
}

export function hasRole(user: User | null | undefined, role: string): boolean {
  return getRoleClaims(user).includes(role);
}

export function hasAnyRole(user: User | null | undefined, roles: readonly string[]): boolean {
  if (!roles.length) return true;
  const claims = getRoleClaims(user);
  return roles.some((r) => claims.includes(r));
}

/** Идентификатор пользователя из payload JWT (поля id / nameid / sub). */
export function getAuthUserId(user: User | null | undefined): number | null {
  if (!user) return null;
  const u = user as Record<string, unknown>;
  const raw = u.id ?? u.nameid ?? u.sub;
  if (typeof raw === 'number' && Number.isFinite(raw)) return raw;
  if (typeof raw === 'string') {
    const n = parseInt(raw, 10);
    return Number.isFinite(n) ? n : null;
  }
  return null;
}
