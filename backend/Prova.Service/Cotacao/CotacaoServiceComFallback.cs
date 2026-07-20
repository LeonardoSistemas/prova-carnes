using Prova.Model.Enums;
using Prova.Service.Exceptions;

namespace Prova.Service.Cotacao;

/// <summary>
/// Decorator de <see cref="ICotacaoService"/> que tenta uma fonte de cotação
/// primária e, somente se ela falhar com <see cref="CotacaoIndisponivelException"/>,
/// cai para uma fonte de fallback.
///
/// Esta é a ÚNICA classe do sistema que sabe que existe mais de uma
/// implementação de <see cref="ICotacaoService"/> (as duas implementações
/// concretas hoje são <see cref="BcbCotacaoService"/> e
/// <see cref="AwesomeApiCotacaoService"/> — qual delas é primária e qual é
/// fallback é decidido só no registro de DI em <c>Program.cs</c>, não aqui;
/// já foi invertido mais de uma vez, então não hardcode a ordem atual neste
/// comentário, veja `Program.cs` para saber o estado vigente).
/// Nenhuma outra classe — em especial a Service de Pedido — deve referenciar
/// essas duas implementações concretas diretamente; todo o resto do sistema
/// enxerga apenas <see cref="ICotacaoService"/>.
///
/// Decisão de design (construtor): em vez de criar tipos "marcadores" (ex.:
/// <c>ICotacaoServicePrimaria</c>/<c>ICotacaoServiceFallback</c>) — abstração
/// desnecessária para um caso de só duas implementações — o construtor recebe
/// duas dependências <see cref="ICotacaoService"/> distintas via parâmetros
/// posicionais nomeados de forma explícita (<c>cotacaoPrimaria</c> e
/// <c>cotacaoFallback</c>). O ponto de registro em <c>Program.cs</c> resolve
/// <see cref="BcbCotacaoService"/> e <see cref="AwesomeApiCotacaoService"/>
/// pelos próprios tipos concretos (não pela interface) e monta este
/// decorator manualmente — único lugar, além desta classe, com conhecimento
/// de que o fallback existe.
/// </summary>
public class CotacaoServiceComFallback : ICotacaoService
{
    private readonly ICotacaoService _cotacaoPrimaria;
    private readonly ICotacaoService _cotacaoFallback;

    /// <param name="cotacaoPrimaria">
    /// Fonte de cotação consultada primeiro. Se ela responder com sucesso, o
    /// fallback nunca é chamado.
    /// </param>
    /// <param name="cotacaoFallback">
    /// Fonte de cotação consultada somente quando <paramref name="cotacaoPrimaria"/>
    /// lançar <see cref="CotacaoIndisponivelException"/>.
    /// </param>
    public CotacaoServiceComFallback(ICotacaoService cotacaoPrimaria, ICotacaoService cotacaoFallback)
    {
        _cotacaoPrimaria = cotacaoPrimaria;
        _cotacaoFallback = cotacaoFallback;
    }

    public async Task<IReadOnlyDictionary<Moeda, decimal>> ObterCotacoesAsync(IEnumerable<Moeda> moedas)
    {
        // Materializa a coleção para poder reutilizá-la em uma segunda
        // chamada (ao fallback) sem depender de o IEnumerable de origem
        // suportar múltipla enumeração.
        var moedasSolicitadas = moedas as IReadOnlyCollection<Moeda> ?? moedas.ToList();

        try
        {
            return await _cotacaoPrimaria.ObterCotacoesAsync(moedasSolicitadas);
        }
        catch (CotacaoIndisponivelException excecaoPrimaria)
        {
            try
            {
                return await _cotacaoFallback.ObterCotacoesAsync(moedasSolicitadas);
            }
            catch (CotacaoIndisponivelException excecaoFallback)
            {
                // Nenhuma das duas causas raiz pode se perder: ambas ficam
                // disponíveis para log via AggregateException.InnerExceptions,
                // e a mensagem exposta já deixa claro que as duas fontes
                // falharam (evita mensagem genérica que mascare a causa).
                throw new CotacaoIndisponivelException(
                    "Não foi possível obter a cotação de moeda: a fonte primária e a fonte de " +
                    $"fallback falharam. Fonte primária: {excecaoPrimaria.Message} " +
                    $"Fonte de fallback: {excecaoFallback.Message}",
                    new AggregateException(excecaoPrimaria, excecaoFallback));
            }
        }
    }
}
