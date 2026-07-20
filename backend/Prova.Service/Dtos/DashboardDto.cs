namespace Prova.Service.Dtos;

/// <summary>
/// Combina <see cref="DashboardResumoDto"/> (T59) e <see cref="DashboardTopDto"/>
/// (T60) numa única resposta para o endpoint <c>GET /api/dashboard</c> (T62),
/// evitando que o frontend precise de duas chamadas HTTP para montar uma
/// única tela. Não duplica nenhuma regra de cálculo — é só uma composição dos
/// dois DTOs já produzidos por <c>IDashboardService.ObterResumoAsync</c> e
/// <c>IDashboardService.ObterTopCarnesECompradoresAsync</c>, montada no
/// <c>DashboardController</c> (a interface do Service não muda: T59/T60
/// continuam com métodos separados, reaproveitáveis independentemente).
/// </summary>
/// <param name="Resumo">Métricas de topo do período (total de pedidos, faturamento, ticket médio, compradores).</param>
/// <param name="Top">Top 5 carnes e Top 5 compradores por valor em Real, no mesmo período.</param>
public record DashboardDto(DashboardResumoDto Resumo, DashboardTopDto Top);
