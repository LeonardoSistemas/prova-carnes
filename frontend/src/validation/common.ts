/**
 * Regras de validação genéricas, reutilizadas por mais de um formulário —
 * evita duplicar a mesma checagem em telas diferentes.
 */

export function isBlank(value: string): boolean {
  return value.trim().length === 0;
}

/** Usado sempre que um campo numérico precisa ser estritamente positivo (ex.: preço de item de pedido). */
export function isPositiveNumber(value: number): boolean {
  return Number.isFinite(value) && value > 0;
}
