import { describe, expect, it } from 'vitest';
import { Moeda } from '../api/types';
import { criarItemPedidoVazio, isPedidoFormValid, validatePedidoForm, type PedidoFormValues } from './validatePedidoForm';

function pedidoValidoComUmItem(): PedidoFormValues {
  return {
    data: '2026-07-18',
    compradorId: 1,
    itens: [{ ...criarItemPedidoVazio(), carneId: 1, preco: '50', moeda: Moeda.BRL }],
  };
}

describe('validatePedidoForm', () => {
  it('exige data preenchida', () => {
    const errors = validatePedidoForm({ ...pedidoValidoComUmItem(), data: '' });

    expect(errors.data).toBe('Data é obrigatória.');
  });

  it('exige comprador selecionado', () => {
    const errors = validatePedidoForm({ ...pedidoValidoComUmItem(), compradorId: '' });

    expect(errors.compradorId).toBe('Comprador é obrigatório.');
  });

  it('exige ao menos um item na lista', () => {
    const errors = validatePedidoForm({ ...pedidoValidoComUmItem(), itens: [] });

    expect(errors.itens).toBe('Adicione ao menos um item ao pedido.');
    expect(isPedidoFormValid(errors)).toBe(false);
  });

  it.each(['0', '-10', 'abc', ''])('rejeita preço não positivo (%s)', (precoInvalido) => {
    const pedido = pedidoValidoComUmItem();
    pedido.itens[0].preco = precoInvalido;

    const errors = validatePedidoForm(pedido);

    expect(errors.itensDetalhes[0].preco).toBe('Preço deve ser um número positivo.');
    expect(isPedidoFormValid(errors)).toBe(false);
  });

  it('aceita preço positivo com casas decimais', () => {
    const pedido = pedidoValidoComUmItem();
    pedido.itens[0].preco = '19.90';

    const errors = validatePedidoForm(pedido);

    expect(errors.itensDetalhes[0].preco).toBeUndefined();
  });

  it('exige carne selecionada em cada item', () => {
    const pedido = pedidoValidoComUmItem();
    pedido.itens[0].carneId = '';

    const errors = validatePedidoForm(pedido);

    expect(errors.itensDetalhes[0].carneId).toBe('Selecione a carne.');
  });

  it('exige moeda selecionada em cada item', () => {
    const pedido = pedidoValidoComUmItem();
    pedido.itens[0].moeda = '';

    const errors = validatePedidoForm(pedido);

    expect(errors.itensDetalhes[0].moeda).toBe('Selecione a moeda.');
  });

  it('não retorna erros quando o pedido é válido', () => {
    const errors = validatePedidoForm(pedidoValidoComUmItem());

    expect(isPedidoFormValid(errors)).toBe(true);
  });

  it('valida múltiplos itens de forma independente (só o item inválido acusa erro)', () => {
    const pedido = pedidoValidoComUmItem();
    pedido.itens.push({ ...criarItemPedidoVazio(), carneId: 2, preco: '-5', moeda: Moeda.USD });

    const errors = validatePedidoForm(pedido);

    expect(errors.itensDetalhes[0]).toEqual({});
    expect(errors.itensDetalhes[1].preco).toBe('Preço deve ser um número positivo.');
    expect(isPedidoFormValid(errors)).toBe(false);
  });
});
