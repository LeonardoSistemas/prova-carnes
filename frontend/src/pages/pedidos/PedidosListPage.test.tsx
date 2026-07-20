import { beforeEach, describe, expect, it, vi } from 'vitest';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderWithProviders } from '../../test/renderWithProviders';
import { pedidosApi } from '../../api/pedidosApi';
import { compradoresApi } from '../../api/compradoresApi';
import { Moeda, type CompradorResponseDto, type PedidoResponseDto } from '../../api/types';
import { PedidosListPage } from './PedidosListPage';

vi.mock('../../api/pedidosApi', () => ({
  pedidosApi: {
    listar: vi.fn(),
    obterPorId: vi.fn(),
    criar: vi.fn(),
    atualizar: vi.fn(),
    excluir: vi.fn(),
  },
}));

vi.mock('../../api/compradoresApi', () => ({
  compradoresApi: {
    listar: vi.fn(),
    criar: vi.fn(),
    atualizar: vi.fn(),
    excluir: vi.fn(),
  },
}));

const pedidosApiMock = vi.mocked(pedidosApi);
const compradoresApiMock = vi.mocked(compradoresApi);

const NOME_COMPRADOR = 'Frigorifico Sul';

const COMPRADOR: CompradorResponseDto = { id: 1, nome: NOME_COMPRADOR, documento: '123', cidadeId: 10 };

const PEDIDO: PedidoResponseDto = {
  id: 7,
  data: '2026-07-18T00:00:00',
  compradorId: 1,
  itens: [{ id: 1, carneId: 1, preco: 50, moeda: Moeda.BRL, cotacaoUsada: 1, valorEmReal: 50 }],
  valorTotalEmReal: 50,
};

beforeEach(() => {
  vi.clearAllMocks();
  pedidosApiMock.listar.mockResolvedValue([PEDIDO]);
  compradoresApiMock.listar.mockResolvedValue([COMPRADOR]);
});

describe('PedidosListPage', () => {
  it('exibe o comprador (via enriquecimento) e o valor total vindo da API, sem recalcular', async () => {
    renderWithProviders(<PedidosListPage />);

    expect(await screen.findByRole('cell', { name: NOME_COMPRADOR })).toBeInTheDocument();
    expect(screen.getByText(/R\$\s?50,00/)).toBeInTheDocument();
  });

  it('cancelar o modal de exclusao NAO dispara requisicao', async () => {
    const user = userEvent.setup();
    renderWithProviders(<PedidosListPage />);

    await waitFor(() => expect(screen.getByRole('cell', { name: NOME_COMPRADOR })).toBeInTheDocument());

    const linha = screen.getByRole('cell', { name: NOME_COMPRADOR }).closest('tr') as HTMLElement;
    await user.click(within(linha).getByRole('button', { name: 'Excluir' }));

    const modal = await screen.findByRole('dialog');
    await user.click(within(modal).getByRole('button', { name: 'Cancelar' }));

    await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());
    expect(pedidosApiMock.excluir).not.toHaveBeenCalled();
  });

  it('confirmar o modal dispara a exclusao do pedido', async () => {
    const user = userEvent.setup();
    pedidosApiMock.excluir.mockResolvedValue(undefined);
    renderWithProviders(<PedidosListPage />);

    await waitFor(() => expect(screen.getByRole('cell', { name: NOME_COMPRADOR })).toBeInTheDocument());

    const linha = screen.getByRole('cell', { name: NOME_COMPRADOR }).closest('tr') as HTMLElement;
    await user.click(within(linha).getByRole('button', { name: 'Excluir' }));

    const modal = await screen.findByRole('dialog');
    await user.click(within(modal).getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => expect(pedidosApiMock.excluir).toHaveBeenCalledWith(7));
  });

  it('carrega a lista sem filtro (sem query params) ao entrar na tela', async () => {
    renderWithProviders(<PedidosListPage />);

    await waitFor(() => expect(pedidosApiMock.listar).toHaveBeenCalledWith(undefined));
  });

  it('selecionar um comprador no filtro busca pedidos filtrados por compradorId', async () => {
    const user = userEvent.setup();
    renderWithProviders(<PedidosListPage />);

    await waitFor(() => expect(screen.getByRole('cell', { name: NOME_COMPRADOR })).toBeInTheDocument());

    await user.selectOptions(screen.getByLabelText('Comprador'), String(COMPRADOR.id));

    await waitFor(() => expect(pedidosApiMock.listar).toHaveBeenLastCalledWith({ compradorId: COMPRADOR.id }));
  });

  it('limpar o filtro volta a listar pedidos sem parametros', async () => {
    const user = userEvent.setup();
    renderWithProviders(<PedidosListPage />);

    await waitFor(() => expect(screen.getByRole('cell', { name: NOME_COMPRADOR })).toBeInTheDocument());

    await user.selectOptions(screen.getByLabelText('Comprador'), String(COMPRADOR.id));
    await waitFor(() => expect(pedidosApiMock.listar).toHaveBeenLastCalledWith({ compradorId: COMPRADOR.id }));

    await user.click(screen.getByRole('button', { name: 'Limpar filtro' }));

    await waitFor(() => expect(pedidosApiMock.listar).toHaveBeenLastCalledWith(undefined));
  });

  it('exibe mensagem inline quando data inicio é posterior a data fim, sem travar a tela', async () => {
    const user = userEvent.setup();
    renderWithProviders(<PedidosListPage />);

    await waitFor(() => expect(screen.getByRole('cell', { name: NOME_COMPRADOR })).toBeInTheDocument());

    await user.type(screen.getByLabelText('Data início'), '2026-07-20');
    await user.type(screen.getByLabelText('Data fim'), '2026-07-10');

    expect(await screen.findByText('Data início não pode ser posterior à data fim.')).toBeInTheDocument();
  });
});
