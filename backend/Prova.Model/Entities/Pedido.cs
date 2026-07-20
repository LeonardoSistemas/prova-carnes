namespace Prova.Model.Entities;

/// <summary>
/// Pedido de compra de carnes feito por um Comprador, composto por um ou
/// mais PedidoItem. Não existe conceito de cancelamento separado de
/// exclusão.
/// </summary>
public class Pedido : EntidadeBase
{
    public DateTime Data { get; set; }

    public int CompradorId { get; set; }

    public Comprador? Comprador { get; set; }

    public ICollection<PedidoItem> Itens { get; set; } = new List<PedidoItem>();

    /// <summary>
    /// Valor total do pedido em Real, calculado a partir da soma dos itens
    /// (cada um já convertido com a cotação congelada no momento do
    /// pedido). Não é recalculado com base em cotação atual — reflete o
    /// valor histórico do pedido.
    /// </summary>
    public decimal ValorTotalEmReal => Itens.Sum(item => item.ValorEmReal);
}
