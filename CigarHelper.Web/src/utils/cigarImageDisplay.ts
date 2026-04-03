import type { CigarImage } from '@/services/cigarService';
import { arrayBufferToBase64 } from '@/utils/imageUtils';

export function cigarImageRawBytes(img: CigarImage | undefined): string | number[] | undefined {
  if (!img) {
    return undefined;
  }
  return img.imageData ?? img.data;
}

/** Data-URL из полей DTO (без запроса к API). */
export function cigarImageInlineDataSrc(img: CigarImage | undefined): string {
  if (!img) {
    return '';
  }
  const raw = cigarImageRawBytes(img);
  const hasBytes = raw != null && (typeof raw === 'string' ? raw.length > 0 : raw.length > 0);

  if (hasBytes && typeof raw === 'string') {
    if (raw.startsWith('data:')) {
      return raw;
    }
    const mime = (img.contentType ?? '').trim();
    const safeMime = mime.startsWith('image/') ? mime : 'image/jpeg';
    return `data:${safeMime};base64,${raw}`;
  }

  if (hasBytes && typeof raw !== 'string') {
    const b64 = arrayBufferToBase64(raw);
    if (b64) {
      const mime = (img.contentType ?? '').trim();
      const safeMime = mime.startsWith('image/') ? mime : 'image/jpeg';
      return `data:${safeMime};base64,${b64}`;
    }
  }

  return '';
}

/** Главное первым, затем по id (для карусели коллекции). */
export function orderUserCigarGalleryImages(images: CigarImage[] | undefined | null): CigarImage[] {
  const list = images?.length ? [...images] : [];
  return list.sort((a, b) => {
    if (a.isMain !== b.isMain) {
      return a.isMain ? -1 : 1;
    }
    return a.id - b.id;
  });
}
