namespace Prova.Service.Dtos;

/// <summary>
/// Uma posição do ranking "Top 5 compradores por valor" do Dashboard (T60).
/// <see cref="ValorTotal"/> é a soma de <c>Pedido.ValorTotalEmReal</c> dos
/// pedidos daquele Comprador dentro do período solicitado.
/// </summary>
/// <param name="CompradorId">Id do Comprador.</param>
/// <param name="Nome">Nome do Comprador, usado como critério de desempate (ver <see cref="ValorTotal"/>).</param>
/// <param name="ValorTotal">Soma de <c>Pedido.ValorTotalEmReal</c> no período.</param>
public record TopCompradorDto(int CompradorId, string Nome, decimal ValorTotal);
