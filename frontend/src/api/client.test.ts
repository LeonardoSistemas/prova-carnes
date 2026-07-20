import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import { apiClient, ApiError } from './client';

// Salvar o fetch original
const originalFetch = global.fetch;

beforeEach(() => {
  vi.clearAllMocks();
});

afterEach(() => {
  // Restaurar fetch original
  global.fetch = originalFetch;
});

describe('apiClient.get', () => {
  it('retorna dados quando resposta é OK com JSON', async () => {
    const mockData = { id: 1, nome: 'Test' };
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      headers: new Headers({ 'content-type': 'application/json' }),
      json: vi.fn().mockResolvedValue(mockData),
    });

    global.fetch = mockFetch as any;

    const resultado = await apiClient.get('/test');

    expect(resultado).toEqual(mockData);
    expect(mockFetch).toHaveBeenCalledWith(expect.stringContaining('/test'), expect.objectContaining({ method: 'GET' }));
  });

  it('retorna undefined para status 204 sem tentar parsear body', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 204,
      headers: new Headers({ 'content-type': 'application/json' }),
    });

    global.fetch = mockFetch as any;

    const resultado = await apiClient.get('/test');

    expect(resultado).toBeUndefined();
    // json() nunca deve ser chamado para 204
    expect(mockFetch).toHaveBeenCalled();
  });

  it('lança ApiError com array de erros quando resposta não-OK com formato correto', async () => {
    const erros = ['Erro 1', 'Erro 2'];
    const mockFetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 422,
      headers: new Headers({ 'content-type': 'application/json' }),
      json: vi.fn().mockResolvedValue({ erros }),
    });

    global.fetch = mockFetch as any;

    await expect(apiClient.get('/test')).rejects.toThrow(ApiError);

    try {
      await apiClient.get('/test');
    } catch (error) {
      if (error instanceof ApiError) {
        expect(error.status).toBe(422);
        expect(error.erros).toEqual(erros);
      }
    }
  });

  it('lança ApiError com mensagem genérica quando resposta não-OK com body fora do formato', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 500,
      headers: new Headers({ 'content-type': 'application/json' }),
      json: vi.fn().mockResolvedValue({ algumOutroCampo: 'valor' }),
    });

    global.fetch = mockFetch as any;

    await expect(apiClient.get('/test')).rejects.toMatchObject({
      status: 500,
      erros: expect.arrayContaining(['Ocorreu um erro inesperado. Tente novamente.']),
    });
  });

  it('lança ApiError com mensagem padrão quando fetch rejeita (rede fora)', async () => {
    const mockFetch = vi.fn().mockRejectedValue(new Error('Network error'));

    global.fetch = mockFetch as any;

    await expect(apiClient.get('/test')).rejects.toMatchObject({
      status: 0,
      erros: [
        'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.',
      ],
    });
  });

  it('envia Content-Type header', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      headers: new Headers({ 'content-type': 'application/json' }),
      json: vi.fn().mockResolvedValue({}),
    });

    global.fetch = mockFetch as any;

    await apiClient.get('/test');

    const callArgs = mockFetch.mock.calls[0][1];
    expect(callArgs.headers['Content-Type']).toBe('application/json');
  });
});

describe('apiClient.post', () => {
  it('envia dados como JSON e retorna resposta', async () => {
    const dadosEnviados = { nome: 'Test', valor: 123 };
    const mockResposta = { id: 1, ...dadosEnviados };

    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 201,
      headers: new Headers({ 'content-type': 'application/json' }),
      json: vi.fn().mockResolvedValue(mockResposta),
    });

    global.fetch = mockFetch as any;

    const resultado = await apiClient.post('/test', dadosEnviados);

    expect(resultado).toEqual(mockResposta);
    const callArgs = mockFetch.mock.calls[0][1];
    expect(callArgs.method).toBe('POST');
    expect(callArgs.body).toBe(JSON.stringify(dadosEnviados));
  });

  it('lança ApiError quando POST falha', async () => {
    const erros = ['Dados inválidos'];
    const mockFetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 400,
      headers: new Headers({ 'content-type': 'application/json' }),
      json: vi.fn().mockResolvedValue({ erros }),
    });

    global.fetch = mockFetch as any;

    await expect(apiClient.post('/test', { nome: 'Test' })).rejects.toMatchObject({
      status: 400,
      erros,
    });
  });
});

describe('apiClient.put', () => {
  it('envia dados como JSON com método PUT', async () => {
    const dadosEnviados = { nome: 'Updated' };

    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 204,
      headers: new Headers({}),
    });

    global.fetch = mockFetch as any;

    const resultado = await apiClient.put('/test/1', dadosEnviados);

    expect(resultado).toBeUndefined();
    const callArgs = mockFetch.mock.calls[0][1];
    expect(callArgs.method).toBe('PUT');
    expect(callArgs.body).toBe(JSON.stringify(dadosEnviados));
  });
});

describe('apiClient.delete', () => {
  it('envia DELETE request', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 204,
      headers: new Headers({}),
    });

    global.fetch = mockFetch as any;

    const resultado = await apiClient.delete('/test/1');

    expect(resultado).toBeUndefined();
    const callArgs = mockFetch.mock.calls[0][1];
    expect(callArgs.method).toBe('DELETE');
  });

  it('lança ApiError quando DELETE falha', async () => {
    const erros = ['Não é possível excluir'];
    const mockFetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 409,
      headers: new Headers({ 'content-type': 'application/json' }),
      json: vi.fn().mockResolvedValue({ erros }),
    });

    global.fetch = mockFetch as any;

    await expect(apiClient.delete('/test/1')).rejects.toMatchObject({
      status: 409,
      erros,
    });
  });
});

describe('apiClient - response sem content-type JSON', () => {
  it('retorna null quando content-type não é JSON', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      headers: new Headers({ 'content-type': 'text/plain' }),
      json: vi.fn().mockRejectedValue(new Error('Not JSON')),
    });

    global.fetch = mockFetch as any;

    const resultado = await apiClient.get('/test');

    expect(resultado).toBeNull();
  });
});

describe('apiClient - error handling', () => {
  it('trata erro de rede corretamente', async () => {
    const mockFetch = vi.fn().mockRejectedValue(new TypeError('Failed to fetch'));

    global.fetch = mockFetch as any;

    await expect(apiClient.get('/test')).rejects.toMatchObject({
      status: 0,
      erros: [
        'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.',
      ],
      message: 'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.',
    });
  });

  it('trata erro de parsing JSON gracefully', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 500,
      headers: new Headers({ 'content-type': 'application/json' }),
      json: vi.fn().mockRejectedValue(new Error('Invalid JSON')),
    });

    global.fetch = mockFetch as any;

    await expect(apiClient.get('/test')).rejects.toMatchObject({
      status: 500,
      erros: ['Ocorreu um erro inesperado. Tente novamente.'],
    });
  });
});
