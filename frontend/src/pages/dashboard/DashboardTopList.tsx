import { formatarReal } from '../../format/currency';

export interface DashboardTopListItem {
  id: number;
  nome: string;
  valorTotal: number;
}

interface DashboardTopListProps {
  titulo: string;
  itens: DashboardTopListItem[];
  mensagemVazia: string;
}

/**
 * Ranking Top 5 (carnes ou compradores) — mesmo padrão visual de `<table>`
 * zebrada já usado em `CarneTable`/`CompradorTable`/`PedidoTable` (frente A),
 * reaproveitado aqui em vez de introduzir um componente de lista novo. Um
 * único componente genérico evita duplicar a mesma tabela duas vezes na
 * página (Top Carnes / Top Compradores) só trocando os nomes das colunas.
 */
export function DashboardTopList({ titulo, itens, mensagemVazia }: DashboardTopListProps) {
  return (
    <div className="dashboard-top-list">
      <h2>{titulo}</h2>
      {itens.length === 0 ? (
        <p>{mensagemVazia}</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>#</th>
              <th>Nome</th>
              <th>Valor</th>
            </tr>
          </thead>
          <tbody>
            {itens.map((item, indice) => (
              <tr key={item.id}>
                <td>{indice + 1}</td>
                <td>{item.nome}</td>
                <td className="valor-monetario">{formatarReal(item.valorTotal)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
