/**
 * Formata um CPF removendo tudo que não é dígito, limitando a 11 dígitos,
 * e inserindo pontos/traço nas posições corretas.
 *
 * Exemplos:
 * - "12345678900" → "123.456.789-00"
 * - "123.456.789-00" → "123.456.789-00" (já formatado)
 * - "123" → "123" (parcialmente digitado)
 * - "123abc456def" → "123.456" (ignora caracteres não-dígito)
 */
export function formatarCpf(valor: string): string {
  if (!valor) return '';

  // Remove tudo que não é dígito
  const apenasDigitos = valor.replace(/\D/g, '');

  // Limita a 11 dígitos
  const limitado = apenasDigitos.slice(0, 11);

  // Aplica a máscara conforme o tamanho
  if (limitado.length <= 3) {
    return limitado;
  }
  if (limitado.length <= 6) {
    return `${limitado.slice(0, 3)}.${limitado.slice(3)}`;
  }
  if (limitado.length <= 9) {
    return `${limitado.slice(0, 3)}.${limitado.slice(3, 6)}.${limitado.slice(6)}`;
  }
  return `${limitado.slice(0, 3)}.${limitado.slice(3, 6)}.${limitado.slice(6, 9)}-${limitado.slice(9)}`;
}

/**
 * Remove a máscara de CPF, retornando apenas os dígitos.
 *
 * Exemplos:
 * - "123.456.789-00" → "12345678900"
 * - "12345678900" → "12345678900"
 * - "123abc456" → "123456"
 */
export function removerMascaraCpf(valor: string): string {
  return valor.replace(/\D/g, '');
}
