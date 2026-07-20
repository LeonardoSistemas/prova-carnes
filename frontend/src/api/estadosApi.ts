import { apiClient } from './client';
import type { EstadoComCidadesDto } from './types';

/** Chamada HTTP de Estado/Cidade — somente leitura, alimenta o combobox em cascata. */
export const estadosApi = {
  listar: (): Promise<EstadoComCidadesDto[]> => apiClient.get<EstadoComCidadesDto[]>('/estados'),
};
