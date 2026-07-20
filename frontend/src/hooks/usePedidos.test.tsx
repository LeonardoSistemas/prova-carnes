import type { ReactNode } from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { renderHook, render } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { pedidosApi } from '../api/pedidosApi';
import { carnesApi } from '../api/carnesApi';
import { compradoresApi } from '../api/compradoresApi';
import { Moeda, OrigemCarne, type CarneResponseDto, type CompradorResponseDto, type PedidoResponseDto } from '../api/types';
import { useCreatePedido, usePedido, usePedidos } from './usePedidos';
import { PedidosListPage } from '../pages/pedidos/PedidosListPage';
import { PedidoFormPage } from '../pages/pedidos/PedidoFormPage';

vi.mock('../api/pedidosApi', () => ({
  pedidosApi: { listar: vi.fn(), obterPorId: vi.fn(), criar: vi.fn(), atualizar: vi.fn(), excluir: vi.fn() },
}));

vi.mock('../api/carnesApi', () => ({
  carnesApi: { listar: vi.fn(), criar: vi.fn(), atualizar: vi.fn(), excluir: vi.fn() },
}));

vi.mock('../api/compradoresApi', () => ({
  compradoresApi: { listar: vi.fn(), criar: vi.fn(), atualizar: vi.fn(), excluir: vi.fn() },
}));

const pedidosApiMock = vi.mocked(pedidosApi);
const carnesApiMock = vi.mocked(carnesApi);
const compradoresApiMock = vi.mocked(compradoresApi);

const CARNE: CarneResponseDto = { id: 1, descricao: 'Picanha', origem: OrigemCarne.Bovina };
const COMPRADOR: CompradorResponseDto = { id: 1, nome: 'Frigorifico Sul', documento: '123', cidadeId: 10 };

const PEDIDO: PedidoResponseDto = {
  id: 7,
  data: '2026-07-18T00:00:00',
  compradorId: 1,
  itens: [{ id: 1, carneId: 1, preco: 50, moeda: Moeda.BRL, cotacaoUsada: 1, valorEmReal: 50 }],
  valorTotalEmReal: 50,
};

function criarWrapper(queryClient: QueryClient) {
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
  };
}

beforeEach(() => {
  vi.clearAllMocks();
  carnesApiMock.listar.mockResolvedValue([CARNE]);
  compradoresApiMock.listar.mockResolvedValue([COMPRADOR]);
});

describe('usePedidos/usePedido - queryKey não colide entre listagem e detalhe (T41)', () => {
  it(
    'reproduz o cenário relatado: visitar /pedidos sem filtro e depois abrir /pedidos/novo ' +
      'com o mesmo QueryClient — o formulário abre vazio, sem lançar exceção',
    async () => {
      pedidosApiMock.listar.mockResolvedValue([PEDIDO]);

      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
      });

      // 1) Visita /pedidos sem filtro — popula o cache de `usePedidos(undefined)`.
      const { unmount } = render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/pedidos']}>
            <PedidosListPage />
          </MemoryRouter>
        </QueryClientProvider>,
      );

      expect(await screen.findByRole('cell', { name: 'Frigorifico Sul' })).toBeInTheDocument();
      unmount();

      // 2) Abre /pedidos/novo (modo criação, `usePedido(undefined)`) reaproveitando o
      // mesmo QueryClient — antes da correção, isso expunha o array da listagem como
      // se fosse o pedido em edição e quebrava em `mapPedidoResponseToFormValues`.
      expect(() =>
        render(
          <QueryClientProvider client={queryClient}>
            <MemoryRouter initialEntries={['/pedidos/novo']}>
              <PedidoFormPage />
            </MemoryRouter>
          </QueryClientProvider>,
        ),
      ).not.toThrow();

      await screen.findByLabelText('Comprador');

      expect(screen.getByLabelText('Data')).toHaveValue('');
      expect(screen.getByLabelText('Comprador')).toHaveValue('');
      expect(screen.queryByLabelText('Carne')).not.toBeInTheDocument();
      expect(pedidosApiMock.obterPorId).not.toHaveBeenCalled();
    },
  );

  it('usam segmentos de queryKey distintos entre si (lista vs. detalhe)', async () => {
    pedidosApiMock.listar.mockResolvedValue([PEDIDO]);
    pedidosApiMock.obterPorId.mockResolvedValue(PEDIDO);

    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    });
    const wrapper = criarWrapper(queryClient);

    renderHook(() => usePedidos(undefined), { wrapper });
    renderHook(() => usePedido(7), { wrapper });

    await waitFor(() => expect(pedidosApiMock.listar).toHaveBeenCalledTimes(1));
    await waitFor(() => expect(pedidosApiMock.obterPorId).toHaveBeenCalledTimes(1));

    const chavesEmCache = queryClient.getQueryCache().getAll().map((query) => query.queryKey);
    expect(chavesEmCache).toContainEqual(['pedidos', 'lista', undefined]);
    expect(chavesEmCache).toContainEqual(['pedidos', 'detalhe', 7]);
  });

  it('invalidateQueries({ queryKey: ["pedidos"] }) nas mutations continua invalidando lista e detalhe', async () => {
    pedidosApiMock.listar.mockResolvedValue([PEDIDO]);
    pedidosApiMock.obterPorId.mockResolvedValue(PEDIDO);
    pedidosApiMock.criar.mockResolvedValue(PEDIDO);

    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    });
    const wrapper = criarWrapper(queryClient);

    renderHook(() => usePedidos(undefined), { wrapper });
    renderHook(() => usePedido(7), { wrapper });

    await waitFor(() => expect(pedidosApiMock.listar).toHaveBeenCalledTimes(1));
    await waitFor(() => expect(pedidosApiMock.obterPorId).toHaveBeenCalledTimes(1));

    const { result } = renderHook(() => useCreatePedido(), { wrapper });
    result.current.mutate({ data: '2026-07-18', compradorId: 1, itens: [] });

    await waitFor(() => expect(pedidosApiMock.listar).toHaveBeenCalledTimes(2));
    await waitFor(() => expect(pedidosApiMock.obterPorId).toHaveBeenCalledTimes(2));
  });
});
