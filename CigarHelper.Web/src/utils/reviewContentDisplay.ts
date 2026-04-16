import DOMPurify from 'dompurify';
import type { Config } from 'dompurify';

/** Типичный вывод TipTap StarterKit и окружения «документ». */
const LOOKS_LIKE_HTML_FRAGMENT =
  /<\s*(?:p|div|br|h[1-6]|ul|ol|li|strong|em|b|i|u|s|blockquote|pre|code|a|img|span|hr)\b/i;

const PURIFY_REVIEW_BODY = {
  USE_PROFILES: { html: true },
  ADD_TAGS: ['img'],
  ADD_ATTR: ['href', 'target', 'rel', 'src', 'alt', 'title', 'width', 'height', 'class', 'loading', 'decoding'],
  FORBID_TAGS: [
    'script',
    'style',
    'iframe',
    'object',
    'embed',
    'form',
    'input',
    'textarea',
    'select',
    'button',
    'base',
    'link',
    'meta',
    'template',
  ],
  FORBID_ATTR: [/^on/i],
} as unknown as Config;

/**
 * Разбирает строку как HTML и возвращает innerHTML тела документа
 * (убирает лишние `<html>` / `<body>`, нормализует фрагмент).
 */
export function extractBodyInnerHtml(raw: string): string {
  const trimmed = raw.trim();
  if (!trimmed) return '';
  const doc = new DOMParser().parseFromString(trimmed, 'text/html');
  return (doc.body?.innerHTML ?? '').trim();
}

function escapeHtmlText(s: string): string {
  return s
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

/** Текст для превью карточек: без тегов, без лишних пробелов (в т.ч. если пришёл «документ»). */
export function stripReviewHtmlToPlainText(raw: string): string {
  const trimmed = raw.trim();
  if (!trimmed) return '';
  const doc = new DOMParser().parseFromString(trimmed, 'text/html');
  const text = doc.body?.textContent ?? '';
  return text.replace(/\s+/g, ' ').trim();
}

/**
 * Короткий отрывок для списка обзоров (summary с бэка может быть усечённым HTML).
 */
export function plainTextReviewExcerpt(raw: string, maxChars = 220): string {
  const plain = stripReviewHtmlToPlainText(raw);
  if (!plain) return '';
  const ellipsis = '…';
  if (plain.length <= maxChars) return plain;
  const budget = maxChars - ellipsis.length;
  if (budget < 1) return ellipsis.slice(0, maxChars);
  const cut = plain.slice(0, budget).trimEnd();
  const lastSpace = cut.lastIndexOf(' ');
  const base = lastSpace > budget * 0.5 ? cut.slice(0, lastSpace).trimEnd() : cut;
  const trimmed = base.length > budget ? base.slice(0, budget).trimEnd() : base;
  return `${trimmed}${ellipsis}`;
}

function plainTextToSanitizedParagraphs(text: string): string {
  const lines = text
    .split(/\r?\n/)
    .map((l) => l.trim())
    .filter(Boolean);
  if (lines.length === 0) return '';
  const html = lines.map((line) => `<p>${escapeHtmlText(line)}</p>`).join('');
  return String(DOMPurify.sanitize(html, PURIFY_REVIEW_BODY));
}

/**
 * HTML для блока «Текст обзора»: TipTap-HTML или плейнтекст; без дублирования оболочки документа.
 */
export function sanitizeReviewBodyForDisplay(raw: string): string {
  const fragment = extractBodyInnerHtml(raw);
  if (!fragment) return '';

  if (!LOOKS_LIKE_HTML_FRAGMENT.test(fragment)) {
    return plainTextToSanitizedParagraphs(fragment);
  }

  return String(DOMPurify.sanitize(fragment, PURIFY_REVIEW_BODY));
}
