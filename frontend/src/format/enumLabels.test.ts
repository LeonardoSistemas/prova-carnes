import { describe, expect, it } from 'vitest';
import {
  ORIGEM_CARNE_LABELS,
  MOEDA_LABELS,
  ORIGEM_CARNE_OPCOES,
  MOEDA_OPCOES,
} from './enumLabels';
import { OrigemCarne, Moeda } from '../api/types';

describe('ORIGEM_CARNE_LABELS', () => {
  it('mapeia OrigemCarne.Bovina para "Bovina"', () => {
    expect(ORIGEM_CARNE_LABELS[OrigemCarne.Bovina]).toBe('Bovina');
  });

  it('mapeia OrigemCarne.Suina para "Suína"', () => {
    expect(ORIGEM_CARNE_LABELS[OrigemCarne.Suina]).toBe('Suína');
  });

  it('mapeia OrigemCarne.Aves para "Aves"', () => {
    expect(ORIGEM_CARNE_LABELS[OrigemCarne.Aves]).toBe('Aves');
  });

  it('mapeia OrigemCarne.Peixes para "Peixes"', () => {
    expect(ORIGEM_CARNE_LABELS[OrigemCarne.Peixes]).toBe('Peixes');
  });

  it('contém exatamente 4 rótulos', () => {
    expect(Object.keys(ORIGEM_CARNE_LABELS)).toHaveLength(4);
  });
});

describe('MOEDA_LABELS', () => {
  it('mapeia Moeda.BRL para "BRL - Real"', () => {
    expect(MOEDA_LABELS[Moeda.BRL]).toBe('BRL - Real');
  });

  it('mapeia Moeda.USD para "USD - Dólar"', () => {
    expect(MOEDA_LABELS[Moeda.USD]).toBe('USD - Dólar');
  });

  it('mapeia Moeda.EUR para "EUR - Euro"', () => {
    expect(MOEDA_LABELS[Moeda.EUR]).toBe('EUR - Euro');
  });

  it('contém exatamente 3 rótulos', () => {
    expect(Object.keys(MOEDA_LABELS)).toHaveLength(3);
  });
});

describe('ORIGEM_CARNE_OPCOES', () => {
  it('contém exatamente 4 valores numéricos', () => {
    expect(ORIGEM_CARNE_OPCOES).toHaveLength(4);
  });

  it('contém todos os valores do enum OrigemCarne', () => {
    expect(ORIGEM_CARNE_OPCOES).toContain(OrigemCarne.Bovina);
    expect(ORIGEM_CARNE_OPCOES).toContain(OrigemCarne.Suina);
    expect(ORIGEM_CARNE_OPCOES).toContain(OrigemCarne.Aves);
    expect(ORIGEM_CARNE_OPCOES).toContain(OrigemCarne.Peixes);
  });

  it('contém apenas números', () => {
    expect(ORIGEM_CARNE_OPCOES.every((valor) => typeof valor === 'number')).toBe(true);
  });

  it('não contém duplicatas', () => {
    const conjunto = new Set(ORIGEM_CARNE_OPCOES);
    expect(conjunto.size).toBe(ORIGEM_CARNE_OPCOES.length);
  });
});

describe('MOEDA_OPCOES', () => {
  it('contém exatamente 3 valores numéricos', () => {
    expect(MOEDA_OPCOES).toHaveLength(3);
  });

  it('contém todos os valores do enum Moeda', () => {
    expect(MOEDA_OPCOES).toContain(Moeda.BRL);
    expect(MOEDA_OPCOES).toContain(Moeda.USD);
    expect(MOEDA_OPCOES).toContain(Moeda.EUR);
  });

  it('contém apenas números', () => {
    expect(MOEDA_OPCOES.every((valor) => typeof valor === 'number')).toBe(true);
  });

  it('não contém duplicatas', () => {
    const conjunto = new Set(MOEDA_OPCOES);
    expect(conjunto.size).toBe(MOEDA_OPCOES.length);
  });
});

describe('Integração entre labels e opcoes', () => {
  it('ORIGEM_CARNE_LABELS tem entrada para cada valor em ORIGEM_CARNE_OPCOES', () => {
    ORIGEM_CARNE_OPCOES.forEach((valor) => {
      expect(ORIGEM_CARNE_LABELS[valor]).toBeDefined();
      expect(typeof ORIGEM_CARNE_LABELS[valor]).toBe('string');
      expect(ORIGEM_CARNE_LABELS[valor].length).toBeGreaterThan(0);
    });
  });

  it('MOEDA_LABELS tem entrada para cada valor em MOEDA_OPCOES', () => {
    MOEDA_OPCOES.forEach((valor) => {
      expect(MOEDA_LABELS[valor]).toBeDefined();
      expect(typeof MOEDA_LABELS[valor]).toBe('string');
      expect(MOEDA_LABELS[valor].length).toBeGreaterThan(0);
    });
  });
});
