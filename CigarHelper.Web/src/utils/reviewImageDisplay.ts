type ReviewImageLike = {
  imageData?: string | null;
  imageBytes?: string | null;
  contentType?: string | null;
};

function safeInlineMime(contentType: string | null | undefined): string {
  const mime = (contentType ?? '').trim();
  return mime.startsWith('image/') ? mime : 'image/jpeg';
}

/** Data-URL из полей DTO (без запроса к API). */
export function reviewImageInlineDataSrc(img: ReviewImageLike | undefined | null): string {
  if (!img) return '';

  const raw = img.imageData ?? img.imageBytes;
  if (!raw) return '';

  if (raw.startsWith('data:')) {
    return raw;
  }

  return `data:${safeInlineMime(img.contentType)};base64,${raw}`;
}
