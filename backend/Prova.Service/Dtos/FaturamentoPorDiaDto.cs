namespace Prova.Service.Dtos;

/// <summary>
/// Um ponto da série de faturamento por dia (T61), usado para alimentar um
/// gráfico de linha no Dashboard. <see cref="Data"/> é sempre uma data "pura"
/// (<c>DateTime.Date</c>, sem componente de hora).
/// </summary>
/// <param name="Data">Dia (00:00) a que o faturamento se refere.</param>
/// <param name="Faturamento">
/// Soma de <c>Pedido.ValorTotalEmReal</c> dos pedidos feitos naquele dia.
/// Vale 0 quando não há nenhum pedido no dia — o dia continua presente na
/// lista (não fica ausente), para que o gráfico de linha não tenha buraco.
/// </param>
public record FaturamentoPorDiaDto(DateTime Data, decimal Faturamento);
