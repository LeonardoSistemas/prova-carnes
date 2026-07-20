import { describe, expect, it } from 'vitest';
import { encontrarNomeCidade, encontrarEstadoDaCidade } from './enderecoLookup';
import type { EstadoComCidadesDto } from '../api/types';

const ESTADOS_MOCK: EstadoComCidadesDto[] = [
  {
    id: 1,
    nome: 'São Paulo',
    uf: 'SP',
    cidades: [
      { id: 1, nome: 'São Paulo', estadoId: 1 },
      { id: 2, nome: 'Campinas', estadoId: 1 },
      { id: 3, nome: 'Santos', estadoId: 1 },
    ],
  },
  {
    id: 2,
    nome: 'Rio de Janeiro',
    uf: 'RJ',
    cidades: [
      { id: 4, nome: 'Rio de Janeiro', estadoId: 2 },
      { id: 5, nome: 'Niterói', estadoId: 2 },
    ],
  },
  {
    id: 3,
    nome: 'Minas Gerais',
    uf: 'MG',
    cidades: [
      { id: 6, nome: 'Belo Horizonte', estadoId: 3 },
    ],
  },
];

describe('encontrarNomeCidade', () => {
  it('retorna nome e UF da cidade quando encontrada', () => {
    const resultado = encontrarNomeCidade(ESTADOS_MOCK, 1);
    expect(resultado).toBe('São Paulo/SP');
  });

  it('retorna nome e UF de cidades diferentes', () => {
    expect(encontrarNomeCidade(ESTADOS_MOCK, 2)).toBe('Campinas/SP');
    expect(encontrarNomeCidade(ESTADOS_MOCK, 5)).toBe('Niterói/RJ');
    expect(encontrarNomeCidade(ESTADOS_MOCK, 6)).toBe('Belo Horizonte/MG');
  });

  it('retorna fallback "Cidade #id" quando cidade não é encontrada', () => {
    expect(encontrarNomeCidade(ESTADOS_MOCK, 999)).toBe('Cidade #999');
  });

  it('retorna fallback "Cidade #id" para ID negativo', () => {
    expect(encontrarNomeCidade(ESTADOS_MOCK, -1)).toBe('Cidade #-1');
  });

  it('retorna fallback "Cidade #id" em lista vazia de estados', () => {
    expect(encontrarNomeCidade([], 1)).toBe('Cidade #1');
  });

  it('retorna fallback para ID zero', () => {
    expect(encontrarNomeCidade(ESTADOS_MOCK, 0)).toBe('Cidade #0');
  });
});

describe('encontrarEstadoDaCidade', () => {
  it('retorna o estado quando a cidade é encontrada', () => {
    const resultado = encontrarEstadoDaCidade(ESTADOS_MOCK, 1);
    expect(resultado?.uf).toBe('SP');
    expect(resultado?.nome).toBe('São Paulo');
    expect(resultado?.id).toBe(1);
  });

  it('encontra o estado correto para cidades diferentes', () => {
    expect(encontrarEstadoDaCidade(ESTADOS_MOCK, 4)?.uf).toBe('RJ');
    expect(encontrarEstadoDaCidade(ESTADOS_MOCK, 6)?.uf).toBe('MG');
  });

  it('retorna undefined quando cidade não é encontrada', () => {
    const resultado = encontrarEstadoDaCidade(ESTADOS_MOCK, 999);
    expect(resultado).toBeUndefined();
  });

  it('retorna undefined em lista vazia de estados', () => {
    const resultado = encontrarEstadoDaCidade([], 1);
    expect(resultado).toBeUndefined();
  });

  it('retorna undefined para ID negativo', () => {
    const resultado = encontrarEstadoDaCidade(ESTADOS_MOCK, -1);
    expect(resultado).toBeUndefined();
  });

  it('retorna o estado completo com todas as cidades', () => {
    const resultado = encontrarEstadoDaCidade(ESTADOS_MOCK, 2);
    expect(resultado?.cidades).toHaveLength(3);
    expect(resultado?.cidades[0].nome).toBe('São Paulo');
    expect(resultado?.cidades[1].nome).toBe('Campinas');
    expect(resultado?.cidades[2].nome).toBe('Santos');
  });
});
