import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { compradoresApi } from '../api/compradoresApi';
import type { CompradorDto } from '../api/types';

const COMPRADORES_QUERY_KEY = ['compradores'] as const;

export function useCompradores() {
  return useQuery({ queryKey: COMPRADORES_QUERY_KEY, queryFn: compradoresApi.listar });
}

export function useComprador(id: number | undefined) {
  return useQuery({
    queryKey: [...COMPRADORES_QUERY_KEY, 'detalhe', id],
    queryFn: () => compradoresApi.obterPorId(id as number),
    enabled: id !== undefined,
  });
}

export function useCreateComprador() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: CompradorDto) => compradoresApi.criar(dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: COMPRADORES_QUERY_KEY }),
  });
}

export function useUpdateComprador() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: CompradorDto }) => compradoresApi.atualizar(id, dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: COMPRADORES_QUERY_KEY }),
  });
}

export function useDeleteComprador() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => compradoresApi.excluir(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: COMPRADORES_QUERY_KEY }),
  });
}
