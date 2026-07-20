import type { ErroRespostaDto } from './types';

/**
 * Cliente HTTP único e centralizado da aplicação. Todo módulo de
 * `src/api/*Api.ts` usa `apiClient`; nenhum componente React chama `fetch`
 * diretamente.
 */
const API_BASE_URL: string =
  (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? 'http://localhost:5299/api';

/**
 * Erro de API tipado — carrega o array `erros` vindo de `ErroRespostaDto`
 * (formato único do backend para 400/404/409/422/500) para que a camada de
 * UI sempre tenha acesso às mensagens reais retornadas pelo servidor, nunca
 * um texto genérico quando o servidor já forneceu uma mensagem amigável.
 */
export class ApiError extends Error {
  readonly status: number;
  readonly erros: string[];

  constructor(status: number, erros: string[]) {
    super(erros[0] ?? 'Ocorreu um erro inesperado.');
    this.name = 'ApiError';
    this.status = status;
    this.erros = erros;
  }
}

async function parseJsonSafely(response: Response): Promise<unknown> {
  const contentType = response.headers.get('content-type');
  if (!contentType?.includes('application/json')) {
    return null;
  }

  try {
    return await response.json();
  } catch {
    return null;
  }
}

function isErroResposta(value: unknown): value is ErroRespostaDto {
  return (
    typeof value === 'object' &&
    value !== null &&
    'erros' in value &&
    Array.isArray((value as { erros: unknown }).erros)
  );
}

async function request<TResponse>(path: string, init?: RequestInit): Promise<TResponse> {
  let response: Response;

  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      ...init,
      headers: {
        'Content-Type': 'application/json',
        ...init?.headers,
      },
    });
  } catch {
    throw new ApiError(0, [
      'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.',
    ]);
  }

  if (response.status === 204) {
    return undefined as TResponse;
  }

  const body = await parseJsonSafely(response);

  if (!response.ok) {
    const erros = isErroResposta(body) ? body.erros : ['Ocorreu um erro inesperado. Tente novamente.'];
    throw new ApiError(response.status, erros);
  }

  return body as TResponse;
}

export const apiClient = {
  get: <TResponse>(path: string): Promise<TResponse> => request<TResponse>(path, { method: 'GET' }),

  post: <TResponse>(path: string, data: unknown): Promise<TResponse> =>
    request<TResponse>(path, { method: 'POST', body: JSON.stringify(data) }),

  put: <TResponse = void>(path: string, data: unknown): Promise<TResponse> =>
    request<TResponse>(path, { method: 'PUT', body: JSON.stringify(data) }),

  delete: (path: string): Promise<void> => request<void>(path, { method: 'DELETE' }),
};
