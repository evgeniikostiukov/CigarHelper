import { describe, expect, it } from 'vitest';
import { MM_PER_INCH, convertLengthInputOnUnitChange, lengthInputFromMm, lengthMmFromInput } from './cigarLengthUnit';

describe('cigarLengthUnit', () => {
  it('lengthMmFromInput rounds mm and inches to stored mm', () => {
    expect(lengthMmFromInput(152, 'mm')).toBe(152);
    expect(lengthMmFromInput(6, 'in')).toBe(Math.round(6 * MM_PER_INCH));
  });

  it('lengthInputFromMm inverts for display', () => {
    expect(lengthInputFromMm(152, 'mm')).toBe(152);
    const inches = lengthInputFromMm(Math.round(6 * MM_PER_INCH), 'in');
    expect(inches).toBeCloseTo(6, 1);
  });

  it('convertLengthInputOnUnitChange preserves physical length', () => {
    const mm = 165;
    const inDisplay = lengthInputFromMm(mm, 'in')!;
    const backToMm = lengthMmFromInput(inDisplay, 'in');
    expect(backToMm).toBe(mm);
    const asMmDisplay = convertLengthInputOnUnitChange(inDisplay, 'in', 'mm');
    expect(asMmDisplay).toBe(mm);
  });
});
