import { describe, expect, it } from 'vitest';
import {
  PEDIDO_FILTRO_INICIAL,
  intervaloDeDatasInvalido,
  paraFiltroApi,
  type PedidoFiltroFormValues,
} from './validatePedidoFiltro';

describe('intervaloDeDatasInvalido', () => {
  it('retorna false quando nenhuma data está preenchida', () => {
    expect(intervaloDeDatasInvalido(PEDIDO_FILTRO_INICIAL)).toBe(false);
  });

  it('retorna false quando só uma das datas está preenchida', () => {
    const filtro: PedidoFiltroFormValues = { ...PEDIDO_FILTRO_INICIAL, dataInicio: '2026-07-01' };
    expect(intervaloDeDatasInvalido(filtro)).toBe(false);
  });

  it('retorna false quando dataInicio é anterior ou igual a dataFim', () => {
    const filtro: PedidoFiltroFormValues = {
      ...PEDIDO_FILTRO_INICIAL,
      dataInicio: '2026-07-01',
      dataFim: '2026-07-10',
    };
    expect(intervaloDeDatasInvalido(filtro)).toBe(false);

    const filtroIgual: PedidoFiltroFormValues = {
      ...PEDIDO_FILTRO_INICIAL,
      dataInicio: '2026-07-01',
      dataFim: '2026-07-01',
    };
    expect(intervaloDeDatasInvalido(filtroIgual)).toBe(false);
  });

  it('retorna true quando dataInicio é posterior a dataFim', () => {
    const filtro: PedidoFiltroFormValues = {
      ...PEDIDO_FILTRO_INICIAL,
      dataInicio: '2026-07-20',
      dataFim: '2026-07-10',
    };
    expect(intervaloDeDatasInvalido(filtro)).toBe(true);
  });
});

describe('paraFiltroApi', () => {
  it('retorna undefined quando nenhum campo está preenchido (sem filtro = GET /pedidos original)', () => {
    expect(paraFiltroApi(PEDIDO_FILTRO_INICIAL)).toBeUndefined();
  });

  it('inclui apenas os campos preenchidos', () => {
    const filtro: PedidoFiltroFormValues = { ...PEDIDO_FILTRO_INICIAL, compradorId: 5 };
    expect(paraFiltroApi(filtro)).toEqual({ compradorId: 5 });
  });

  it('inclui compradorId e datas quando todos preenchidos', () => {
    const filtro: PedidoFiltroFormValues = {
      compradorId: 5,
      dataInicio: '2026-07-01',
      dataFim: '2026-07-10',
    };
    expect(paraFiltroApi(filtro)).toEqual({
      compradorId: 5,
      dataInicio: '2026-07-01',
      dataFim: '2026-07-10',
    });
  });
});
