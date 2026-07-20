using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Prova.Model.Enums;
using Prova.Service.Exceptions;

namespace Prova.Service.Cotacao;

/// <summary>
/// Implementação de <see cref="ICotacaoService"/> consultando o serviço PTAX
/// do Banco Central (<c>CotacaoMoedaDia</c>, em
/// https://olinda.bcb.gov.br/olinda/servico/PTAX) para a data de hoje.
/// Alternativa a <see cref="AwesomeApiCotacaoService"/> — ambas implementam o
/// mesmo contrato e podem ser trocadas via injeção de dependência (a escolha
/// de qual fica registrada em Program.cs é escopo de outra task). Recebe
/// <see cref="HttpClient"/> via injeção (padrão "typed client" do
/// <c>IHttpClientFactory</c>); nunca instancia <c>new HttpClient()</c>
/// diretamente. O parâmetro opcional <c>timeout</c> existe só para permitir
/// que os testes unitários simulem timeout rapidamente (poucos milissegundos)
/// em vez de esperar os 5s padrão de produção.
///
/// Trade-off aceito: diferente da AwesomeAPI, o endpoint
/// <c>CotacaoMoedaDia</c> do BCB não aceita múltiplas moedas em uma única
/// chamada — o parâmetro <c>@moeda</c> é singular. Por isso, quando as moedas
/// solicitadas incluem mais de uma moeda estrangeira (ex.: USD e EUR no mesmo
/// pedido), este serviço faz uma requisição HTTP por moeda distinta, em
/// sequência, em vez de uma única chamada em lote como a AwesomeAPI permite.
/// Para o conjunto de moedas suportado hoje (USD, EUR — no máximo duas
/// chamadas por pedido), o custo extra é aceitável; não é um bug, é uma
/// limitação da API externa que optamos por não contornar com complexidade
/// adicional (ex.: cache/paralelização) sem um caso de uso real que a exija.
/// </summary>
public class BcbCotacaoService : ICotacaoService
{
    private const string BaseUrl =
        "https://olinda.bcb.gov.br/olinda/servico/PTAX/versao/v1/odata/CotacaoMoedaDia";

    private const string BaseUrlPeriodo =
        "https://olinda.bcb.gov.br/olinda/servico/PTAX/versao/v1/odata/CotacaoMoedaPeriodo";

    private const string TipoBoletimFechamento = "Fechamento PTAX";

    /// <summary>
    /// Janela de dias corridos para trás usada na segunda tentativa (via
    /// <c>CotacaoMoedaPeriodo</c>) quando não há cotação para hoje — cobre o
    /// caso de fim de semana/feriado prolongado sem exagerar no intervalo
    /// consultado.
    /// </summary>
    private const int DiasDaJanelaDeFallback = 7;

    private static readonly TimeSpan TimeoutPadrao = TimeSpan.FromSeconds(5);

    private static readonly IReadOnlyDictionary<Moeda, string> CodigosPorMoeda = new Dictionary<Moeda, string>
    {
        [Moeda.USD] = "USD",
        [Moeda.EUR] = "EUR",
    };

    private readonly HttpClient _httpClient;
    private readonly TimeSpan _timeout;

    public BcbCotacaoService(HttpClient httpClient, TimeSpan? timeout = null)
    {
        _httpClient = httpClient;
        _timeout = timeout ?? TimeoutPadrao;
    }

    public async Task<IReadOnlyDictionary<Moeda, decimal>> ObterCotacoesAsync(IEnumerable<Moeda> moedas)
    {
        var moedasDistintas = moedas.Distinct().ToList();
        var resultado = new Dictionary<Moeda, decimal>();

        if (moedasDistintas.Contains(Moeda.BRL))
        {
            resultado[Moeda.BRL] = 1m;
        }

        var moedasEstrangeiras = moedasDistintas.Where(m => m != Moeda.BRL).ToList();

        foreach (var moeda in moedasEstrangeiras)
        {
            resultado[moeda] = await ObterCotacaoDoDiaAsync(moeda);
        }

        return resultado;
    }

    private async Task<decimal> ObterCotacaoDoDiaAsync(Moeda moeda)
    {
        var codigoMoeda = CodigosPorMoeda[moeda];
        var dataCotacao = DateTime.Today.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
        var url = $"{BaseUrl}(moeda=@moeda,dataCotacao=@dataCotacao)?@moeda='{codigoMoeda}'&@dataCotacao='{dataCotacao}'&$format=json";

        using var cts = new CancellationTokenSource(_timeout);

        BcbCotacaoResposta? resposta;
        try
        {
            resposta = await _httpClient.GetFromJsonAsync<BcbCotacaoResposta>(url, cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            throw new CotacaoIndisponivelException(
                $"Tempo limite excedido ao consultar a cotação de {codigoMoeda} no Banco Central. Tente novamente.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new CotacaoIndisponivelException(
                $"Não foi possível obter a cotação de {codigoMoeda} no Banco Central no momento.", ex);
        }
        catch (JsonException ex)
        {
            throw new CotacaoIndisponivelException(
                $"A cotação de {codigoMoeda} foi recebida em um formato inesperado do Banco Central.", ex);
        }

        if (resposta?.Value is { Count: > 0 } itensDoDia)
        {
            // "Fechamento PTAX" é o valor oficial do dia; na ausência dela (não
            // deveria acontecer num dia de pregão normal, mas o array pode vir
            // incompleto), usamos o boletim mais recente como aproximação
            // defensiva em vez de falhar.
            var itemDoDia = itensDoDia.FirstOrDefault(item => item.TipoBoletim == TipoBoletimFechamento)
                ?? itensDoDia.OrderByDescending(item => item.DataHoraCotacao, StringComparer.Ordinal).First();

            return itemDoDia.CotacaoCompra;
        }

        // Sem cotação para hoje (fim de semana, feriado, ou o dia ainda não
        // fechou) — antes de escalar para o fallback externo (AwesomeAPI, via
        // CotacaoServiceComFallback), tenta o último dia útil disponível numa
        // janela de dias corridos para trás via CotacaoMoedaPeriodo.
        return await ObterCotacaoDaJanelaAsync(codigoMoeda);
    }

    private async Task<decimal> ObterCotacaoDaJanelaAsync(string codigoMoeda)
    {
        var dataFinal = DateTime.Today;
        var dataInicial = dataFinal.AddDays(-DiasDaJanelaDeFallback);
        var dataInicialFormatada = dataInicial.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
        var dataFinalFormatada = dataFinal.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
        var url = $"{BaseUrlPeriodo}(moeda=@moeda,dataInicial=@dataInicial,dataFinalCotacao=@dataFinalCotacao)" +
                  $"?@moeda='{codigoMoeda}'&@dataInicial='{dataInicialFormatada}'&@dataFinalCotacao='{dataFinalFormatada}'&$format=json";

        using var cts = new CancellationTokenSource(_timeout);

        BcbCotacaoResposta? resposta;
        try
        {
            resposta = await _httpClient.GetFromJsonAsync<BcbCotacaoResposta>(url, cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            throw new CotacaoIndisponivelException(
                $"Tempo limite excedido ao consultar a cotação de {codigoMoeda} dos últimos {DiasDaJanelaDeFallback} dias no Banco Central. Tente novamente.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new CotacaoIndisponivelException(
                $"Não foi possível obter a cotação de {codigoMoeda} dos últimos {DiasDaJanelaDeFallback} dias no Banco Central no momento.", ex);
        }
        catch (JsonException ex)
        {
            throw new CotacaoIndisponivelException(
                $"A cotação de {codigoMoeda} dos últimos {DiasDaJanelaDeFallback} dias foi recebida em um formato inesperado do Banco Central.", ex);
        }

        if (resposta?.Value is null || resposta.Value.Count == 0)
        {
            throw new CotacaoIndisponivelException(
                $"Não há cotação de {codigoMoeda} disponível no Banco Central nem para hoje, nem para os últimos {DiasDaJanelaDeFallback} dias corridos.");
        }

        // Sem depender do texto de "tipoBoletim" (que vem sem o sufixo
        // "PTAX" neste endpoint de período — mesma cotação, rótulo
        // diferente): a entrada de dataHoraCotacao mais tardia é sempre a
        // última divulgada no dia mais recente presente no array, já que o
        // formato "yyyy-MM-dd HH:mm:ss.fff" é lexicograficamente ordenável.
        var itemMaisRecente = resposta.Value
            .OrderByDescending(item => item.DataHoraCotacao, StringComparer.Ordinal)
            .First();

        return itemMaisRecente.CotacaoCompra;
    }
}
