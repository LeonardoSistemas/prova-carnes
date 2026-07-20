import type { PedidosFiltro } from '../api/pedidosApi';
import { isBlank } from './common';

/**
 * Estado controlado do formulário de filtro da listagem de Pedidos (T36).
 * Usa `''` como sentinela de "não selecionado" para `compradorId`, mesmo
 * padrão já usado em `validatePedidoForm.ts`.
 */
export interface PedidoFiltroFormValues {
  compradorId: number | '';
  dataInicio: string;
  dataFim: string;
}

export const PEDIDO_FILTRO_INICIAL: PedidoFiltroFormValues = {
  compradorId: '',
  dataInicio: '',
  dataFim: '',
};

/**
 * `true` quando as duas datas estão preenchidas e `dataInicio` é posterior a
 * `dataFim` — combinação que o backend aceita (AND dos filtros) mas que
 * sempre resulta em lista vazia, sem explicar o motivo ao usuário. A UI usa
 * este sinal para mostrar uma mensagem inline em vez de deixar o usuário
 * imaginando por que a tabela ficou vazia.
 */
export function intervaloDeDatasInvalido(filtro: PedidoFiltroFormValues): boolean {
  if (isBlank(filtro.dataInicio) || isBlank(filtro.dataFim)) {
    return false;
  }

  return filtro.dataInicio > filtro.dataFim;
}

/**
 * Converte o estado do formulário (com sentinelas `''`) para o shape aceito
 * por `pedidosApi.listar`/`usePedidos`, incluindo apenas os campos
 * preenchidos. Retorna `undefined` quando nenhum campo está preenchido, para
 * que "sem filtro" produza exatamente o mesmo request de antes desta feature
 * (`GET /pedidos` sem query string).
 */
export function paraFiltroApi(filtro: PedidoFiltroFormValues): PedidosFiltro | undefined {
  const resultado: PedidosFiltro = {};

  if (filtro.compradorId !== '') {
    resultado.compradorId = filtro.compradorId;
  }
  if (!isBlank(filtro.dataInicio)) {
    resultado.dataInicio = filtro.dataInicio;
  }
  if (!isBlank(filtro.dataFim)) {
    resultado.dataFim = filtro.dataFim;
  }

  return Object.keys(resultado).length === 0 ? undefined : resultado;
}
