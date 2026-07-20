import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import type { FaturamentoPorDiaDto } from '../../api/types';
import { formatarDataBR, formatarReal } from '../../format/currency';

interface FaturamentoChartProps {
  dados: FaturamentoPorDiaDto[];
}

/**
 * Gráfico de linha de faturamento por dia (Recharts). A cor da linha usa
 * `--accent` (#A63D2F, ver `index.css`) para manter consistência com o resto
 * do design system em vez da paleta padrão do Recharts.
 *
 * `ResponsiveContainer` depende de `ResizeObserver`, indisponível no jsdom
 * usado pelos testes — o próprio Recharts já trata essa ausência sem lançar
 * exceção (só não recalcula o tamanho), então nenhum polyfill é necessário
 * aqui: o teste cobre a presença do container, não as dimensões do SVG.
 */
export function FaturamentoChart({ dados }: FaturamentoChartProps) {
  if (dados.length === 0) {
    return <p>Sem faturamento no período selecionado.</p>;
  }

  const dadosFormatados = dados.map((item) => ({
    ...item,
    dataFormatada: formatarDataBR(item.data),
  }));

  return (
    <div className="dashboard-chart-container" data-testid="faturamento-chart">
      <ResponsiveContainer width="100%" height={280}>
        <LineChart data={dadosFormatados} margin={{ top: 8, right: 16, left: 8, bottom: 8 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#e5e5e5" />
          <XAxis dataKey="dataFormatada" tick={{ fontSize: 12 }} />
          <YAxis tick={{ fontSize: 12 }} tickFormatter={(valor: number) => formatarReal(valor)} width={90} />
          <Tooltip formatter={(valor) => (typeof valor === 'number' ? formatarReal(valor) : valor)} />
          <Line
            type="monotone"
            dataKey="faturamento"
            name="Faturamento"
            stroke="#a63d2f"
            strokeWidth={2}
            dot={{ r: 3 }}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
