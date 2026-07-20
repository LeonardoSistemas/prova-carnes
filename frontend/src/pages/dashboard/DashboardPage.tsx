import { useState } from 'react';
import type { PeriodoDashboard } from '../../api/types';
import { ApiErrorBanner } from '../../components/ApiErrorBanner';
import { Breadcrumb } from '../../components/Breadcrumb';
import { formatarReal } from '../../format/currency';
import { useDashboardResumo, useFaturamentoPorDia } from '../../hooks/useDashboard';
import { DashboardTopList } from './DashboardTopList';
import { FaturamentoChart } from './FaturamentoChart';

const OPCOES_PERIODO: Array<{ id: PeriodoDashboard; label: string }> = [
  { id: 'hoje', label: 'Hoje' },
  { id: 'semana', label: 'Semana' },
  { id: 'mes', label: 'Mês' },
];

/**
 * Janela fixa (em dias) do gráfico de faturamento (T66). A task deixa livre
 * entre 7 ou 30 dias corridos, com seletor opcional — optamos por um valor
 * fixo de 7 dias, alinhado ao período "Semana" já existente no seletor de
 * cards acima, para manter a tela enxuta sem introduzir um segundo seletor
 * fazendo a mesma pergunta ("qual janela de tempo?") de duas formas
 * diferentes na mesma página.
 */
const DIAS_GRAFICO_FATURAMENTO = 7;

/**
 * Tela de Dashboard: cards de métrica (T65) + rankings Top 5 de carnes/
 * compradores e gráfico de linha de faturamento por dia (T66). Os rankings
 * vêm do mesmo `useDashboardResumo(periodo)` dos cards — não há uma segunda
 * chamada HTTP para o Top 5, então trocar o período recalcula cards e
 * rankings juntos, a partir de um único fetch. O gráfico usa a query própria
 * `useFaturamentoPorDia`, independente do seletor de período.
 */
export function DashboardPage() {
  const [periodo, setPeriodo] = useState<PeriodoDashboard>('hoje');
  const { data, isLoading, isError } = useDashboardResumo(periodo);
  const faturamento = useFaturamentoPorDia(DIAS_GRAFICO_FATURAMENTO);
  const resumo = data?.resumo;
  const top = data?.top;

  return (
    <section>
      <Breadcrumb />
      <div className="page-header">
        <h1>Dashboard</h1>
      </div>

      <div className="periodo-seletor" role="group" aria-label="Período">
        {OPCOES_PERIODO.map((opcao) => (
          <button
            key={opcao.id}
            type="button"
            aria-pressed={opcao.id === periodo}
            className={opcao.id === periodo ? 'periodo-botao periodo-botao--ativo' : 'periodo-botao'}
            onClick={() => setPeriodo(opcao.id)}
          >
            {opcao.label}
          </button>
        ))}
      </div>

      {isLoading && <p>Carregando dashboard...</p>}
      {isError && <ApiErrorBanner errors={['Não foi possível carregar os dados do dashboard.']} />}

      {!isLoading && !isError && resumo && (
        <div className="dashboard-cards">
          <div className="dashboard-card">
            <h2>Pedidos no período</h2>
            <p className="dashboard-card-valor">{resumo.totalPedidos}</p>
          </div>

          <div className="dashboard-card">
            <h2>Faturamento</h2>
            <p className="dashboard-card-valor">{formatarReal(resumo.faturamentoTotal)}</p>
            <p className="dashboard-card-secundario">Ticket médio: {formatarReal(resumo.ticketMedio)}</p>
          </div>

          <div className="dashboard-card">
            <h2>Compradores ativos</h2>
            <p className="dashboard-card-valor">
              {resumo.compradoresAtivos} de {resumo.compradoresCadastrados}
            </p>
          </div>
        </div>
      )}

      {!isLoading && !isError && top && (
        <div className="dashboard-top-lists">
          <DashboardTopList
            titulo="Top 5 Carnes"
            mensagemVazia="Nenhuma carne com pedidos no período."
            itens={top.topCarnes.map((carne) => ({
              id: carne.carneId,
              nome: carne.descricao,
              valorTotal: carne.valorTotal,
            }))}
          />

          <DashboardTopList
            titulo="Top 5 Compradores"
            mensagemVazia="Nenhum comprador com pedidos no período."
            itens={top.topCompradores.map((comprador) => ({
              id: comprador.compradorId,
              nome: comprador.nome,
              valorTotal: comprador.valorTotal,
            }))}
          />
        </div>
      )}

      <div className="dashboard-chart">
        <h2>Faturamento — últimos {DIAS_GRAFICO_FATURAMENTO} dias</h2>

        {faturamento.isLoading && <p>Carregando gráfico...</p>}
        {faturamento.isError && (
          <ApiErrorBanner errors={['Não foi possível carregar o faturamento por dia.']} />
        )}
        {!faturamento.isLoading && !faturamento.isError && (
          <FaturamentoChart dados={faturamento.data ?? []} />
        )}
      </div>
    </section>
  );
}
