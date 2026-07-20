import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderWithProviders } from '../../test/renderWithProviders';
import { ApiError } from '../../api/client';
import { compradoresApi } from '../../api/compradoresApi';
import { estadosApi } from '../../api/estadosApi';
import type { CompradorResponseDto, EstadoComCidadesDto } from '../../api/types';
import { CompradoresPage } from './CompradoresPage';

vi.mock('../../api/compradoresApi', () => ({
  compradoresApi: {
    listar: vi.fn(),
    obterPorId: vi.fn(),
    criar: vi.fn(),
    atualizar: vi.fn(),
    excluir: vi.fn(),
  },
}));

vi.mock('../../api/estadosApi', () => ({
  estadosApi: {
    listar: vi.fn(),
  },
}));

const compradoresApiMock = vi.mocked(compradoresApi);
const estadosApiMock = vi.mocked(estadosApi);

const ESTADOS: EstadoComCidadesDto[] = [
  {
    id: 1,
    nome: 'São Paulo',
    uf: 'SP',
    cidades: [
      { id: 10, nome: 'São Paulo', estadoId: 1 },
      { id: 11, nome: 'Campinas', estadoId: 1 },
    ],
  },
  {
    id: 2,
    nome: 'Minas Gerais',
    uf: 'MG',
    cidades: [{ id: 20, nome: 'Belo Horizonte', estadoId: 2 }],
  },
];

const COMPRADOR_EXISTENTE: CompradorResponseDto = {
  id: 1,
  nome: 'Frigorífico Sul',
  documento: '12345678900',
  cidadeId: 10,
};

beforeEach(() => {
  vi.clearAllMocks();
  compradoresApiMock.listar.mockResolvedValue([COMPRADOR_EXISTENTE]);
  estadosApiMock.listar.mockResolvedValue(ESTADOS);
});

afterEach(() => {
  vi.resetAllMocks();
});

describe('CompradoresPage - layout e navegação', () => {
  it('renderiza a tabela com lista de compradores e botão "Novo" no topo', async () => {
    renderWithProviders(<CompradoresPage />);

    await waitFor(() => expect(screen.getByText('Frigorífico Sul')).toBeInTheDocument());

    expect(screen.getByRole('link', { name: 'Novo' })).toHaveAttribute('href', '/compradores/novo');
  });

  it('botão "Novo" leva para a rota /compradores/novo', async () => {
    renderWithProviders(<CompradoresPage />);

    const botaoNovo = screen.getByRole('link', { name: 'Novo' });
    expect(botaoNovo).toHaveAttribute('href', '/compradores/novo');
  });

  it('botão "Editar" na tabela navega para a rota de edição', async () => {
    const user = userEvent.setup();
    renderWithProviders(<CompradoresPage />);

    await waitFor(() => expect(screen.getByText('Frigorífico Sul')).toBeInTheDocument());

    const linha = screen.getByText('Frigorífico Sul').closest('tr') as HTMLElement;
    const botaoEditar = within(linha).getByRole('button', { name: 'Editar' });

    await user.click(botaoEditar);

    expect(botaoEditar).toBeInTheDocument();
  });
});

describe('CompradoresPage - modal de exclusão', () => {
  it('cancelar o modal NÃO dispara requisição de exclusão', async () => {
    const user = userEvent.setup();
    renderWithProviders(<CompradoresPage />);

    await waitFor(() => expect(screen.getByText('Frigorífico Sul')).toBeInTheDocument());

    const linha = screen.getByText('Frigorífico Sul').closest('tr') as HTMLElement;
    await user.click(within(linha).getByRole('button', { name: 'Excluir' }));

    const modal = await screen.findByRole('dialog');
    await user.click(within(modal).getByRole('button', { name: 'Cancelar' }));

    await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());
    expect(compradoresApiMock.excluir).not.toHaveBeenCalled();
  });

  it('confirmar o modal dispara a exclusão', async () => {
    const user = userEvent.setup();
    compradoresApiMock.excluir.mockResolvedValue(undefined);
    renderWithProviders(<CompradoresPage />);

    await waitFor(() => expect(screen.getByText('Frigorífico Sul')).toBeInTheDocument());

    const linha = screen.getByText('Frigorífico Sul').closest('tr') as HTMLElement;
    await user.click(within(linha).getByRole('button', { name: 'Excluir' }));

    const modal = await screen.findByRole('dialog');
    await user.click(within(modal).getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => expect(compradoresApiMock.excluir).toHaveBeenCalledWith(1));
  });

  it('exibe a mensagem real de erro 409 (delete bloqueado) dentro do modal, sem fechar', async () => {
    const user = userEvent.setup();
    compradoresApiMock.excluir.mockRejectedValue(
      new ApiError(409, ['Não é possível excluir: existem pedidos vinculados a este comprador.']),
    );
    renderWithProviders(<CompradoresPage />);

    await waitFor(() => expect(screen.getByText('Frigorífico Sul')).toBeInTheDocument());

    const linha = screen.getByText('Frigorífico Sul').closest('tr') as HTMLElement;
    await user.click(within(linha).getByRole('button', { name: 'Excluir' }));

    const modal = await screen.findByRole('dialog');
    await user.click(within(modal).getByRole('button', { name: 'Confirmar' }));

    expect(
      await within(modal).findByText('Não é possível excluir: existem pedidos vinculados a este comprador.'),
    ).toBeInTheDocument();
    expect(screen.getByRole('dialog')).toBeInTheDocument();
  });

  it('mostra spinner e desabilita botão durante submissão de exclusão', async () => {
    const user = userEvent.setup();
    let resolveExcluir: (() => void) | undefined;
    const excluirPromise = new Promise<void>((resolve) => {
      resolveExcluir = resolve;
    });
    compradoresApiMock.excluir.mockReturnValue(excluirPromise);

    renderWithProviders(<CompradoresPage />);

    await waitFor(() => expect(screen.getByText('Frigorífico Sul')).toBeInTheDocument());

    const linha = screen.getByText('Frigorífico Sul').closest('tr') as HTMLElement;
    await user.click(within(linha).getByRole('button', { name: 'Excluir' }));

    const modal = await screen.findByRole('dialog');
    const botaoConfirmar = within(modal).getByRole('button', { name: 'Confirmar' });
    await user.click(botaoConfirmar);

    await waitFor(() => {
      expect(botaoConfirmar).toBeDisabled();
      const spinner = botaoConfirmar.querySelector('.loading-button__spinner');
      expect(spinner).toBeInTheDocument();
    });

    resolveExcluir?.();

    await waitFor(() => {
      expect(compradoresApiMock.excluir).toHaveBeenCalledWith(1);
    });
  });
});

describe('CompradoresPage - carregamento e erros', () => {
  it('exibe "Carregando compradores..." enquanto busca os dados', () => {
    compradoresApiMock.listar.mockImplementation(() => new Promise(() => {}));

    renderWithProviders(<CompradoresPage />);

    expect(screen.getByText('Carregando compradores...')).toBeInTheDocument();
  });

  it('exibe mensagem de erro ao falhar no carregamento', async () => {
    compradoresApiMock.listar.mockRejectedValue(new Error('Erro de conexão'));

    renderWithProviders(<CompradoresPage />);

    await waitFor(() => {
      expect(screen.getByText('Não foi possível carregar a lista de compradores.')).toBeInTheDocument();
    });
  });

  it('exibe "Nenhum comprador cadastrado." quando a lista está vazia', async () => {
    compradoresApiMock.listar.mockResolvedValue([]);

    renderWithProviders(<CompradoresPage />);

    await waitFor(() => {
      expect(screen.getByText('Nenhum comprador cadastrado.')).toBeInTheDocument();
    });
  });
});
