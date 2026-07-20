import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { render } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { compradoresApi } from '../../api/compradoresApi';
import { estadosApi } from '../../api/estadosApi';
import type { CompradorResponseDto, EstadoComCidadesDto } from '../../api/types';
import { CompradorFormPage } from './CompradorFormPage';

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
  estadosApiMock.listar.mockResolvedValue(ESTADOS);
});

afterEach(() => {
  vi.resetAllMocks();
});

/**
 * Helper para renderizar `CompradorFormPage` com rotas corretamente configuradas,
 * de forma que `useParams` funcione.
 */
function renderCompradorFormPageWithRoute(initialRoute: string) {
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
          <Route path="/compradores/novo" element={<CompradorFormPage />} />
          <Route path="/compradores/:id/editar" element={<CompradorFormPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('CompradorFormPage - criação de novo comprador', () => {
  it('renderiza o formulário vazio com título "Novo comprador"', async () => {
    renderCompradorFormPageWithRoute('/compradores/novo');

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Novo comprador' })).toBeInTheDocument());
    expect(screen.getByLabelText(/Nome/)).toHaveValue('');
    expect(screen.getByLabelText(/Documento/)).toHaveValue('');
    expect(screen.getByLabelText(/Estado/)).toHaveValue('');
    expect(screen.getByLabelText(/Cidade/)).toHaveValue('');
  });

  it('exibe erros de validação ao submeter sem preenchimento', async () => {
    const user = userEvent.setup();
    renderCompradorFormPageWithRoute('/compradores/novo');

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Novo comprador' })).toBeInTheDocument());

    await user.click(screen.getByRole('button', { name: 'Cadastrar comprador' }));

    expect(await screen.findByText('Nome é obrigatório.')).toBeInTheDocument();
    expect(screen.getByText('Documento é obrigatório.')).toBeInTheDocument();
    expect(screen.getByText('Estado é obrigatório.')).toBeInTheDocument();
    expect(screen.getByText('Cidade é obrigatória.')).toBeInTheDocument();
    expect(compradoresApiMock.criar).not.toHaveBeenCalled();
  });

  it('submete o formulário com valores válidos e navega de volta para /compradores', async () => {
    const user = userEvent.setup();
    compradoresApiMock.criar.mockResolvedValue({ id: 2, nome: 'Novo Comprador', documento: '98765432100', cidadeId: 10 });

    renderCompradorFormPageWithRoute('/compradores/novo');

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Novo comprador' })).toBeInTheDocument());

    await user.type(screen.getByLabelText(/Nome/), 'Novo Comprador');
    await user.type(screen.getByLabelText(/Documento/), '98765432100');
    await user.selectOptions(screen.getByLabelText(/Estado/), '1');
    await user.selectOptions(screen.getByLabelText(/Cidade/), '10');
    await user.click(screen.getByRole('button', { name: 'Cadastrar comprador' }));

    await waitFor(() =>
      expect(compradoresApiMock.criar).toHaveBeenCalledWith({
        nome: 'Novo Comprador',
        documento: '98765432100',
        cidadeId: 10,
      }),
    );
  });

  it('focus automático no campo Nome', async () => {
    renderCompradorFormPageWithRoute('/compradores/novo');

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Novo comprador' })).toBeInTheDocument());

    const inputNome = screen.getByLabelText(/Nome/) as HTMLInputElement;
    expect(document.activeElement).toBe(inputNome);
  });
});

describe('CompradorFormPage - edição de comprador existente', () => {
  it('carrega o comprador existente e renderiza o formulário preenchido', async () => {
    compradoresApiMock.obterPorId.mockResolvedValue(COMPRADOR_EXISTENTE);

    renderCompradorFormPageWithRoute('/compradores/1/editar');

    await waitFor(() => expect(screen.getByLabelText(/Nome/)).toHaveValue('Frigorífico Sul'));

    expect(screen.getByRole('heading', { name: /Editar comprador #1/ })).toBeInTheDocument();
    expect(screen.getByLabelText(/Documento/)).toHaveValue('123.456.789-00');
    expect(screen.getByLabelText(/Estado/)).toHaveValue('1');
    expect(screen.getByLabelText(/Cidade/)).toHaveValue('10');
    expect(screen.getByRole('button', { name: 'Salvar alterações' })).toBeInTheDocument();
  });

  it('exibe "Carregando..." enquanto busca os dados no modo de edição', () => {
    compradoresApiMock.obterPorId.mockImplementation(() => new Promise(() => {}));
    estadosApiMock.listar.mockImplementation(() => new Promise(() => {}));

    renderCompradorFormPageWithRoute('/compradores/1/editar');

    expect(screen.getByText('Carregando...')).toBeInTheDocument();
  });

  it('submete alterações e navega de volta para /compradores', async () => {
    const user = userEvent.setup();
    compradoresApiMock.obterPorId.mockResolvedValue(COMPRADOR_EXISTENTE);
    compradoresApiMock.atualizar.mockResolvedValue(undefined);

    renderCompradorFormPageWithRoute('/compradores/1/editar');

    await waitFor(() => expect(screen.getByLabelText(/Nome/)).toHaveValue('Frigorífico Sul'));

    await user.clear(screen.getByLabelText(/Nome/));
    await user.type(screen.getByLabelText(/Nome/), 'Frigorífico Sul Premium');
    await user.click(screen.getByRole('button', { name: 'Salvar alterações' }));

    await waitFor(() =>
      expect(compradoresApiMock.atualizar).toHaveBeenCalledWith(1, {
        nome: 'Frigorífico Sul Premium',
        documento: '12345678900',
        cidadeId: 10,
      }),
    );
  });

  it('preserva dados do formulário enquanto valida após tentar submeter com erro', async () => {
    const user = userEvent.setup();
    compradoresApiMock.obterPorId.mockResolvedValue(COMPRADOR_EXISTENTE);
    compradoresApiMock.atualizar.mockRejectedValue(new Error('Erro de servidor'));

    renderCompradorFormPageWithRoute('/compradores/1/editar');

    await waitFor(() => expect(screen.getByLabelText(/Nome/)).toHaveValue('Frigorífico Sul'));

    await user.click(screen.getByRole('button', { name: 'Salvar alterações' }));

    await waitFor(() => {
      expect(screen.getByLabelText(/Nome/)).toHaveValue('Frigorífico Sul');
      expect(screen.getByLabelText(/Documento/)).toHaveValue('123.456.789-00');
      expect(screen.getByLabelText(/Estado/)).toHaveValue('1');
      expect(screen.getByLabelText(/Cidade/)).toHaveValue('10');
    });
  });
});

describe('CompradorFormPage - cancelamento', () => {
  it('botão Cancelar em modo criação navega de volta para /compradores sem enviar dados', async () => {
    const user = userEvent.setup();
    renderCompradorFormPageWithRoute('/compradores/novo');

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Novo comprador' })).toBeInTheDocument());

    await user.type(screen.getByLabelText(/Nome/), 'Teste');
    await user.click(screen.getByRole('button', { name: 'Cancelar' }));

    expect(compradoresApiMock.criar).not.toHaveBeenCalled();
  });

  it('botão Cancelar em modo edição navega de volta para /compradores sem enviar dados', async () => {
    const user = userEvent.setup();
    compradoresApiMock.obterPorId.mockResolvedValue(COMPRADOR_EXISTENTE);

    renderCompradorFormPageWithRoute('/compradores/1/editar');

    await waitFor(() => expect(screen.getByLabelText(/Nome/)).toHaveValue('Frigorífico Sul'));

    await user.clear(screen.getByLabelText(/Nome/));
    await user.type(screen.getByLabelText(/Nome/), 'Alteração não salva');
    await user.click(screen.getByRole('button', { name: 'Cancelar' }));

    expect(compradoresApiMock.atualizar).not.toHaveBeenCalled();
  });
});

describe('CompradorFormPage - validação de campos', () => {
  it('valida campo Nome obrigatório', async () => {
    const user = userEvent.setup();
    renderCompradorFormPageWithRoute('/compradores/novo');

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Novo comprador' })).toBeInTheDocument());

    const inputNome = screen.getByLabelText(/Nome/);

    await user.click(screen.getByRole('button', { name: 'Cadastrar comprador' }));
    expect(await screen.findByText('Nome é obrigatório.')).toBeInTheDocument();

    await user.type(inputNome, 'Novo Comprador');
    expect(screen.queryByText('Nome é obrigatório.')).not.toBeInTheDocument();
  });

  it('valida campo Documento obrigatório', async () => {
    const user = userEvent.setup();
    renderCompradorFormPageWithRoute('/compradores/novo');

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Novo comprador' })).toBeInTheDocument());

    const inputDocumento = screen.getByLabelText(/Documento/);

    await user.click(screen.getByRole('button', { name: 'Cadastrar comprador' }));
    expect(await screen.findByText('Documento é obrigatório.')).toBeInTheDocument();

    await user.type(inputDocumento, '12345678900');
    expect(screen.queryByText('Documento é obrigatório.')).not.toBeInTheDocument();
  });

  it('valida campo Estado obrigatório', async () => {
    const user = userEvent.setup();
    renderCompradorFormPageWithRoute('/compradores/novo');

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Novo comprador' })).toBeInTheDocument());

    const selectEstado = screen.getByLabelText(/Estado/);

    await user.click(screen.getByRole('button', { name: 'Cadastrar comprador' }));
    expect(await screen.findByText('Estado é obrigatório.')).toBeInTheDocument();

    await user.selectOptions(selectEstado, '1');
    expect(screen.queryByText('Estado é obrigatório.')).not.toBeInTheDocument();
  });

  it('valida campo Cidade obrigatório', async () => {
    const user = userEvent.setup();
    renderCompradorFormPageWithRoute('/compradores/novo');

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Novo comprador' })).toBeInTheDocument());

    const selectCidade = screen.getByLabelText(/Cidade/);

    await user.click(screen.getByRole('button', { name: 'Cadastrar comprador' }));
    expect(await screen.findByText('Cidade é obrigatória.')).toBeInTheDocument();

    // Tem que selecionar Estado antes de poder selecionar Cidade
    await user.selectOptions(screen.getByLabelText(/Estado/), '1');
    await user.selectOptions(selectCidade, '10');
    expect(screen.queryByText('Cidade é obrigatória.')).not.toBeInTheDocument();
  });
});

describe('CompradorFormPage - combobox Estado -> Cidade em cascata', () => {
  it('filtra as cidades disponíveis de acordo com o estado selecionado', async () => {
    const user = userEvent.setup();
    renderCompradorFormPageWithRoute('/compradores/novo');

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Novo comprador' })).toBeInTheDocument());

    const selectCidade = screen.getByLabelText(/Cidade/) as HTMLSelectElement;
    expect(selectCidade).toBeDisabled();

    await user.selectOptions(screen.getByLabelText(/Estado/), '1');

    expect(selectCidade).toBeEnabled();
    expect(within(selectCidade).getByText('São Paulo')).toBeInTheDocument();
    expect(within(selectCidade).getByText('Campinas')).toBeInTheDocument();
    expect(within(selectCidade).queryByText('Belo Horizonte')).not.toBeInTheDocument();

    await user.selectOptions(screen.getByLabelText(/Estado/), '2');

    expect(within(selectCidade).getByText('Belo Horizonte')).toBeInTheDocument();
    expect(within(selectCidade).queryByText('Campinas')).not.toBeInTheDocument();
  });

  it('reseta Cidade quando Estado muda', async () => {
    const user = userEvent.setup();
    renderCompradorFormPageWithRoute('/compradores/novo');

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Novo comprador' })).toBeInTheDocument());

    // Seleciona SP e depois Campinas
    await user.selectOptions(screen.getByLabelText(/Estado/), '1');
    await user.selectOptions(screen.getByLabelText(/Cidade/), '11');
    expect(screen.getByLabelText(/Cidade/)).toHaveValue('11');

    // Muda para MG — Cidade deve resetar
    await user.selectOptions(screen.getByLabelText(/Estado/), '2');
    expect(screen.getByLabelText(/Cidade/)).toHaveValue('');
  });
});

describe('CompradorFormPage - LoadingButton', () => {
  it('mostra spinner e desabilita botão durante submissão', async () => {
    const user = userEvent.setup();
    let resolveCriar: ((value: CompradorResponseDto) => void) | undefined;
    const criarPromise = new Promise<CompradorResponseDto>((resolve) => {
      resolveCriar = resolve;
    });
    compradoresApiMock.criar.mockReturnValue(criarPromise);

    renderCompradorFormPageWithRoute('/compradores/novo');

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Novo comprador' })).toBeInTheDocument());

    await user.type(screen.getByLabelText(/Nome/), 'Novo Comprador');
    await user.type(screen.getByLabelText(/Documento/), '98765432100');
    await user.selectOptions(screen.getByLabelText(/Estado/), '1');
    await user.selectOptions(screen.getByLabelText(/Cidade/), '10');

    const submitButton = screen.getByRole('button', { name: 'Cadastrar comprador' });
    await user.click(submitButton);

    await waitFor(() => {
      expect(submitButton).toBeDisabled();
      const spinner = submitButton.querySelector('.loading-button__spinner');
      expect(spinner).toBeInTheDocument();
    });

    resolveCriar?.({ id: 2, nome: 'Novo Comprador', documento: '98765432100', cidadeId: 10 });

    await waitFor(() => {
      expect(compradoresApiMock.criar).toHaveBeenCalledWith({
        nome: 'Novo Comprador',
        documento: '98765432100',
        cidadeId: 10,
      });
    });
  });
});
