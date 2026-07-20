import { describe, expect, it, vi, beforeEach } from 'vitest';
import { pedidosApi, type PedidosFiltro } from './pedidosApi';
import { apiClient } from './client';
import { Moeda } from './types';

// Mockar o apiClient.get localmente para este arquivo de teste
vi.mock('./client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

const apiClientMock = vi.mocked(apiClient);

beforeEach(() => {
  vi.clearAllMocks();
});

describe('pedidosApi.listar', () => {
  it('sem filtro não gera query string', async () => {
    const mockResposta = [
      {
        id: 1,
        data: '2026-07-18',
        compradorId: 1,
        itens: [],
        valorTotalEmReal: 0,
      },
    ];

    apiClientMock.get.mockResolvedValue(mockResposta);

    await pedidosApi.listar();

    expect(apiClientMock.get).toHaveBeenCalledWith('/pedidos');
    expect(apiClientMock.get).toHaveBeenCalledTimes(1);
  });

  it('com compradorId gera query string correta', async () => {
    const mockResposta = [];
    apiClientMock.get.mockResolvedValue(mockResposta);

    const filtro: PedidosFiltro = { compradorId: 42 };
    await pedidosApi.listar(filtro);

    expect(apiClientMock.get).toHaveBeenCalledWith('/pedidos?compradorId=42');
  });

  it('com dataInicio e dataFim gera query string com ambos os params', async () => {
    const mockResposta = [];
    apiClientMock.get.mockResolvedValue(mockResposta);

    const filtro: PedidosFiltro = { dataInicio: '2026-07-01', dataFim: '2026-07-31' };
    await pedidosApi.listar(filtro);

    expect(apiClientMock.get).toHaveBeenCalledWith('/pedidos?dataInicio=2026-07-01&dataFim=2026-07-31');
  });

  it('com os três parâmetros de filtro gera query string completa', async () => {
    const mockResposta = [];
    apiClientMock.get.mockResolvedValue(mockResposta);

    const filtro: PedidosFiltro = {
      compradorId: 10,
      dataInicio: '2026-07-01',
      dataFim: '2026-07-31',
    };
    await pedidosApi.listar(filtro);

    // A ordem dos parâmetros pode variar, então vamos verificar que todos estão presentes
    const callUrl = (apiClientMock.get.mock.calls[0][0] as string);
    expect(callUrl).toContain('/pedidos?');
    expect(callUrl).toContain('compradorId=10');
    expect(callUrl).toContain('dataInicio=2026-07-01');
    expect(callUrl).toContain('dataFim=2026-07-31');
  });

  it('ignora parâmetros undefined', async () => {
    const mockResposta = [];
    apiClientMock.get.mockResolvedValue(mockResposta);

    const filtro: PedidosFiltro = {
      compradorId: 10,
      dataInicio: undefined,
      dataFim: '2026-07-31',
    };
    await pedidosApi.listar(filtro);

    const callUrl = (apiClientMock.get.mock.calls[0][0] as string);
    expect(callUrl).toContain('compradorId=10');
    expect(callUrl).toContain('dataFim=2026-07-31');
    expect(callUrl).not.toContain('dataInicio');
  });

  it('com apenas compradorId e dataFim gera query string correta', async () => {
    const mockResposta = [];
    apiClientMock.get.mockResolvedValue(mockResposta);

    const filtro: PedidosFiltro = {
      compradorId: 5,
      dataFim: '2026-07-31',
    };
    await pedidosApi.listar(filtro);

    const callUrl = (apiClientMock.get.mock.calls[0][0] as string);
    expect(callUrl).toContain('compradorId=5');
    expect(callUrl).toContain('dataFim=2026-07-31');
    expect(callUrl).not.toContain('dataInicio');
  });

  it('retorna a resposta do apiClient', async () => {
    const mockResposta = [
      {
        id: 1,
        data: '2026-07-18',
        compradorId: 1,
        itens: [
          {
            id: 1,
            carneId: 1,
            preco: 100,
            moeda: Moeda.BRL,
            cotacaoUsada: 1,
            valorEmReal: 100,
          },
        ],
        valorTotalEmReal: 100,
      },
    ];

    apiClientMock.get.mockResolvedValue(mockResposta);

    const resultado = await pedidosApi.listar();

    expect(resultado).toEqual(mockResposta);
  });
});

describe('pedidosApi.obterPorId', () => {
  it('chama GET com o ID correto', async () => {
    const mockResposta = {
      id: 42,
      data: '2026-07-18',
      compradorId: 1,
      itens: [],
      valorTotalEmReal: 0,
    };

    apiClientMock.get.mockResolvedValue(mockResposta);

    await pedidosApi.obterPorId(42);

    expect(apiClientMock.get).toHaveBeenCalledWith('/pedidos/42');
  });
});

describe('pedidosApi.criar', () => {
  it('chama POST com o DTO correto', async () => {
    const dto = {
      data: '2026-07-18',
      compradorId: 1,
      itens: [
        {
          carneId: 1,
          preco: 100,
          moeda: Moeda.BRL,
        },
      ],
    };

    const mockResposta = {
      id: 1,
      data: '2026-07-18',
      compradorId: 1,
      itens: [
        {
          id: 1,
          carneId: 1,
          preco: 100,
          moeda: Moeda.BRL,
          cotacaoUsada: 1,
          valorEmReal: 100,
        },
      ],
      valorTotalEmReal: 100,
    };

    apiClientMock.post.mockResolvedValue(mockResposta);

    const resultado = await pedidosApi.criar(dto);

    expect(apiClientMock.post).toHaveBeenCalledWith('/pedidos', dto);
    expect(resultado).toEqual(mockResposta);
  });
});

describe('pedidosApi.atualizar', () => {
  it('chama PUT com o ID e DTO corretos', async () => {
    const dto = {
      data: '2026-07-18',
      compradorId: 1,
      itens: [],
    };

    apiClientMock.put.mockResolvedValue(undefined);

    await pedidosApi.atualizar(42, dto);

    expect(apiClientMock.put).toHaveBeenCalledWith('/pedidos/42', dto);
  });
});

describe('pedidosApi.excluir', () => {
  it('chama DELETE com o ID correto', async () => {
    apiClientMock.delete.mockResolvedValue(undefined);

    await pedidosApi.excluir(42);

    expect(apiClientMock.delete).toHaveBeenCalledWith('/pedidos/42');
  });
});
