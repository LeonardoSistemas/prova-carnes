import { beforeEach, describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { render } from '@testing-library/react';
import { renderWithProviders } from '../../test/renderWithProviders';
import { ApiError } from '../../api/client';
import { carnesApi } from '../../api/carnesApi';
import { compradoresApi } from '../../api/compradoresApi';
import { pedidosApi } from '../../api/pedidosApi';
import { Moeda, OrigemCarne, type CarneResponseDto, type CompradorResponseDto, type PedidoResponseDto } from '../../api/types';
import { PedidoFormPage } from './PedidoFormPage';

vi.mock('../../api/carnesApi', () => ({
  carnesApi: { listar: vi.fn(), criar: vi.fn(), atualizar: vi.fn(), excluir: vi.fn() },
}));

vi.mock('../../api/compradoresApi', () => ({
  compradoresApi: { listar: vi.fn(), criar: vi.fn(), atualizar: vi.fn(), excluir: vi.fn() },
}));

vi.mock('../../api/pedidosApi', () => ({
  pedidosApi: { listar: vi.fn(), obterPorId: vi.fn(), criar: vi.fn(), atualizar: vi.fn(), excluir: vi.fn() },
}));

const carnesApiMock = vi.mocked(carnesApi);
const compradoresApiMock = vi.mocked(compradoresApi);
const pedidosApiMock = vi.mocked(pedidosApi);

const CARNE: CarneResponseDto = { id: 1, descricao: 'Picanha', origem: OrigemCarne.Bovina };
const COMPRADOR: CompradorResponseDto = { id: 1, nome: 'Frigorifico Sul', documento: '123', cidadeId: 10 };

beforeEach(() => {
  vi.clearAllMocks();
  carnesApiMock.listar.mockResolvedValue([CARNE]);
  compradoresApiMock.listar.mockResolvedValue([COMPRADOR]);
});

/**
 * Helper para renderizar `PedidoFormPage` com rotas corretamente configuradas,
 * de forma que `useParams` funcione em modo edição.
 */
function renderPedidoFormPageWithRoute(initialRoute: string) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialRoute]}>
        <Routes>
          <Route path="/pedidos/novo" element={<PedidoFormPage />} />
          <Route path="/pedidos/:id/editar" element={<PedidoFormPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

async function preencherCabecalho(user: ReturnType<typeof userEvent.setup>) {
  await user.type(screen.getByLabelText('Data'), '2026-07-18');
  await user.selectOptions(await screen.findByLabelText('Comprador'), '1');
}

describe('PedidoFormPage - não mostra erro de validação antes da primeira interação', () => {
  it('não exibe nenhuma mensagem de erro ao abrir o formulário de novo pedido', async () => {
    renderWithProviders(<PedidoFormPage />);

    await screen.findByLabelText('Comprador');

    expect(screen.queryByText('Data é obrigatória.')).not.toBeInTheDocument();
    expect(screen.queryByText('Comprador é obrigatório.')).not.toBeInTheDocument();
    expect(screen.queryByText('Adicione ao menos um item ao pedido.')).not.toBeInTheDocument();
  });
});

describe('PedidoFormPage - lista de itens não pode ficar vazia', () => {
  it('impede salvar sem nenhum item e não chama a API', async () => {
    const user = userEvent.setup();
    renderWithProviders(<PedidoFormPage />);

    await preencherCabecalho(user);
    await user.click(screen.getByRole('button', { name: 'Salvar pedido' }));

    expect(await screen.findByText('Adicione ao menos um item ao pedido.')).toBeInTheDocument();
    expect(pedidosApiMock.criar).not.toHaveBeenCalled();
  });
});

describe('PedidoFormPage - preço deve ser positivo', () => {
  it('rejeita preço zero ou negativo e não chama a API', async () => {
    const user = userEvent.setup();
    renderWithProviders(<PedidoFormPage />);

    await preencherCabecalho(user);
    await user.click(screen.getByRole('button', { name: 'Adicionar item' }));

    await user.selectOptions(screen.getByLabelText('Carne'), '1');
    await user.clear(screen.getByLabelText('Preço'));
    await user.type(screen.getByLabelText('Preço'), '-5');
    await user.selectOptions(screen.getByLabelText('Moeda'), String(Moeda.BRL));

    await user.click(screen.getByRole('button', { name: 'Salvar pedido' }));

    expect(await screen.findByText('Preço deve ser um número positivo.')).toBeInTheDocument();
    expect(pedidosApiMock.criar).not.toHaveBeenCalled();
  });

  it('salva o pedido quando o preço é positivo e o restante do formulário é válido', async () => {
    const user = userEvent.setup();
    pedidosApiMock.criar.mockResolvedValue({
      id: 1,
      data: '2026-07-18',
      compradorId: 1,
      itens: [{ id: 1, carneId: 1, preco: 50, moeda: Moeda.BRL, cotacaoUsada: 1, valorEmReal: 50 }],
      valorTotalEmReal: 50,
    });
    renderWithProviders(<PedidoFormPage />);

    await preencherCabecalho(user);
    await user.click(screen.getByRole('button', { name: 'Adicionar item' }));

    await user.selectOptions(screen.getByLabelText('Carne'), '1');
    await user.clear(screen.getByLabelText('Preço'));
    await user.type(screen.getByLabelText('Preço'), '50');
    await user.selectOptions(screen.getByLabelText('Moeda'), String(Moeda.BRL));

    await user.click(screen.getByRole('button', { name: 'Salvar pedido' }));

    await waitFor(() =>
      expect(pedidosApiMock.criar).toHaveBeenCalledWith({
        data: '2026-07-18',
        compradorId: 1,
        itens: [{ carneId: 1, preco: 50, moeda: Moeda.BRL }],
      }),
    );
  });
});

describe('PedidoFormPage - erro 422 de cotação indisponível', () => {
  it('exibe a mensagem real da API e o botão volta ao estado normal (sem loading infinito)', async () => {
    const user = userEvent.setup();
    pedidosApiMock.criar.mockRejectedValue(
      new ApiError(422, ['Não foi possível obter a cotação da moeda no momento. Tente novamente mais tarde.']),
    );
    renderWithProviders(<PedidoFormPage />);

    await preencherCabecalho(user);
    await user.click(screen.getByRole('button', { name: 'Adicionar item' }));

    await user.selectOptions(screen.getByLabelText('Carne'), '1');
    await user.clear(screen.getByLabelText('Preço'));
    await user.type(screen.getByLabelText('Preço'), '50');
    await user.selectOptions(screen.getByLabelText('Moeda'), String(Moeda.USD));

    await user.click(screen.getByRole('button', { name: 'Salvar pedido' }));

    expect(
      await screen.findByText('Não foi possível obter a cotação da moeda no momento. Tente novamente mais tarde.'),
    ).toBeInTheDocument();

    const botaoSalvar = screen.getByRole('button', { name: 'Salvar pedido' });
    expect(botaoSalvar).toBeEnabled();
  });
});

describe('PedidoFormPage - modo de edição', () => {
  it('carrega o pedido existente pelo ID e pré-preenche o formulário', async () => {
    const pedidoExistente: PedidoResponseDto = {
      id: 42,
      data: '2026-07-10T00:00:00',
      compradorId: 1,
      itens: [
        {
          id: 1,
          carneId: 1,
          preco: 150.5,
          moeda: Moeda.USD,
          cotacaoUsada: 5.2,
          valorEmReal: 783.6,
        },
      ],
      valorTotalEmReal: 783.6,
    };

    pedidosApiMock.obterPorId.mockResolvedValue(pedidoExistente);
    renderPedidoFormPageWithRoute('/pedidos/42/editar');

    // Aguardar que o formulário seja carregado
    const inputData = await screen.findByLabelText('Data');
    const selectComprador = screen.getByLabelText('Comprador');

    // Verificar que o formulário foi pré-preenchido
    expect(inputData).toHaveValue('2026-07-10');
    expect(selectComprador).toHaveValue('1');

    // Verificar que o item também foi carregado (deveria haver um item na lista)
    expect(screen.getByDisplayValue('150.5')).toBeInTheDocument();
  });

  it('chama pedidosApi.atualizar ao invés de criar', async () => {
    const user = userEvent.setup();
    const pedidoExistente: PedidoResponseDto = {
      id: 42,
      data: '2026-07-10T00:00:00',
      compradorId: 1,
      itens: [
        {
          id: 1,
          carneId: 1,
          preco: 150,
          moeda: Moeda.BRL,
          cotacaoUsada: 1,
          valorEmReal: 150,
        },
      ],
      valorTotalEmReal: 150,
    };

    pedidosApiMock.obterPorId.mockResolvedValue(pedidoExistente);
    pedidosApiMock.atualizar.mockResolvedValue(undefined);

    renderPedidoFormPageWithRoute('/pedidos/42/editar');

    // Aguardar que o formulário seja carregado
    await screen.findByLabelText('Data');

    // Clicar em Salvar (o pedido já está pré-preenchido)
    const botaoSalvar = screen.getByRole('button', { name: 'Salvar pedido' });
    await user.click(botaoSalvar);

    // Verificar que atualizar foi chamado, não criar
    await waitFor(() => {
      expect(pedidosApiMock.atualizar).toHaveBeenCalledWith(42, {
        data: '2026-07-10',
        compradorId: 1,
        itens: [{ carneId: 1, preco: 150, moeda: Moeda.BRL }],
      });
    });
    expect(pedidosApiMock.criar).not.toHaveBeenCalled();
  });

  it('exibe título "Editar pedido" com ID quando em modo edição', async () => {
    const pedidoExistente: PedidoResponseDto = {
      id: 42,
      data: '2026-07-10T00:00:00',
      compradorId: 1,
      itens: [],
      valorTotalEmReal: 0,
    };

    pedidosApiMock.obterPorId.mockResolvedValue(pedidoExistente);
    renderPedidoFormPageWithRoute('/pedidos/42/editar');

    await screen.findByText('Editar pedido #42');
    expect(screen.getByText('Editar pedido #42')).toBeInTheDocument();
  });
});
