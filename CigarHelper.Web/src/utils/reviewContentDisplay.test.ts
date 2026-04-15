import { describe, expect, it } from 'vitest';
import {
  extractBodyInnerHtml,
  plainTextReviewExcerpt,
  sanitizeReviewBodyForDisplay,
  stripReviewHtmlToPlainText,
} from './reviewContentDisplay';

describe('reviewContentDisplay', () => {
  it('stripReviewHtmlToPlainText убирает теги TipTap', () => {
    const html = '<p>Очень <strong>хорошая</strong> сигара.</p>';
    expect(stripReviewHtmlToPlainText(html)).toBe('Очень хорошая сигара.');
  });

  it('stripReviewHtmlToPlainText извлекает текст из оболочки html/body', () => {
    const doc = `<!DOCTYPE html><html><head><title>x</title></head><body><p>Тело</p></body></html>`;
    expect(stripReviewHtmlToPlainText(doc)).toBe('Тело');
  });

  it('extractBodyInnerHtml убирает внешнюю оболочку документа', () => {
    const raw = `<html><body><p>Hi</p></body></html>`;
    expect(extractBodyInnerHtml(raw)).toContain('Hi');
    expect(extractBodyInnerHtml(raw)).not.toMatch(/<html\b/i);
  });

  it('plainTextReviewExcerpt сокращает длинный текст', () => {
    const long = 'слово '.repeat(80);
    const out = plainTextReviewExcerpt(long, 40);
    expect(out.length).toBeLessThanOrEqual(40);
    expect(out.endsWith('…')).toBe(true);
  });

  it('sanitizeReviewBodyForDisplay сохраняет безопасную разметку TipTap', () => {
    const html = '<p>Текст</p><h2>Заголовок</h2><ul><li>один</li></ul>';
    const out = sanitizeReviewBodyForDisplay(html);
    expect(out).toContain('Текст');
    expect(out).toContain('Заголовок');
    expect(out).not.toMatch(/<script/i);
  });

  it('sanitizeReviewBodyForDisplay вырезает script', () => {
    const evil = '<p>x</p><script>alert(1)</script><p>y</p>';
    const out = sanitizeReviewBodyForDisplay(evil);
    expect(out).not.toMatch(/script/i);
    expect(out).toContain('x');
    expect(out).toContain('y');
  });

  it('sanitizeReviewBodyForDisplay оборачивает плейнтекст в параграфы', () => {
    const plain = 'Строка один\n\nСтрока два';
    const out = sanitizeReviewBodyForDisplay(plain);
    expect(out).toMatch(/<p>Строка один<\/p>/);
    expect(out).toMatch(/<p>Строка два<\/p>/);
  });
});
