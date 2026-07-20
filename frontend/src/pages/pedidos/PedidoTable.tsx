import { useNavigate } from 'react-router-dom';
import type { CompradorResponseDto, PedidoResponseDto } from '../../api/types';
import { formatarDataBR, formatarReal } from '../../format/currency';
import { encontrarNomeComprador } from '../../format/compradorLookup';

interface PedidoTableProps {
  pedidos: PedidoResponseDto[];
  compradores: CompradorResponseDto[];
  onDeleteRequest: (pedido: PedidoResponseDto) => void;
}

/**
 * Exibe `valorTotalEmReal` exatamente como veio da API — a cotação usada em
 * cada item já foi persistida no momento do POST/PUT; a listagem nunca
 * recalcula.
 */
export function PedidoTable({ pedidos, compradores, onDeleteRequest }: PedidoTableProps) {
  const navigate = useNavigate();

  if (pedidos.length === 0) {
    return <p>Nenhum pedido cadastrado.</p>;
  }

  return (
    <table>
      <thead>
        <tr>
          <th>Id</th>
          <th>Data</th>
          <th>Comprador</th>
          <th>Valor total (R$)</th>
          <th>Ações</th>
        </tr>
      </thead>
      <tbody>
        {pedidos.map((pedido) => (
          <tr key={pedido.id}>
            <td data-label="Id">{pedido.id}</td>
            <td data-label="Data">{formatarDataBR(pedido.data)}</td>
            <td data-label="Comprador">{encontrarNomeComprador(compradores, pedido.compradorId)}</td>
            <td data-label="Valor total (R$)" className="valor-monetario">{formatarReal(pedido.valorTotalEmReal)}</td>
            <td data-label="Ações">
              <button type="button" title="Editar pedido" onClick={() => navigate(`/pedidos/${pedido.id}/editar`)}>
                Editar
              </button>
              <button type="button" className="danger" title="Excluir pedido" onClick={() => onDeleteRequest(pedido)}>
                Excluir
              </button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
