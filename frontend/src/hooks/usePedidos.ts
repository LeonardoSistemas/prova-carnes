import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { pedidosApi, type PedidosFiltro } from '../api/pedidosApi';
import type { PedidoDto } from '../api/types';

const PEDIDOS_QUERY_KEY = ['pedidos'] as const;

/**
 * `filtro` entra na `queryKey` para que cada combinação de
 * comprador/data tenha seu próprio cache e trocar o filtro dispare um novo
 * fetch (T36). `invalidateQueries({ queryKey: PEDIDOS_QUERY_KEY })` nas
 * mutations abaixo continua funcionando: TanStack Query invalida por prefixo.
 *
 * O segmento `'lista'`/`'detalhe'` é essencial: sem ele, `usePedidos(undefined)`
 * (sem filtro ativo) e `usePedido(undefined)` (modo "Novo pedido") colapsam
 * para a mesma chave `['pedidos', undefined]`, e o cache da listagem vaza
 * para o hook de detalhe mesmo com `enabled: false` (bug relatado — ver T41).
 */
export function usePedidos(filtro?: PedidosFiltro) {
  return useQuery({
    queryKey: [...PEDIDOS_QUERY_KEY, 'lista', filtro],
    queryFn: () => pedidosApi.listar(filtro),
  });
}

export function usePedido(id: number | undefined) {
  return useQuery({
    queryKey: [...PEDIDOS_QUERY_KEY, 'detalhe', id],
    queryFn: () => pedidosApi.obterPorId(id as number),
    enabled: id !== undefined,
  });
}

export function useCreatePedido() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: PedidoDto) => pedidosApi.criar(dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: PEDIDOS_QUERY_KEY }),
  });
}

export function useUpdatePedido() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: PedidoDto }) => pedidosApi.atualizar(id, dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: PEDIDOS_QUERY_KEY }),
  });
}

export function useDeletePedido() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => pedidosApi.excluir(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: PEDIDOS_QUERY_KEY }),
  });
}
