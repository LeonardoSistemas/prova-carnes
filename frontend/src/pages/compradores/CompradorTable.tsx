import { useNavigate } from 'react-router-dom';
import type { CompradorResponseDto, EstadoComCidadesDto } from '../../api/types';
import { encontrarNomeCidade } from '../../format/enderecoLookup';

interface CompradorTableProps {
  compradores: CompradorResponseDto[];
  estados: EstadoComCidadesDto[];
  onDeleteRequest: (comprador: CompradorResponseDto) => void;
}

export function CompradorTable({ compradores, estados, onDeleteRequest }: CompradorTableProps) {
  const navigate = useNavigate();

  if (compradores.length === 0) {
    return <p>Nenhum comprador cadastrado.</p>;
  }

  return (
    <table>
      <thead>
        <tr>
          <th>Id</th>
          <th>Nome</th>
          <th>Documento</th>
          <th>Cidade/UF</th>
          <th>Ações</th>
        </tr>
      </thead>
      <tbody>
        {compradores.map((comprador) => (
          <tr key={comprador.id}>
            <td data-label="Id">{comprador.id}</td>
            <td data-label="Nome">{comprador.nome}</td>
            <td data-label="Documento">{comprador.documento}</td>
            <td data-label="Cidade/UF">{encontrarNomeCidade(estados, comprador.cidadeId)}</td>
            <td data-label="Ações">
              <button
                type="button"
                title="Editar comprador"
                onClick={() => navigate(`/compradores/${comprador.id}/editar`)}
              >
                Editar
              </button>
              <button type="button" className="danger" title="Excluir comprador" onClick={() => onDeleteRequest(comprador)}>
                Excluir
              </button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
