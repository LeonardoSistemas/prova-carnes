using Microsoft.AspNetCore.Mvc;
using Prova.Api.Middlewares;
using Prova.Model.Enums;
using Prova.Service.Dtos;
using Prova.Service.Services;

namespace Prova.Api.Controllers;

/// <summary>
/// Endpoints do Dashboard (T62). Nenhuma regra de negócio aqui — apenas
/// tradução HTTP↔Service, incluindo a composição de <see cref="DashboardDto"/>
/// a partir dos dois métodos de <see cref="IDashboardService"/> (T59+T60), o
/// que não é regra de negócio, só agregação de resposta para poupar o
/// frontend de duas chamadas.
///
/// Validação de <c>periodo</c>: recebido como <see cref="string"/> (não como
/// <see cref="PeriodoDashboard"/> direto no parâmetro da action) porque o
/// model binding do ASP.NET Core, ao falhar o bind de um enum a partir de
/// query string, produz um 400 com o formato de erro padrão do framework
/// (<c>ValidationProblemDetails</c>) — diferente do <see cref="ErroRespostaDto"/>
/// usado por todo o resto da API. Fazer o parse manualmente aqui (com
/// <see cref="Enum.TryParse{TEnum}(string?, bool, out TEnum)"/>, case-insensitive)
/// garante um 400 com o mesmo contrato de erro dos demais endpoints.
///
/// Validação de <c>dias</c>: delegada à Service (que já lança
/// <see cref="ArgumentOutOfRangeException"/> para valores &lt;= 0, ver T61) e
/// mapeada para 400 pelo middleware global de exceção — nenhum try/catch
/// neste Controller (ver <see cref="ExceptionHandlingMiddleware"/>).
/// </summary>
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Resumo + top carnes/compradores do período informado. Valores aceitos
    /// para <paramref name="periodo"/> (case-insensitive): <c>hoje</c>,
    /// <c>semana</c>, <c>mes</c>.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> ObterDashboard([FromQuery] string? periodo)
    {
        if (!Enum.TryParse<PeriodoDashboard>(periodo, ignoreCase: true, out var periodoConvertido))
        {
            return BadRequest(new ErroRespostaDto(new[]
            {
                "Período inválido. Valores aceitos: hoje, semana, mes.",
            }));
        }

        var resumo = await _dashboardService.ObterResumoAsync(periodoConvertido);
        var top = await _dashboardService.ObterTopCarnesECompradoresAsync(periodoConvertido);

        return Ok(new DashboardDto(resumo, top));
    }

    /// <summary>
    /// Série de faturamento diário dos últimos <paramref name="dias"/> dias
    /// corridos (padrão: 7). <paramref name="dias"/> &lt;= 0 vira 400 (ver
    /// mapeamento de <see cref="ArgumentOutOfRangeException"/> no
    /// <see cref="ExceptionHandlingMiddleware"/>).
    /// </summary>
    [HttpGet("faturamento-por-dia")]
    public async Task<ActionResult<IReadOnlyList<FaturamentoPorDiaDto>>> ObterFaturamentoPorDia([FromQuery] int dias = 7)
    {
        var serie = await _dashboardService.ObterFaturamentoPorDiaAsync(dias);

        return Ok(serie);
    }
}
