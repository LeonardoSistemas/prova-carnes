import { useQuery } from '@tanstack/react-query';
import { estadosApi } from '../api/estadosApi';

export function useEstados() {
  return useQuery({ queryKey: ['estados'], queryFn: estadosApi.listar });
}
