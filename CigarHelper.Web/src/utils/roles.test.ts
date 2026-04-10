import { describe, it, expect } from 'vitest';
import type { User } from '@/services/authService';
import { getRoleClaims, hasRole, getAuthUserId } from './roles';

function user(role: User['role']): User {
  return {
    id: 1,
    nameid: '1',
    email: 'a@b.c',
    unique_name: 'a',
    role,
  };
}

describe('getRoleClaims', () => {
  it('пусто для отсутствующего пользователя', () => {
    expect(getRoleClaims(null)).toEqual([]);
    expect(getRoleClaims(undefined)).toEqual([]);
  });

  it('одна строка роли', () => {
    expect(getRoleClaims(user('Admin'))).toEqual(['Admin']);
  });

  it('несколько ролей из массива', () => {
    expect(getRoleClaims(user(['Admin', 'Moderator']))).toEqual(['Admin', 'Moderator']);
  });
});

describe('hasRole', () => {
  it('находит роль в списке', () => {
    expect(hasRole(user('Admin'), 'Admin')).toBe(true);
    expect(hasRole(user(['User', 'Admin']), 'Admin')).toBe(true);
  });

  it('не находит отсутствующую роль', () => {
    expect(hasRole(user('User'), 'Admin')).toBe(false);
  });
});

describe('getAuthUserId', () => {
  it('берёт id как число', () => {
    expect(getAuthUserId(user('User'))).toBe(1);
  });

  it('нормализует id из строки (как в JWT JSON)', () => {
    const u = { ...user('User'), id: '42' as unknown as number };
    expect(getAuthUserId(u)).toBe(42);
  });

  it('берёт nameid при отсутствии числового id', () => {
    const u = {
      nameid: '7',
      unique_name: 'x',
      role: 'User' as const,
    } as unknown as User;
    expect(getAuthUserId(u)).toBe(7);
  });

  it('берёт claim NameIdentifier (.NET JWT)', () => {
    const u = {
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': '99',
      unique_name: 'x',
      role: 'User' as const,
    } as unknown as User;
    expect(getAuthUserId(u)).toBe(99);
  });

  it('null без пользователя', () => {
    expect(getAuthUserId(null)).toBeNull();
  });
});
