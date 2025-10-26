export function arrayBufferToBase64(buffer: Array<number> | string | undefined | null): string {
  if (!buffer) {
    return '';
  }

  if (typeof buffer === 'string') {
    return buffer;
  }

  try {
    let binary = '';
    const bytes = new Uint8Array(buffer);
    const len = bytes.byteLength;
    for (let i = 0; i < len; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
  } catch (error) {
    console.error('Ошибка при преобразовании ArrayBuffer в Base64:', error);
    return '';
  }
} 