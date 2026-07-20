using Prova.Model.Enums;

namespace Prova.Model.Entities;

/// <summary>
/// Entidade associativa entre Pedido e Carne. Cada item tem Preço e Moeda
/// próprios (preço "spot", negociado por pedido — não segue tabela de preço
/// fixa). A mesma Carne pode aparecer mais de uma vez no mesmo Pedido como
/// itens distintos (ver PRD).
/// </summary>
public class PedidoItem : EntidadeBase
{
    public int PedidoId { get; set; }

    public Pedido? Pedido { get; set; }

    public int CarneId { get; set; }

    public Carne? Carne { get; set; }

    /// <summary>
    /// Preço "spot" do item, digitado livremente no pedido. Já representa o
    /// valor total daquele item/lote — não há campo de quantidade (ver PRD).
    /// </summary>
    public decimal Preco { get; set; }

    public Moeda Moeda { get; set; }

    /// <summary>
    /// Cotação de conversão para Real capturada no momento do POST (ou do
    /// PUT, quando a edição altera item/preço/moeda) e persistida junto ao
    /// item — nunca recalculada na leitura, para não alterar
    /// retroativamente o valor histórico do pedido.
    /// Por convenção, itens em BRL têm CotacaoUsada = 1, o que permite
    /// calcular o valor em Real de forma uniforme (Preco * CotacaoUsada)
    /// sem precisar de tratamento condicional por moeda.
    /// </summary>
    public decimal CotacaoUsada { get; set; }

    /// <summary>
    /// Valor do item já convertido para Real, usando a cotação congelada no
    /// momento do pedido.
    /// </summary>
    public decimal ValorEmReal => Preco * CotacaoUsada;
}
