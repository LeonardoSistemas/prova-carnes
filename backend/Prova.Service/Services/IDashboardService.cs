using Prova.Model.Enums;
using Prova.Service.Dtos;

namespace Prova.Service.Services;

public interface IDashboardService
{
    /// <summary>
    /// Calcula as métricas de topo do Dashboard para o <paramref name="periodo"/>
    /// informado (ver <see cref="DashboardService"/> para a conversão de
    /// período em intervalo de datas).
    /// </summary>
    Task<DashboardResumoDto> ObterResumoAsync(PeriodoDashboard periodo);

    /// <summary>
    /// Calcula o Top 5 carnes e o Top 5 compradores por valor em Real, no
    /// mesmo <paramref name="periodo"/> informado (ver <see cref="DashboardService"/>
    /// para a conversão de período em intervalo de datas e o critério de
    /// desempate).
    /// </summary>
    Task<DashboardTopDto> ObterTopCarnesECompradoresAsync(PeriodoDashboard periodo);

    /// <summary>
    /// Série de faturamento diário dos últimos <paramref name="dias"/> dias
    /// corridos, terminando hoje (inclusive), em ordem cronológica (mais
    /// antigo primeiro). Dias sem nenhum pedido entram na lista com
    /// faturamento 0 — não ficam ausentes (ver <see cref="DashboardService"/>
    /// para detalhes e critério de validação de <paramref name="dias"/>).
    /// </summary>
    Task<IReadOnlyList<FaturamentoPorDiaDto>> ObterFaturamentoPorDiaAsync(int dias);
}
