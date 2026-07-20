import { beforeEach, describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderWithProviders } from '../../test/renderWithProviders';
import { dashboardApi } from '../../api/dashboardApi';
import type { DashboardDto, DashboardTopDto, FaturamentoPorDiaDto } from '../../api/types';
import { DashboardPage } from './DashboardPage';

vi.mock('../../api/dashboardApi', () => ({
  dashboardApi: {
    obterResumo: vi.fn(),
    obterFaturamentoPorDia: vi.fn(),
  },
}));

const dashboardApiMock = vi.mocked(dashboardApi);

function criarDashboardDto(
  overridesResumo: Partial<DashboardDto['resumo']> = {},
  overridesTop: Partial<DashboardTopDto> = {},
): DashboardDto {
  return {
    resumo: {
      totalPedidos: 12,
      faturamentoTotal: 34567.89,
      ticketMedio: 2880.66,
      compradoresAtivos: 5,
      compradoresCadastrados: 9,
      ...overridesResumo,
    },
    top: { topCarnes: [], topCompradores: [], ...overridesTop },
  };
}

function criarTopCarnes(): DashboardTopDto['topCarnes'] {
  return [
    { carneId: 1, descricao: 'Picanha', valorTotal: 5000 },
    { carneId: 2, descricao: 'Alcatra', valorTotal: 4200 },
    { carneId: 3, descricao: 'Fraldinha', valorTotal: 3100 },
    { carneId: 4, descricao: 'Costela', valorTotal: 2400 },
    { carneId: 5, descricao: 'Maminha', valorTotal: 1800 },
  ];
}

function criarTopCompradores(): DashboardTopDto['topCompradores'] {
  return [
    { compradorId: 1, nome: 'Açougue Central', valorTotal: 6000 },
    { compradorId: 2, nome: 'Churrascaria Boi Bom', valorTotal: 5100 },
    { compradorId: 3, nome: 'Distribuidora Sul', valorTotal: 4300 },
    { compradorId: 4, nome: 'Mercado Bom Preço', valorTotal: 3200 },
    { compradorId: 5, nome: 'Restaurante Fogo Vivo', valorTotal: 2500 },
  ];
}

function criarFaturamentoPorDia(): FaturamentoPorDiaDto[] {
  return [
    { data: '2026-07-12', faturamento: 1000 },
    { data: '2026-07-13', faturamento: 1500 },
    { data: '2026-07-14', faturamento: 900 },
  ];
}

beforeEach(() => {
  vi.clearAllMocks();
  dashboardApiMock.obterResumo.mockResolvedValue(criarDashboardDto());
  dashboardApiMock.obterFaturamentoPorDia.mockResolvedValue([]);
});

describe('DashboardPage', () => {
  it('busca o resumo do período "hoje" por padrão ao entrar na tela', async () => {
    renderWithProviders(<DashboardPage />);

    await waitFor(() => expect(dashboardApiMock.obterResumo).toHaveBeenCalledWith('hoje'));
  });

  it('exibe os 3 cards de métrica com os dados retornados pela API', async () => {
    renderWithProviders(<DashboardPage />);

    expect(await screen.findByText('12')).toBeInTheDocument();
    expect(screen.getByText(/R\$\s?34\.567,89/)).toBeInTheDocument();
    expect(screen.getByText(/Ticket médio:\s*R\$\s?2\.880,66/)).toBeInTheDocument();
    expect(screen.getByText('5 de 9')).toBeInTheDocument();
  });

  it('trocar o seletor para "Semana" dispara novo fetch com o período correto', async () => {
    const user = userEvent.setup();
    renderWithProviders(<DashboardPage />);

    await waitFor(() => expect(dashboardApiMock.obterResumo).toHaveBeenCalledWith('hoje'));

    await user.click(screen.getByRole('button', { name: 'Semana' }));

    await waitFor(() => expect(dashboardApiMock.obterResumo).toHaveBeenLastCalledWith('semana'));
  });

  it('trocar o seletor para "Mês" dispara novo fetch com o período correto', async () => {
    const user = userEvent.setup();
    renderWithProviders(<DashboardPage />);

    await waitFor(() => expect(dashboardApiMock.obterResumo).toHaveBeenCalledWith('hoje'));

    await user.click(screen.getByRole('button', { name: 'Mês' }));

    await waitFor(() => expect(dashboardApiMock.obterResumo).toHaveBeenLastCalledWith('mes'));
  });

  it('exibe mensagem inline quando a busca do resumo falha', async () => {
    dashboardApiMock.obterResumo.mockRejectedValue(new Error('falha'));
    renderWithProviders(<DashboardPage />);

    expect(await screen.findByRole('alert')).toHaveTextContent('Não foi possível carregar os dados do dashboard.');
  });

  it('busca o faturamento por dia com a janela fixa de 7 dias ao entrar na tela', async () => {
    renderWithProviders(<DashboardPage />);

    await waitFor(() => expect(dashboardApiMock.obterFaturamentoPorDia).toHaveBeenCalledWith(7));
  });

  it('exibe os 5 itens do ranking Top Carnes com nome e valor formatado', async () => {
    dashboardApiMock.obterResumo.mockResolvedValue(criarDashboardDto({}, { topCarnes: criarTopCarnes() }));
    renderWithProviders(<DashboardPage />);

    expect(await screen.findByText('Picanha')).toBeInTheDocument();
    for (const carne of criarTopCarnes()) {
      expect(screen.getByText(carne.descricao)).toBeInTheDocument();
    }
    expect(screen.getByText(/R\$\s?5\.000,00/)).toBeInTheDocument();
    expect(screen.getByText(/R\$\s?1\.800,00/)).toBeInTheDocument();
  });

  it('exibe os 5 itens do ranking Top Compradores com nome e valor formatado', async () => {
    dashboardApiMock.obterResumo.mockResolvedValue(
      criarDashboardDto({}, { topCompradores: criarTopCompradores() }),
    );
    renderWithProviders(<DashboardPage />);

    expect(await screen.findByText('Açougue Central')).toBeInTheDocument();
    for (const comprador of criarTopCompradores()) {
      expect(screen.getByText(comprador.nome)).toBeInTheDocument();
    }
    expect(screen.getByText(/R\$\s?6\.000,00/)).toBeInTheDocument();
    expect(screen.getByText(/R\$\s?2\.500,00/)).toBeInTheDocument();
  });

  it('exibe mensagem quando não há dados no ranking do período', async () => {
    renderWithProviders(<DashboardPage />);

    expect(await screen.findByText('Nenhuma carne com pedidos no período.')).toBeInTheDocument();
    expect(screen.getByText('Nenhum comprador com pedidos no período.')).toBeInTheDocument();
  });

  it('renderiza o container do gráfico de faturamento quando há dados', async () => {
    dashboardApiMock.obterFaturamentoPorDia.mockResolvedValue(criarFaturamentoPorDia());
    renderWithProviders(<DashboardPage />);

    expect(await screen.findByTestId('faturamento-chart')).toBeInTheDocument();
  });

  it('não quebra e exibe mensagem quando a lista de faturamento por dia vem vazia', async () => {
    dashboardApiMock.obterFaturamentoPorDia.mockResolvedValue([]);
    renderWithProviders(<DashboardPage />);

    expect(await screen.findByText('Sem faturamento no período selecionado.')).toBeInTheDocument();
    expect(screen.queryByTestId('faturamento-chart')).not.toBeInTheDocument();
  });

  it('exibe mensagem inline quando a busca do faturamento por dia falha', async () => {
    dashboardApiMock.obterFaturamentoPorDia.mockRejectedValue(new Error('falha'));
    renderWithProviders(<DashboardPage />);

    expect(await screen.findByRole('alert')).toHaveTextContent('Não foi possível carregar o faturamento por dia.');
  });
});
