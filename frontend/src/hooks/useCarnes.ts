import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { carnesApi } from '../api/carnesApi';
import type { CarneDto } from '../api/types';

const CARNES_QUERY_KEY = ['carnes'] as const;

export function useCarnes() {
  return useQuery({ queryKey: CARNES_QUERY_KEY, queryFn: carnesApi.listar });
}

export function useCarne(id: number | undefined) {
  return useQuery({
    queryKey: [...CARNES_QUERY_KEY, 'detalhe', id],
    queryFn: () => carnesApi.obterPorId(id as number),
    enabled: id !== undefined,
  });
}

export function useCreateCarne() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: CarneDto) => carnesApi.criar(dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CARNES_QUERY_KEY }),
  });
}

export function useUpdateCarne() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: CarneDto }) => carnesApi.atualizar(id, dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CARNES_QUERY_KEY }),
  });
}

export function useDeleteCarne() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => carnesApi.excluir(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CARNES_QUERY_KEY }),
  });
}
