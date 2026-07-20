using System.Text.Json.Serialization;

namespace Prova.Service.Cotacao;

/// <summary>
/// Modelo interno (não exposto fora deste namespace) para desserializar a
/// resposta da AwesomeAPI. Endpoint real:
/// https://economia.awesomeapi.com.br/last/USD-BRL,EUR-BRL — responde um
/// objeto cujas chaves são dinâmicas (ex.: "USDBRL", "EURBRL"), por isso a
/// resposta é desserializada como <c>Dictionary&lt;string, AwesomeApiCotacaoItem&gt;</c>
/// em vez de um tipo fixo.
/// </summary>
internal class AwesomeApiCotacaoItem
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("codein")]
    public string CodeIn { get; set; } = string.Empty;

    /// <summary>Valor da cotação, vem como string na resposta da AwesomeAPI.</summary>
    [JsonPropertyName("bid")]
    public string Bid { get; set; } = string.Empty;
}
