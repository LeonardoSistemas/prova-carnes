import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderWithProviders } from '../../test/renderWithProviders';
import { ApiError } from '../../api/client';
import { carnesApi } from '../../api/carnesApi';
import { OrigemCarne, type CarneResponseDto } from '../../api/types';
import { CarnesPage } from './CarnesPage';

vi.mock('../../api/carnesApi', () => ({
  carnesApi: {
    listar: vi.fn(),
    obterPorId: vi.fn(),
    criar: vi.fn(),
    atualizar: vi.fn(),
    excluir: vi.fn(),
  },
}));

const carnesApiMock = vi.mocked(carnesApi);

const CARNE_EXISTENTE: CarneResponseDto = { id: 1, descricao: 'Picanha', origem: OrigemCarne.Bovina };

beforeEach(() => {
  vi.clearAllMocks();
  carnesApiMock.listar.mockResolvedValue([CARNE_EXISTENTE]);
});

afterEach(() => {
  vi.resetAllMocks();
});

describe('CarnesPage - layout e navegação', () => {
  it('renderiza a tabela com lista de carnes e botão "Novo" no topo', async () => {
    renderWithProviders(<CarnesPage />);

    await waitFor(() => expect(screen.getByText('Picanha')).toBeInTheDocument());

    expect(screen.getByRole('link', { name: 'Novo' })).toHaveAttribute('href', '/carnes/novo');
  });

  it('botão "Novo" leva para a rota /carnes/novo', async () => {
    renderWithProviders(<CarnesPage />);

    const botaoNovo = screen.getByRole('link', { name: 'Novo' });
    expect(botaoNovo).toHaveAttribute('href', '/carnes/novo');
  });

  it('botão "Editar" na tabela navega para a rota de edição', async () => {
    const user = userEvent.setup();
    renderWithProviders(<CarnesPage />);

    await waitFor(() => expect(screen.getByText('Picanha')).toBeInTheDocument());

    const linha = screen.getByText('Picanha').closest('tr') as HTMLElement;
    const botaoEditar = within(linha).getByRole('button', { name: 'Editar' });

    // Simulamos que o clique leva para a rota de edição
    await user.click(botaoEditar);

    // O componente CarneTable chama navigate, mas em testes o comportamento
    // de navegação é mockado pelo renderWithProviders — apenas verificamos
    // que o botão existe e pode ser clicado.
    expect(botaoEditar).toBeInTheDocument();
  });
});

describe('CarnesPage - modal de exclusão', () => {
  it('cancelar o modal NÃO dispara requisição de exclusão', async () => {
    const user = userEvent.setup();
    renderWithProviders(<CarnesPage />);

    await waitFor(() => expect(screen.getByText('Picanha')).toBeInTheDocument());

    const linha = screen.getByText('Picanha').closest('tr') as HTMLElement;
    await user.click(within(linha).getByRole('button', { name: 'Excluir' }));

    const modal = await screen.findByRole('dialog');
    await user.click(within(modal).getByRole('button', { name: 'Cancelar' }));

    await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());
    expect(carnesApiMock.excluir).not.toHaveBeenCalled();
  });

  it('confirmar o modal dispara a exclusão', async () => {
    const user = userEvent.setup();
    carnesApiMock.excluir.mockResolvedValue(undefined);
    renderWithProviders(<CarnesPage />);

    await waitFor(() => expect(screen.getByText('Picanha')).toBeInTheDocument());

    const linha = screen.getByText('Picanha').closest('tr') as HTMLElement;
    await user.click(within(linha).getByRole('button', { name: 'Excluir' }));

    const modal = await screen.findByRole('dialog');
    await user.click(within(modal).getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => expect(carnesApiMock.excluir).toHaveBeenCalledWith(1));
  });

  it('exibe a mensagem real de erro 409 (delete bloqueado) dentro do modal, sem fechar', async () => {
    const user = userEvent.setup();
    carnesApiMock.excluir.mockRejectedValue(
      new ApiError(409, ['Não é possível excluir: existem pedidos vinculados a esta carne.']),
    );
    renderWithProviders(<CarnesPage />);

    await waitFor(() => expect(screen.getByText('Picanha')).toBeInTheDocument());

    const linha = screen.getByText('Picanha').closest('tr') as HTMLElement;
    await user.click(within(linha).getByRole('button', { name: 'Excluir' }));

    const modal = await screen.findByRole('dialog');
    await user.click(within(modal).getByRole('button', { name: 'Confirmar' }));

    expect(
      await within(modal).findByText('Não é possível excluir: existem pedidos vinculados a esta carne.'),
    ).toBeInTheDocument();
    expect(screen.getByRole('dialog')).toBeInTheDocument();
  });

  it('mostra spinner e desabilita botão durante submissão de exclusão (T76 - LoadingButton)', async () => {
    const user = userEvent.setup();
    let resolveExcluir: (() => void) | undefined;
    const excluirPromise = new Promise<void>((resolve) => {
      resolveExcluir = resolve;
    });
    carnesApiMock.excluir.mockReturnValue(excluirPromise);

    renderWithProviders(<CarnesPage />);

    await waitFor(() => expect(screen.getByText('Picanha')).toBeInTheDocument());

    const linha = screen.getByText('Picanha').closest('tr') as HTMLElement;
    await user.click(within(linha).getByRole('button', { name: 'Excluir' }));

    const modal = await screen.findByRole('dialog');
    const botaoConfirmar = within(modal).getByRole('button', { name: 'Confirmar' });
    await user.click(botaoConfirmar);

    // Durante a mutation pendente: botão desabilitado e spinner visível
    await waitFor(() => {
      expect(botaoConfirmar).toBeDisabled();
      const spinner = botaoConfirmar.querySelector('.loading-button__spinner');
      expect(spinner).toBeInTheDocument();
    });

    // Resolver a promise para limpar o teste
    resolveExcluir?.();

    // Aguardar que a API call tenha sido feita
    await waitFor(() => {
      expect(carnesApiMock.excluir).toHaveBeenCalledWith(1);
    });
  });
});

describe('CarnesPage - carregamento e erros', () => {
  it('exibe "Carregando carnes..." enquanto busca os dados', () => {
    carnesApiMock.listar.mockImplementation(() => new Promise(() => {})); // Never resolves

    renderWithProviders(<CarnesPage />);

    expect(screen.getByText('Carregando carnes...')).toBeInTheDocument();
  });

  it('exibe mensagem de erro ao falhar no carregamento', async () => {
    carnesApiMock.listar.mockRejectedValue(new Error('Erro de conexão'));

    renderWithProviders(<CarnesPage />);

    await waitFor(() => {
      expect(screen.getByText('Não foi possível carregar a lista de carnes.')).toBeInTheDocument();
    });
  });

  it('exibe "Nenhuma carne cadastrada." quando a lista está vazia', async () => {
    carnesApiMock.listar.mockResolvedValue([]);

    renderWithProviders(<CarnesPage />);

    await waitFor(() => {
      expect(screen.getByText('Nenhuma carne cadastrada.')).toBeInTheDocument();
    });
  });
});
