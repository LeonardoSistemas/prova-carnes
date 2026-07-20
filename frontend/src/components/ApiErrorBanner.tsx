interface ApiErrorBannerProps {
  /** Mensagens vindas de `ErroRespostaDto.erros` — nunca um texto genérico quando o servidor já forneceu conteúdo. */
  errors: string[] | null | undefined;
}

/**
 * Banner de erro inline usado para exibir o resultado de uma chamada HTTP
 * que falhou (409, 422, 500, etc.), sempre com as mensagens reais vindas da
 * API — nunca "Erro desconhecido" quando `erros` tem conteúdo.
 */
export function ApiErrorBanner({ errors }: ApiErrorBannerProps) {
  if (!errors || errors.length === 0) {
    return null;
  }

  return (
    <div className="api-error-banner" role="alert">
      {errors.map((mensagem) => (
        <p key={mensagem}>{mensagem}</p>
      ))}
    </div>
  );
}
