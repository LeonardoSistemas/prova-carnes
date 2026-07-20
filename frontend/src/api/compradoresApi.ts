import { apiClient } from './client';
import type { CompradorDto, CompradorResponseDto } from './types';

/** Chamadas HTTP de Comprador — único lugar que conhece as rotas `/compradores`. */
export const compradoresApi = {
  listar: (): Promise<CompradorResponseDto[]> => apiClient.get<CompradorResponseDto[]>('/compradores'),

  obterPorId: (id: number): Promise<CompradorResponseDto> =>
    apiClient.get<CompradorResponseDto>(`/compradores/${id}`),

  criar: (dto: CompradorDto): Promise<CompradorResponseDto> =>
    apiClient.post<CompradorResponseDto>('/compradores', dto),

  atualizar: (id: number, dto: CompradorDto): Promise<void> => apiClient.put<void>(`/compradores/${id}`, dto),

  excluir: (id: number): Promise<void> => apiClient.delete(`/compradores/${id}`),
};
