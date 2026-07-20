import type { EstadoComCidadesDto } from '../api/types';

/**
 * Funções puras de busca em Estado→Cidade, reutilizadas pelo formulário de
 * Comprador (cascata) e pela listagem (exibição do nome da cidade/estado a
 * partir de `cidadeId`, já que `CompradorResponseDto` só traz o id).
 */

export function encontrarEstadoDaCidade(
  estados: EstadoComCidadesDto[],
  cidadeId: number,
): EstadoComCidadesDto | undefined {
  return estados.find((estado) => estado.cidades.some((cidade) => cidade.id === cidadeId));
}

export function encontrarNomeCidade(estados: EstadoComCidadesDto[], cidadeId: number): string {
  for (const estado of estados) {
    const cidade = estado.cidades.find((candidata) => candidata.id === cidadeId);
    if (cidade) {
      return `${cidade.nome}/${estado.uf}`;
    }
  }
  return `Cidade #${cidadeId}`;
}
