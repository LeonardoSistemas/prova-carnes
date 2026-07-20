namespace Prova.Service.Dtos;

/// <summary>
/// Uma posição do ranking "Top 5 carnes por valor" do Dashboard (T60).
/// <see cref="ValorTotal"/> é a soma de <c>PedidoItem.ValorEmReal</c> (já em
/// Real, cotação congelada no momento do pedido) de todos os itens daquela
/// Carne dentro do período solicitado.
/// </summary>
/// <param name="CarneId">Id da Carne.</param>
/// <param name="Descricao">Descrição da Carne, usada como critério de desempate (ver <see cref="ValorTotal"/>).</param>
/// <param name="ValorTotal">Soma de <c>PedidoItem.ValorEmReal</c> no período.</param>
public record TopCarneDto(int CarneId, string Descricao, decimal ValorTotal);
