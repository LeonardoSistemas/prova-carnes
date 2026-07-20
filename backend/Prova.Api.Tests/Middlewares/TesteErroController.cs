using Microsoft.AspNetCore.Mvc;

namespace Prova.Api.Tests.Middlewares;

/// <summary>
/// Controller exclusivo dos testes de <see cref="ExceptionHandlingMiddlewareTests"/>
/// (T37) — não existe no projeto Prova.Api, é registrado no host de teste via
/// ApplicationPart adicional só quando necessário. Único propósito: lançar uma
/// exceção arbitrária, não mapeada por nenhum catch específico do
/// <c>ExceptionHandlingMiddleware</c> (não é <c>ValidationException</c>,
/// <c>EntidadeNaoEncontradaException</c>, <c>EntidadeVinculadaException</c> nem
/// <c>CotacaoIndisponivelException</c>), para forçar o branch
/// <c>catch (Exception ex)</c> genérico e comprovar que o middleware nunca
/// devolve a mensagem crua/stack trace ao cliente.
/// </summary>
[ApiController]
[Route("api/teste-erro")]
public class TesteErroController : ControllerBase
{
    public const string MensagemInternaSensivel =
        "mensagem interna sensível de teste — jamais deve chegar ao cliente";

    [HttpGet]
    public ActionResult LancarExcecaoNaoMapeada()
    {
        throw new InvalidOperationException(MensagemInternaSensivel);
    }
}
