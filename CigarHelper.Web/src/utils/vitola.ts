/** Текст для UI: «длина × кольцо», если заданы оба числа. */
export function formatVitola(lengthMm: number | null | undefined, diameter: number | null | undefined): string {
  if (lengthMm != null && diameter != null) return `${lengthMm} × ${diameter}`;
  return '';
}
