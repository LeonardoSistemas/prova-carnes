namespace Prova.Service.Dtos;

/// <summary>
/// Métricas de topo do Dashboard (T59) para o período solicitado. Valores
/// monetários usam Real — rankings/métricas usam valor em Real, não
/// quantidade.
/// </summary>
/// <param name="TotalPedidos">Quantidade de pedidos com <c>Pedido.Data</c> dentro do período.</param>
/// <param name="FaturamentoTotal">Soma de <c>Pedido.ValorTotalEmReal</c> dos pedidos do período.</param>
/// <param name="TicketMedio">
/// <c>FaturamentoTotal / TotalPedidos</c>. Quando não há pedidos no período,
/// vale 0 (nunca lança exceção nem retorna NaN).
/// </param>
/// <param name="CompradoresAtivos">
/// Quantidade de compradores distintos que fizeram ao menos 1 pedido dentro do período.
/// </param>
/// <param name="CompradoresCadastrados">
/// Quantidade total de compradores cadastrados, sem filtro de período.
/// </param>
public record DashboardResumoDto(
    int TotalPedidos,
    decimal FaturamentoTotal,
    decimal TicketMedio,
    int CompradoresAtivos,
    int CompradoresCadastrados);
