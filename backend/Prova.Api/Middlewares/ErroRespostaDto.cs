namespace Prova.Api.Middlewares;

/// <summary>
/// Formato único de corpo de erro devolvido pelo middleware global de
/// exceção (<see cref="ExceptionHandlingMiddleware"/>) para qualquer status
/// 400/404/409/422/500, em qualquer um dos 4 controllers — consistência de
/// contrato de erro é o que permite o frontend tratar erro de forma genérica
/// (ver revisão de integração, T30).
/// </summary>
/// <param name="Erros">
/// Uma ou mais mensagens de erro voltadas para exibição ao usuário/cliente.
/// Nunca contém stack trace ou mensagem crua de exceção não mapeada.
/// </param>
public record ErroRespostaDto(IReadOnlyList<string> Erros);
