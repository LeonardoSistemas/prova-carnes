import { ApiError } from './client';

/**
 * Extrai as mensagens amigáveis de um erro capturado numa mutation. Se for
 * um `ApiError` (erro vindo da API), retorna `erros` (mensagens reais do
 * servidor). Caso contrário (erro de programação inesperado), retorna uma
 * mensagem genérica só como último recurso.
 */
export function getErrorMessages(error: unknown): string[] {
  if (error instanceof ApiError) {
    return error.erros;
  }

  return ['Ocorreu um erro inesperado. Tente novamente.'];
}
