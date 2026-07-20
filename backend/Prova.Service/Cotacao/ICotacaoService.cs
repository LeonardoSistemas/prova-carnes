using Prova.Model.Enums;

namespace Prova.Service.Cotacao;

/// <summary>
/// Abstrai a obtenção de cotação de moeda estrangeira para Real, usada pela
/// Service de Pedido (T13/T14) para congelar <c>PedidoItem.CotacaoUsada</c>
/// no momento da criação/edição.
///
/// Decisão: BRL é responsabilidade deste serviço, não do consumidor — ele
/// retorna 1 diretamente (sem chamada HTTP) quando a moeda pedida é BRL.
/// Isso evita espalhar "if (moeda == Moeda.BRL)" pela Service de Pedido toda
/// vez que uma cotação é necessária; o consumidor sempre pode perguntar "qual
/// a cotação desta moeda para Real" e recebe uma resposta uniforme,
/// independentemente de ela exigir chamada externa ou não.
/// </summary>
public interface ICotacaoService
{
    /// <summary>
    /// Obtém a cotação para Real de cada moeda informada. Moedas duplicadas
    /// são resolvidas uma única vez; a estratégia de chamada HTTP (uma
    /// requisição em lote ou uma por moeda) é responsabilidade de cada
    /// implementação concreta, não faz parte deste contrato.
    /// </summary>
    /// <param name="moedas">Moedas necessárias (duplicatas são ignoradas).</param>
    /// <returns>Dicionário moeda → cotação para Real (BRL sempre 1).</returns>
    /// <exception cref="Exceptions.CotacaoIndisponivelException">
    /// Quando a fonte de cotação está indisponível, expira o tempo limite,
    /// não tem cotação para a data ou devolve uma resposta em formato
    /// inesperado.
    /// </exception>
    Task<IReadOnlyDictionary<Moeda, decimal>> ObterCotacoesAsync(IEnumerable<Moeda> moedas);
}
