import { useNavigate } from 'react-router-dom';
import type { CarneResponseDto } from '../../api/types';
import { ORIGEM_CARNE_LABELS } from '../../format/enumLabels';

interface CarneTableProps {
  carnes: CarneResponseDto[];
  onDeleteRequest: (carne: CarneResponseDto) => void;
}

export function CarneTable({ carnes, onDeleteRequest }: CarneTableProps) {
  const navigate = useNavigate();

  if (carnes.length === 0) {
    return <p>Nenhuma carne cadastrada.</p>;
  }

  return (
    <table>
      <thead>
        <tr>
          <th>Id</th>
          <th>Descrição</th>
          <th>Origem</th>
          <th>Ações</th>
        </tr>
      </thead>
      <tbody>
        {carnes.map((carne) => (
          <tr key={carne.id}>
            <td data-label="Id">{carne.id}</td>
            <td data-label="Descrição">{carne.descricao}</td>
            <td data-label="Origem">{ORIGEM_CARNE_LABELS[carne.origem]}</td>
            <td data-label="Ações">
              <button type="button" title="Editar carne" onClick={() => navigate(`/carnes/${carne.id}/editar`)}>
                Editar
              </button>
              <button type="button" className="danger" title="Excluir carne" onClick={() => onDeleteRequest(carne)}>
                Excluir
              </button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
