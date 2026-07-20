import { apiClient } from './client';
import type { PedidoDto, PedidoResponseDto } from './types';

/**
 * Filtro combinável (AND) suportado por `GET /pedidos` — espelha os query
 * params opcionais implementados no backend (T35): `compradorId`,
 * `dataInicio` e `dataFim` (ISO). Todos os campos são opcionais; ausência de
 * filtro equivale a listar tudo.
 */
export interface PedidosFiltro {
  compradorId?: number;
  dataInicio?: string;
  dataFim?: string;
}

/** Monta a query string apenas com os campos preenchidos — sem filtro, retorna string vazia (mesmo comportamento de antes desta função existir). */
function montarQueryString(filtro?: PedidosFiltro): string {
  if (!filtro) {
    return '';
  }

  const params = new URLSearchParams();

  if (filtro.compradorId !== undefined) {
    params.set('compradorId', String(filtro.compradorId));
  }
  if (filtro.dataInicio) {
    params.set('dataInicio', filtro.dataInicio);
  }
  if (filtro.dataFim) {
    params.set('dataFim', filtro.dataFim);
  }

  const query = params.toString();
  return query ? `?${query}` : '';
}

/** Chamadas HTTP de Pedido — único lugar que conhece as rotas `/pedidos`. */
export const pedidosApi = {
  listar: (filtro?: PedidosFiltro): Promise<PedidoResponseDto[]> =>
    apiClient.get<PedidoResponseDto[]>(`/pedidos${montarQueryString(filtro)}`),

  obterPorId: (id: number): Promise<PedidoResponseDto> => apiClient.get<PedidoResponseDto>(`/pedidos/${id}`),

  criar: (dto: PedidoDto): Promise<PedidoResponseDto> => apiClient.post<PedidoResponseDto>('/pedidos', dto),

  atualizar: (id: number, dto: PedidoDto): Promise<void> => apiClient.put<void>(`/pedidos/${id}`, dto),

  excluir: (id: number): Promise<void> => apiClient.delete(`/pedidos/${id}`),
};
