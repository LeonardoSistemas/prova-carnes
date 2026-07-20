using System.Net;
using System.Text.Json;
using FluentValidation;
using Prova.Service.Exceptions;

namespace Prova.Api.Middlewares;

/// <summary>
/// Middleware global de tratamento de exceções — único lugar da aplicação
/// que faz try/catch — tratamento de exceção centralizado aqui, nunca
/// disperso pelo resto do código.
/// Controllers e Services nunca capturam exceção de domínio; deixam subir
/// até aqui.
///
/// Mapeamento por TIPO de exceção (regra única, sem exceção por contexto):
///   - <see cref="ValidationException"/> (FluentValidation)  → 400 Bad Request
///   - <see cref="ArgumentOutOfRangeException"/>             → 400 Bad Request
///     (usada pela Service para parâmetro de query fora do intervalo
///     aceitável, ex: <c>dias</c> do Dashboard — ver
///     <c>DashboardController.ObterFaturamentoPorDia</c>/T62; input de
///     query string inválido é erro do chamador, mesma categoria de
///     <see cref="ValidationException"/>, por isso mesmo status)
///   - <see cref="EntidadeNaoEncontradaException"/>          → 404 Not Found
///   - <see cref="EntidadeVinculadaException"/>              → 409 Conflict
///   - <see cref="CotacaoIndisponivelException"/>            → 422 Unprocessable Entity
///   - qualquer outra exceção                                → 500 Internal Server Error,
///     com mensagem genérica — nunca stack trace/mensagem crua da exception,
///     em nenhum ambiente (nem em Development). O detalhe completo (incluindo
///     stack trace) só é registrado no <see cref="ILogger"/> do servidor.
///
/// Corpo de resposta padronizado em <see cref="ErroRespostaDto"/> para os 4
/// controllers.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            var mensagens = ex.Errors.Select(e => e.ErrorMessage).ToList();
            await EscreverRespostaAsync(context, HttpStatusCode.BadRequest, mensagens, ex);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            // Nunca usa ex.Message diretamente aqui: o .NET concatena
            // automaticamente um sufixo técnico cru na mensagem
            // (ex.: "(Parameter 'dias')\r\nActual value was 0."),
            // violando a regra de nunca expor mensagem de exception crua.
            // ex.ParamName é controlado pelo código que lança a exceção,
            // nunca contém o valor recebido — seguro de expor.
            var mensagem = ex.ParamName is null
                ? "Parâmetro de requisição inválido."
                : $"O valor informado para '{ex.ParamName}' é inválido.";
            await EscreverRespostaAsync(context, HttpStatusCode.BadRequest, new[] { mensagem }, ex);
        }
        catch (EntidadeNaoEncontradaException ex)
        {
            await EscreverRespostaAsync(context, HttpStatusCode.NotFound, new[] { ex.Message }, ex);
        }
        catch (EntidadeVinculadaException ex)
        {
            await EscreverRespostaAsync(context, HttpStatusCode.Conflict, new[] { ex.Message }, ex);
        }
        catch (CotacaoIndisponivelException ex)
        {
            await EscreverRespostaAsync(context, HttpStatusCode.UnprocessableEntity, new[] { ex.Message }, ex);
        }
        catch (Exception ex)
        {
            // Nunca expõe stack trace/mensagem crua ao cliente — o detalhe
            // fica só no log do servidor.
            await EscreverRespostaAsync(
                context,
                HttpStatusCode.InternalServerError,
                new[] { "Ocorreu um erro ao processar a requisição." },
                ex);
        }
    }

    private async Task EscreverRespostaAsync(
        HttpContext context, HttpStatusCode statusCode, IReadOnlyList<string> mensagens, Exception ex)
    {
        _logger.LogError(ex, "Erro ao processar {Method} {Path}: {Mensagem}",
            context.Request.Method, context.Request.Path, ex.Message);

        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var corpo = new ErroRespostaDto(mensagens);
        await context.Response.WriteAsync(JsonSerializer.Serialize(corpo, JsonOptions));
    }
}
