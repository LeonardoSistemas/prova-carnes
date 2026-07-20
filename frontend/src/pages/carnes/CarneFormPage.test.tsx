import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { render } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { carnesApi } from '../../api/carnesApi';
import { OrigemCarne, type CarneResponseDto } from '../../api/types';
import { CarneFormPage } from './CarneFormPage';

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
});

afterEach(() => {
  vi.resetAllMocks();
});

/**
 * Helper para renderizar `CarneFormPage` com rotas corretamente configuradas,
 * de forma que `useParams` funcione.
 */
function renderCarneFormPageWithRoute(initialRoute: string) {
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
          <Route path="/carnes/novo" element={<CarneFormPage />} />
          <Route path="/carnes/:id/editar" element={<CarneFormPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('CarneFormPage - criação de nova carne', () => {
  it('renderiza o formulário vazio com título "Nova carne"', () => {
    renderCarneFormPageWithRoute('/carnes/novo');

    expect(screen.getByRole('heading', { name: 'Nova carne' })).toBeInTheDocument();
    expect(screen.getByRole('textbox', { name: /Descrição/ })).toHaveValue('');
    expect(screen.getByRole('combobox', { name: /Origem/ })).toHaveValue('');
  });

  it('exibe erros de validação ao submeter sem preenchimento', async () => {
    const user = userEvent.setup();
    renderCarneFormPageWithRoute('/carnes/novo');

    await user.click(screen.getByRole('button', { name: 'Cadastrar carne' }));

    expect(await screen.findByText('Descrição é obrigatória.')).toBeInTheDocument();
    expect(screen.getByText('Origem é obrigatória.')).toBeInTheDocument();
    expect(carnesApiMock.criar).not.toHaveBeenCalled();
  });

  it('submete o formulário com valores válidos e navega de volta para /carnes', async () => {
    const user = userEvent.setup();
    carnesApiMock.criar.mockResolvedValue({ id: 2, descricao: 'Alcatra', origem: OrigemCarne.Bovina });

    renderCarneFormPageWithRoute('/carnes/novo');

    await user.type(screen.getByRole('textbox', { name: /Descrição/ }), 'Alcatra');
    await user.selectOptions(screen.getByRole('combobox', { name: /Origem/ }), String(OrigemCarne.Bovina));
    await user.click(screen.getByRole('button', { name: 'Cadastrar carne' }));

    await waitFor(() =>
      expect(carnesApiMock.criar).toHaveBeenCalledWith({
        descricao: 'Alcatra',
        origem: OrigemCarne.Bovina,
      }),
    );
  });

  it('focus automático no campo Descrição (T77)', () => {
    renderCarneFormPageWithRoute('/carnes/novo');

    const inputDescricao = screen.getByRole('textbox', { name: /Descrição/ }) as HTMLInputElement;
    expect(document.activeElement).toBe(inputDescricao);
  });
});

describe('CarneFormPage - edição de carne existente', () => {
  it('carrega a carne existente e renderiza o formulário preenchido', async () => {
    carnesApiMock.obterPorId.mockResolvedValue(CARNE_EXISTENTE);

    renderCarneFormPageWithRoute('/carnes/1/editar');

    await waitFor(() => expect(screen.getByRole('textbox', { name: /Descrição/ })).toHaveValue('Picanha'));

    expect(screen.getByRole('heading', { name: /Editar carne #1/ })).toBeInTheDocument();
    expect(screen.getByRole('combobox', { name: /Origem/ })).toHaveValue(String(OrigemCarne.Bovina));
    expect(screen.getByRole('button', { name: 'Salvar alterações' })).toBeInTheDocument();
  });

  it('exibe "Carregando carne..." enquanto busca os dados no modo de edição', () => {
    carnesApiMock.obterPorId.mockImplementation(() => new Promise(() => {})); // Never resolves

    renderCarneFormPageWithRoute('/carnes/1/editar');

    expect(screen.getByText('Carregando carne...')).toBeInTheDocument();
  });

  it('submete alterações e navega de volta para /carnes', async () => {
    const user = userEvent.setup();
    carnesApiMock.obterPorId.mockResolvedValue(CARNE_EXISTENTE);
    carnesApiMock.atualizar.mockResolvedValue(undefined);

    renderCarneFormPageWithRoute('/carnes/1/editar');

    await waitFor(() => expect(screen.getByRole('textbox', { name: /Descrição/ })).toHaveValue('Picanha'));

    await user.clear(screen.getByRole('textbox', { name: /Descrição/ }));
    await user.type(screen.getByRole('textbox', { name: /Descrição/ }), 'Picanha Premium');
    await user.click(screen.getByRole('button', { name: 'Salvar alterações' }));

    await waitFor(() =>
      expect(carnesApiMock.atualizar).toHaveBeenCalledWith(1, {
        descricao: 'Picanha Premium',
        origem: OrigemCarne.Bovina,
      }),
    );
  });

  it('preserva dados do formulário enquanto valida após tentar submeter com erro', async () => {
    const user = userEvent.setup();
    carnesApiMock.obterPorId.mockResolvedValue(CARNE_EXISTENTE);
    carnesApiMock.atualizar.mockRejectedValue(new Error('Erro de servidor'));

    renderCarneFormPageWithRoute('/carnes/1/editar');

    await waitFor(() => expect(screen.getByRole('textbox', { name: /Descrição/ })).toHaveValue('Picanha'));

    // Tentar submeter (vai falhar)
    await user.click(screen.getByRole('button', { name: 'Salvar alterações' }));

    // Os dados devem estar preservados mesmo após erro
    await waitFor(() => {
      expect(screen.getByRole('textbox', { name: /Descrição/ })).toHaveValue('Picanha');
      expect(screen.getByRole('combobox', { name: /Origem/ })).toHaveValue(String(OrigemCarne.Bovina));
    });
  });
});

describe('CarneFormPage - cancelamento', () => {
  it('botão Cancelar em modo criação navega de volta para /carnes sem enviar dados', async () => {
    const user = userEvent.setup();
    renderCarneFormPageWithRoute('/carnes/novo');

    await user.type(screen.getByRole('textbox', { name: /Descrição/ }), 'Teste');
    await user.click(screen.getByRole('button', { name: 'Cancelar' }));

    expect(carnesApiMock.criar).not.toHaveBeenCalled();
  });

  it('botão Cancelar em modo edição navega de volta para /carnes sem enviar dados', async () => {
    const user = userEvent.setup();
    carnesApiMock.obterPorId.mockResolvedValue(CARNE_EXISTENTE);

    renderCarneFormPageWithRoute('/carnes/1/editar');

    await waitFor(() => expect(screen.getByRole('textbox', { name: /Descrição/ })).toHaveValue('Picanha'));

    await user.clear(screen.getByRole('textbox', { name: /Descrição/ }));
    await user.type(screen.getByRole('textbox', { name: /Descrição/ }), 'Alteração não salva');
    await user.click(screen.getByRole('button', { name: 'Cancelar' }));

    expect(carnesApiMock.atualizar).not.toHaveBeenCalled();
  });
});

describe('CarneFormPage - validação de campos', () => {
  it('valida campo Descrição obrigatório', async () => {
    const user = userEvent.setup();
    renderCarneFormPageWithRoute('/carnes/novo');

    const inputDescricao = screen.getByRole('textbox', { name: /Descrição/ });

    // Submeter sem descrição
    await user.click(screen.getByRole('button', { name: 'Cadastrar carne' }));
    expect(await screen.findByText('Descrição é obrigatória.')).toBeInTheDocument();

    // Preencher descrição
    await user.type(inputDescricao, 'Nova Carne');
    expect(screen.queryByText('Descrição é obrigatória.')).not.toBeInTheDocument();
  });

  it('valida campo Origem obrigatório', async () => {
    const user = userEvent.setup();
    renderCarneFormPageWithRoute('/carnes/novo');

    const selectOrigem = screen.getByRole('combobox', { name: /Origem/ });

    // Submeter sem origem
    await user.click(screen.getByRole('button', { name: 'Cadastrar carne' }));
    expect(await screen.findByText('Origem é obrigatória.')).toBeInTheDocument();

    // Selecionar origem
    await user.selectOptions(selectOrigem, String(OrigemCarne.Bovina));
    expect(screen.queryByText('Origem é obrigatória.')).not.toBeInTheDocument();
  });
});

describe('CarneFormPage - LoadingButton (T76)', () => {
  it('mostra spinner e desabilita botão durante submissão', async () => {
    const user = userEvent.setup();
    let resolveCriar: ((value: CarneResponseDto) => void) | undefined;
    const criarPromise = new Promise<CarneResponseDto>((resolve) => {
      resolveCriar = resolve;
    });
    carnesApiMock.criar.mockReturnValue(criarPromise);

    renderCarneFormPageWithRoute('/carnes/novo');

    await user.type(screen.getByRole('textbox', { name: /Descrição/ }), 'Alcatra');
    await user.selectOptions(screen.getByRole('combobox', { name: /Origem/ }), String(OrigemCarne.Bovina));

    const submitButton = screen.getByRole('button', { name: 'Cadastrar carne' });
    await user.click(submitButton);

    // Durante a mutation pendente: botão desabilitado e spinner visível
    await waitFor(() => {
      expect(submitButton).toBeDisabled();
      const spinner = submitButton.querySelector('.loading-button__spinner');
      expect(spinner).toBeInTheDocument();
    });

    // Resolver a promise para limpar o teste
    resolveCriar?.({ id: 2, descricao: 'Alcatra', origem: OrigemCarne.Bovina });

    // Aguardar que a API call tenha sido feita
    await waitFor(() => {
      expect(carnesApiMock.criar).toHaveBeenCalledWith({
        descricao: 'Alcatra',
        origem: OrigemCarne.Bovina,
      });
    });
  });
});
