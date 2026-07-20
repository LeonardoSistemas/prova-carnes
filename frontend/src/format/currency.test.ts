import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { formatarDataBR } from './currency';

describe('formatarDataBR', () => {
  beforeEach(() => {
    // Simula um navegador num timezone com offset positivo em relação a UTC
    // (UTC+2) — é o cenário onde `new Date(isoDate)` interpretado como hora
    // local (em vez de UTC) faria a data exibida recuar um dia.
    vi.stubEnv('TZ', 'Europe/Berlin');
  });

  afterEach(() => {
    vi.unstubAllEnvs();
  });

  it('não recua um dia quando o navegador está em timezone com offset positivo', () => {
    expect(formatarDataBR('2026-07-18T00:00:00')).toBe('18/07/2026');
  });

  it('funciona também para string somente-data', () => {
    expect(formatarDataBR('2026-07-18')).toBe('18/07/2026');
  });

  it('retorna a string original quando o valor não é uma data válida', () => {
    expect(formatarDataBR('não-é-data')).toBe('não-é-data');
  });
});
