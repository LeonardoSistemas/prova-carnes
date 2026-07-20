import type { PedidoResponseDto } from '../../api/types';
import { paraInputDate } from '../../format/currency';
import type { PedidoFormValues, PedidoItemFormValues } from '../../validation/validatePedidoForm';

let proximaChaveItem = 0;

function gerarChaveItem(): string {
  proximaChaveItem += 1;
  return `item-existente-${proximaChaveItem}`;
}

/** Converte a resposta de GET /pedidos/{id} para os valores do formulário controlado (modo edição). */
export function mapPedidoResponseToFormValues(pedido: PedidoResponseDto): PedidoFormValues {
  const itens: PedidoItemFormValues[] = pedido.itens.map((item) => ({
    chave: gerarChaveItem(),
    carneId: item.carneId,
    preco: String(item.preco),
    moeda: item.moeda,
  }));

  return {
    data: paraInputDate(pedido.data),
    compradorId: pedido.compradorId,
    itens,
  };
}
