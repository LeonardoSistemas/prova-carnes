import { apiClient } from './client';
import type { CarneDto, CarneResponseDto } from './types';

/** Chamadas HTTP de Carne — único lugar que conhece as rotas `/carnes`. */
export const carnesApi = {
  listar: (): Promise<CarneResponseDto[]> => apiClient.get<CarneResponseDto[]>('/carnes'),

  obterPorId: (id: number): Promise<CarneResponseDto> => apiClient.get<CarneResponseDto>(`/carnes/${id}`),

  criar: (dto: CarneDto): Promise<CarneResponseDto> => apiClient.post<CarneResponseDto>('/carnes', dto),

  atualizar: (id: number, dto: CarneDto): Promise<void> => apiClient.put<void>(`/carnes/${id}`, dto),

  excluir: (id: number): Promise<void> => apiClient.delete(`/carnes/${id}`),
};
