import { Moeda } from '../api/types';
import { isBlank, isPositiveNumber } from './common';

export interface PedidoItemFormValues {
  /** Chave local estável para uso em `key` de lista — nunca enviada ao backend. */
  chave: string;
  carneId: number | '';
  preco: string;
  moeda: Moeda | '';
}

export interface PedidoFormValues {
  data: string;
  compradorId: number | '';
  itens: PedidoItemFormValues[];
}

export interface PedidoItemFormErrors {
  carneId?: string;
  preco?: string;
  moeda?: string;
}

export interface PedidoFormErrors {
  data?: string;
  compradorId?: string;
  /** Erro geral da lista de itens (ex.: "nenhum item adicionado"). */
  itens?: string;
  /** Erro por linha, no mesmo índice de `PedidoFormValues.itens`. */
  itensDetalhes: PedidoItemFormErrors[];
}

let proximaChaveItem = 0;

/** Gerador simples de chave local (não depende de `crypto.randomUUID`, que nem sempre está disponível em todo ambiente de execução/teste). */
function gerarChaveItem(): string {
  proximaChaveItem += 1;
  return `item-${Date.now()}-${proximaChaveItem}`;
}

export function criarItemPedidoVazio(): PedidoItemFormValues {
  return {
    chave: gerarChaveItem(),
    carneId: '',
    preco: '',
    moeda: '',
  };
}

export const PEDIDO_FORM_INICIAL: PedidoFormValues = {
  data: '',
  compradorId: '',
  itens: [],
};

/**
 * Validação client-side de Pedido, espelhando as regras de negócio do
 * backend (T13): data e comprador obrigatórios, ao menos 1 item, preço de
 * cada item estritamente positivo (reutiliza `isPositiveNumber` — mesma
 * regra usada por qualquer outro campo de preço, sem duplicação).
 */
export function validatePedidoForm(values: PedidoFormValues): PedidoFormErrors {
  const errors: PedidoFormErrors = { itensDetalhes: [] };

  if (isBlank(values.data)) {
    errors.data = 'Data é obrigatória.';
  }

  if (values.compradorId === '') {
    errors.compradorId = 'Comprador é obrigatório.';
  }

  if (values.itens.length === 0) {
    errors.itens = 'Adicione ao menos um item ao pedido.';
  }

  errors.itensDetalhes = values.itens.map((item) => {
    const itemErrors: PedidoItemFormErrors = {};

    if (item.carneId === '') {
      itemErrors.carneId = 'Selecione a carne.';
    }

    if (isBlank(item.preco) || !isPositiveNumber(Number(item.preco))) {
      itemErrors.preco = 'Preço deve ser um número positivo.';
    }

    if (item.moeda === '') {
      itemErrors.moeda = 'Selecione a moeda.';
    }

    return itemErrors;
  });

  return errors;
}

export function isPedidoFormValid(errors: PedidoFormErrors): boolean {
  const semErroGeral = !errors.data && !errors.compradorId && !errors.itens;
  const semErroDeItem = errors.itensDetalhes.every((item) => Object.keys(item).length === 0);

  return semErroGeral && semErroDeItem;
}
