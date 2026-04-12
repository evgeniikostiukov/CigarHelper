/** Единица ввода длины сигары; в API и БД всегда миллиметры. */
export type CigarLengthUnit = 'mm' | 'in';

export const MM_PER_INCH = 25.4;

const STORAGE_KEY = 'cigarHelper.cigarLengthUnit';

export const lengthUnitSelectOptions: { label: string; value: CigarLengthUnit }[] = [
  { label: 'мм', value: 'mm' },
  { label: 'дюймы', value: 'in' },
];

export function readStoredLengthUnit(): CigarLengthUnit {
  if (typeof localStorage === 'undefined') return 'mm';
  return localStorage.getItem(STORAGE_KEY) === 'in' ? 'in' : 'mm';
}

export function writeStoredLengthUnit(unit: CigarLengthUnit): void {
  if (typeof localStorage === 'undefined') return;
  localStorage.setItem(STORAGE_KEY, unit);
}

/** Сохраняемое в БД значение длины (мм) из поля ввода и выбранной единицы. */
export function lengthMmFromInput(value: number | null | undefined, unit: CigarLengthUnit): number | null {
  if (value == null || !Number.isFinite(value) || value <= 0) return null;
  if (unit === 'mm') return Math.round(value);
  return Math.round(value * MM_PER_INCH);
}

/** Значение для поля ввода при известной длине из API (мм). */
export function lengthInputFromMm(lengthMm: number | null | undefined, unit: CigarLengthUnit): number | null {
  if (lengthMm == null) return null;
  if (unit === 'mm') return lengthMm;
  return Math.round((lengthMm / MM_PER_INCH) * 100) / 100;
}

/** При смене единицы пересчитать число в поле, сохраняя физическую длину. */
export function convertLengthInputOnUnitChange(
  value: number | null | undefined,
  from: CigarLengthUnit,
  to: CigarLengthUnit,
): number | null {
  if (from === to) return value ?? null;
  if (value == null || !Number.isFinite(value) || value <= 0) return null;
  const mm = from === 'mm' ? Math.round(value) : Math.round(value * MM_PER_INCH);
  return lengthInputFromMm(mm, to);
}
