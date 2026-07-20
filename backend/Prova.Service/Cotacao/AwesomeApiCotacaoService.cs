using System.Net.Http.Json;
using System.Text.Json;
using Prova.Model.Enums;
using Prova.Service.Exceptions;

namespace Prova.Service.Cotacao;

/// <summary>
/// Implementação de <see cref="ICotacaoService"/> consultando a AwesomeAPI
/// (https://economia.awesomeapi.com.br). Recebe <see cref="HttpClient"/> via
/// injeção (padrão "typed client" do <c>IHttpClientFactory</c>); nunca
/// instancia <c>new HttpClient()</c> diretamente. Desde a troca de fonte
/// primária para o BCB, esta classe é registrada em <c>Program.cs</c> pelo
/// próprio tipo concreto (<c>services.AddHttpClient&lt;AwesomeApiCotacaoService&gt;()</c>,
/// não mais como <see cref="ICotacaoService"/> direto) e usada como
/// fallback dentro de <see cref="CotacaoServiceComFallback"/>. O parâmetro
/// opcional <c>timeout</c> existe só para permitir que os testes unitários
/// simulem timeout rapidamente (poucos milissegundos) em vez de esperar os
/// 5s padrão de produção.
/// </summary>
public class AwesomeApiCotacaoService : ICotacaoService
{
    private const string BaseUrl = "https://economia.awesomeapi.com.br/last/";
    private static readonly TimeSpan TimeoutPadrao = TimeSpan.FromSeconds(5);

    private static readonly IReadOnlyDictionary<Moeda, string> CodigosPorMoeda = new Dictionary<Moeda, string>
    {
        [Moeda.USD] = "USD",
        [Moeda.EUR] = "EUR",
    };

    private readonly HttpClient _httpClient;
    private readonly TimeSpan _timeout;

    public AwesomeApiCotacaoService(HttpClient httpClient, TimeSpan? timeout = null)
    {
        _httpClient = httpClient;
        _timeout = timeout ?? TimeoutPadrao;
    }

    public async Task<IReadOnlyDictionary<Moeda, decimal>> ObterCotacoesAsync(IEnumerable<Moeda> moedas)
    {
        var moedasDistintas = moedas.Distinct().ToList();
        var resultado = new Dictionary<Moeda, decimal>();

        var moedasEstrangeiras = moedasDistintas.Where(m => m != Moeda.BRL).ToList();

        if (moedasDistintas.Contains(Moeda.BRL))
        {
            resultado[Moeda.BRL] = 1m;
        }

        if (moedasEstrangeiras.Count == 0)
        {
            return resultado;
        }

        var pares = string.Join(",", moedasEstrangeiras.Select(m => $"{CodigosPorMoeda[m]}-BRL"));

        using var cts = new CancellationTokenSource(_timeout);

        Dictionary<string, AwesomeApiCotacaoItem>? resposta;
        try
        {
            resposta = await _httpClient.GetFromJsonAsync<Dictionary<string, AwesomeApiCotacaoItem>>(
                $"{BaseUrl}{pares}", cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            throw new CotacaoIndisponivelException(
                "Tempo limite excedido ao consultar a cotação da moeda. Tente novamente.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new CotacaoIndisponivelException(
                "Não foi possível obter a cotação da moeda no momento.", ex);
        }
        catch (JsonException ex)
        {
            throw new CotacaoIndisponivelException(
                "A cotação da moeda foi recebida em um formato inesperado.", ex);
        }

        if (resposta is null)
        {
            throw new CotacaoIndisponivelException(
                "A cotação da moeda não foi retornada pelo serviço externo.");
        }

        foreach (var moeda in moedasEstrangeiras)
        {
            var chave = $"{CodigosPorMoeda[moeda]}BRL";

            if (!resposta.TryGetValue(chave, out var item) ||
                !decimal.TryParse(item.Bid, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var cotacao))
            {
                throw new CotacaoIndisponivelException(
                    $"A cotação de {CodigosPorMoeda[moeda]} não veio em um formato válido na resposta.");
            }

            resultado[moeda] = cotacao;
        }

        return resultado;
    }
}
