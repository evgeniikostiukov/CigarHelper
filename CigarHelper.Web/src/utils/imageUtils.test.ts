import { describe, it, expect } from 'vitest';
import { arrayBufferToBase64 } from './imageUtils';

describe('arrayBufferToBase64', () => {
  it('возвращает пустую строку для null и undefined', () => {
    expect(arrayBufferToBase64(null)).toBe('');
    expect(arrayBufferToBase64(undefined)).toBe('');
  });

  it('не меняет уже base64-строку', () => {
    expect(arrayBufferToBase64('YWJj')).toBe('YWJj');
  });

  it('кодирует массив байт в base64', () => {
    expect(arrayBufferToBase64([65, 66, 67])).toBe('QUJD');
  });
});
