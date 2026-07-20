using System.Text.Json.Serialization;

namespace Prova.Service.Cotacao;

/// <summary>
/// Modelo interno (não exposto fora deste namespace) para desserializar a
/// resposta do serviço PTAX do Banco Central. Endpoint real:
/// https://olinda.bcb.gov.br/olinda/servico/PTAX/versao/v1/odata/CotacaoMoedaDia
/// — responde um envelope OData com uma única propriedade <c>value</c>,
/// contendo um array com todas as cotações divulgadas ao longo do dia
/// (Abertura, um ou mais Intermediário, Fechamento PTAX).
/// </summary>
internal class BcbCotacaoResposta
{
    [JsonPropertyName("value")]
    public List<BcbCotacaoItem> Value { get; set; } = new();
}

/// <summary>
/// Uma entrada do array <c>value</c> da resposta do BCB — corresponde a um
/// boletim de cotação divulgado em um horário específico do dia.
/// </summary>
internal class BcbCotacaoItem
{
    /// <summary>Cotação de compra da moeda em Real, a que este serviço usa.</summary>
    [JsonPropertyName("cotacaoCompra")]
    public decimal CotacaoCompra { get; set; }

    [JsonPropertyName("cotacaoVenda")]
    public decimal CotacaoVenda { get; set; }

    /// <summary>
    /// Mantido como <see cref="string"/> (em vez de <see cref="DateTime"/>)
    /// de propósito: o formato retornado pelo BCB
    /// (<c>"yyyy-MM-dd HH:mm:ss.fff"</c>) é lexicograficamente ordenável —
    /// comparar as strings diretamente (ordinal) já dá a ordem cronológica
    /// correta, sem depender do parser de data do System.Text.Json aceitar
    /// um formato com espaço em vez de "T" como separador.
    /// </summary>
    [JsonPropertyName("dataHoraCotacao")]
    public string DataHoraCotacao { get; set; } = string.Empty;

    /// <summary>
    /// Identifica o momento do boletim no dia: "Abertura", "Intermediário" ou
    /// "Fechamento PTAX" — este último é o valor oficial do dia, usado como
    /// cotação de referência por este serviço.
    /// </summary>
    [JsonPropertyName("tipoBoletim")]
    public string TipoBoletim { get; set; } = string.Empty;
}
