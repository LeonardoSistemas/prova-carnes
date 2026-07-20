import { useQuery } from '@tanstack/react-query';
import { dashboardApi } from '../api/dashboardApi';
import type { PeriodoDashboard } from '../api/types';

/**
 * Namespace próprio (`['dashboard', ...]`), nunca reaproveitando
 * `['pedidos', ...]` ou de outra entidade — lição da T41 (ver `usePedidos.ts`):
 * chaves colidindo entre hooks diferentes vazam cache de um para o outro.
 */
const DASHBOARD_QUERY_KEY = ['dashboard'] as const;

/** Resumo + top carnes/compradores do período informado (T62: `GET /api/dashboard`). */
export function useDashboardResumo(periodo: PeriodoDashboard) {
  return useQuery({
    queryKey: [...DASHBOARD_QUERY_KEY, 'resumo', periodo],
    queryFn: () => dashboardApi.obterResumo(periodo),
  });
}

/** Série de faturamento diário dos últimos `dias` dias (T62: `GET /api/dashboard/faturamento-por-dia`). */
export function useFaturamentoPorDia(dias: number) {
  return useQuery({
    queryKey: [...DASHBOARD_QUERY_KEY, 'faturamento-por-dia', dias],
    queryFn: () => dashboardApi.obterFaturamentoPorDia(dias),
  });
}
