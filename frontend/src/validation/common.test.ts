import { describe, expect, it } from 'vitest';
import { isBlank, isPositiveNumber } from './common';

describe('isBlank', () => {
  it('retorna true para string vazia ou só com espaços', () => {
    expect(isBlank('')).toBe(true);
    expect(isBlank('   ')).toBe(true);
  });

  it('retorna false para string com conteúdo', () => {
    expect(isBlank('Picanha')).toBe(false);
  });
});

describe('isPositiveNumber', () => {
  it.each([0, -1, -0.01, NaN, Infinity])('rejeita %s', (valor) => {
    expect(isPositiveNumber(valor)).toBe(false);
  });

  it.each([0.01, 1, 100.5])('aceita %s', (valor) => {
    expect(isPositiveNumber(valor)).toBe(true);
  });
});
