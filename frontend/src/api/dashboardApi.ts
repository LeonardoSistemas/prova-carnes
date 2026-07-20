import { apiClient } from './client';
import type { DashboardDto, FaturamentoPorDiaDto, PeriodoDashboard } from './types';

/** Chamadas HTTP de Dashboard — único lugar que conhece as rotas `/dashboard`. */
export const dashboardApi = {
  obterResumo: (periodo: PeriodoDashboard): Promise<DashboardDto> =>
    apiClient.get<DashboardDto>(`/dashboard?periodo=${periodo}`),

  obterFaturamentoPorDia: (dias: number): Promise<FaturamentoPorDiaDto[]> =>
    apiClient.get<FaturamentoPorDiaDto[]>(`/dashboard/faturamento-por-dia?dias=${dias}`),
};
