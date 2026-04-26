/**
 * Опциональное сжатие в браузере перед multipart: WebP через canvas, только если размер уменьшается.
 * GIF не трогаем (анимация). При отсутствии поддержки WebP / canvas — возвращается исходный файл.
 */
const DEFAULT_MAX_LONG_EDGE = 2560;
const DEFAULT_QUALITY = 0.82;

export async function maybeCompressImageFileForUpload(
  file: File,
  opts?: { maxLongEdge?: number; quality?: number; enabled?: boolean },
): Promise<File> {
  const enabled = opts?.enabled !== false;
  if (!enabled || !file.type.startsWith('image/')) return file;
  if (file.type === 'image/gif') return file;

  const maxLongEdge = opts?.maxLongEdge ?? DEFAULT_MAX_LONG_EDGE;
  const quality = opts?.quality ?? DEFAULT_QUALITY;

  if (typeof createImageBitmap !== 'function' || typeof document === 'undefined') return file;

  let bitmap: ImageBitmap;
  try {
    bitmap = await createImageBitmap(file);
  } catch {
    return file;
  }

  try {
    let { width, height } = bitmap;
    const long = Math.max(width, height);
    if (long > maxLongEdge) {
      const s = maxLongEdge / long;
      width = Math.max(1, Math.round(width * s));
      height = Math.max(1, Math.round(height * s));
    }

    const canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = height;
    const ctx = canvas.getContext('2d');
    if (!ctx) return file;
    ctx.drawImage(bitmap, 0, 0, width, height);

    const targetType = 'image/webp';
    const blob: Blob | null = await new Promise((resolve) => {
      canvas.toBlob((b) => resolve(b), targetType, quality);
    });
    if (!blob || blob.size >= file.size * 0.95) return file;

    const baseName = file.name.replace(/\.[^.]+$/, '') || 'image';
    return new File([blob], `${baseName}.webp`, { type: targetType, lastModified: Date.now() });
  } finally {
    bitmap.close();
  }
}
