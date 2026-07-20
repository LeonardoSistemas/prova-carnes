import type { CompradorResponseDto } from '../api/types';

/** `PedidoResponseDto` só traz `compradorId` — o enriquecimento de exibição (nome) é responsabilidade do frontend. */
export function encontrarNomeComprador(compradores: CompradorResponseDto[], compradorId: number): string {
  const comprador = compradores.find((candidato) => candidato.id === compradorId);
  return comprador ? comprador.nome : `Comprador #${compradorId}`;
}
