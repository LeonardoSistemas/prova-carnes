import { describe, expect, it, beforeEach } from 'vitest';
import { mapPedidoResponseToFormValues } from './mapPedidoResponseToFormValues';
import type { PedidoResponseDto } from '../../api/types';
import { Moeda } from '../../api/types';

describe('mapPedidoResponseToFormValues', () => {
  beforeEach(() => {
    // Resetar o contador global de chaves entre testes
    // Isso é necessário porque a função mantém estado interno
    // Para isso, precisamos reimportar o módulo, mas isso é difícil em vitest
    // Uma alternativa é verificar o comportamento esperado sem depender do valor exato da chave
  });

  it('converte data ISO para formato de input (yyyy-MM-dd)', () => {
    const pedido: PedidoResponseDto = {
      id: 1,
      data: '2026-07-18T00:00:00',
      compradorId: 5,
      itens: [],
      valorTotalEmReal: 0,
    };

    const resultado = mapPedidoResponseToFormValues(pedido);

    expect(resultado.data).toBe('2026-07-18');
  });

  it('preserva o compradorId', () => {
    const pedido: PedidoResponseDto = {
      id: 1,
      data: '2026-07-18T00:00:00',
      compradorId: 42,
      itens: [],
      valorTotalEmReal: 0,
    };

    const resultado = mapPedidoResponseToFormValues(pedido);

    expect(resultado.compradorId).toBe(42);
  });

  it('converte cada item preservando carneId e moeda', () => {
    const pedido: PedidoResponseDto = {
      id: 1,
      data: '2026-07-18T00:00:00',
      compradorId: 5,
      itens: [
        {
          id: 100,
          carneId: 10,
          preco: 150.5,
          moeda: Moeda.USD,
          cotacaoUsada: 5.2,
          valorEmReal: 783.6,
        },
        {
          id: 101,
          carneId: 20,
          preco: 200,
          moeda: Moeda.EUR,
          cotacaoUsada: 5.8,
          valorEmReal: 1160,
        },
      ],
      valorTotalEmReal: 1943.6,
    };

    const resultado = mapPedidoResponseToFormValues(pedido);

    expect(resultado.itens).toHaveLength(2);
    expect(resultado.itens[0].carneId).toBe(10);
    expect(resultado.itens[0].moeda).toBe(Moeda.USD);
    expect(resultado.itens[1].carneId).toBe(20);
    expect(resultado.itens[1].moeda).toBe(Moeda.EUR);
  });

  it('converte preço para string', () => {
    const pedido: PedidoResponseDto = {
      id: 1,
      data: '2026-07-18T00:00:00',
      compradorId: 5,
      itens: [
        {
          id: 100,
          carneId: 10,
          preco: 150.5,
          moeda: Moeda.BRL,
          cotacaoUsada: 1,
          valorEmReal: 150.5,
        },
      ],
      valorTotalEmReal: 150.5,
    };

    const resultado = mapPedidoResponseToFormValues(pedido);

    expect(resultado.itens[0].preco).toBe('150.5');
    expect(typeof resultado.itens[0].preco).toBe('string');
  });

  it('atribui chave única a cada item (não colidem)', () => {
    const pedido: PedidoResponseDto = {
      id: 1,
      data: '2026-07-18T00:00:00',
      compradorId: 5,
      itens: [
        {
          id: 100,
          carneId: 10,
          preco: 150,
          moeda: Moeda.BRL,
          cotacaoUsada: 1,
          valorEmReal: 150,
        },
        {
          id: 101,
          carneId: 20,
          preco: 200,
          moeda: Moeda.BRL,
          cotacaoUsada: 1,
          valorEmReal: 200,
        },
        {
          id: 102,
          carneId: 30,
          preco: 300,
          moeda: Moeda.BRL,
          cotacaoUsada: 1,
          valorEmReal: 300,
        },
      ],
      valorTotalEmReal: 650,
    };

    const resultado = mapPedidoResponseToFormValues(pedido);

    const chaves = resultado.itens.map((item) => item.chave);
    // Verificar que todas as chaves são únicas (usar Set)
    expect(new Set(chaves).size).toBe(chaves.length);
    // Verificar que não estão vazias
    expect(chaves.every((chave) => chave.length > 0)).toBe(true);
  });

  it('lida com pedido sem itens', () => {
    const pedido: PedidoResponseDto = {
      id: 1,
      data: '2026-07-18T00:00:00',
      compradorId: 5,
      itens: [],
      valorTotalEmReal: 0,
    };

    const resultado = mapPedidoResponseToFormValues(pedido);

    expect(resultado.itens).toEqual([]);
  });

  it('converte data ISO com hora para apenas a data', () => {
    const pedido: PedidoResponseDto = {
      id: 1,
      data: '2026-07-18T14:30:00',
      compradorId: 5,
      itens: [],
      valorTotalEmReal: 0,
    };

    const resultado = mapPedidoResponseToFormValues(pedido);

    expect(resultado.data).toBe('2026-07-18');
  });
});
