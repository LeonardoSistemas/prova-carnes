using System.Net;
using Prova.Model.Enums;
using Prova.Service.Cotacao;
using Prova.Service.Exceptions;
using Prova.Service.Tests.Fakes;
using Xunit;

namespace Prova.Service.Tests.Cotacao;

public class AwesomeApiCotacaoServiceTests
{
    [Fact]
    public async Task ObterCotacoesAsync_QuandoApiRespondeComSucesso_RetornaCotacoesConvertidas()
    {
        var json = """
        {
            "USDBRL": { "code": "USD", "codein": "BRL", "bid": "5.3210" },
            "EURBRL": { "code": "EUR", "codein": "BRL", "bid": "5.7650" }
        }
        """;

        var handler = FakeHttpMessageHandler.ComResposta(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        });

        var httpClient = new HttpClient(handler);
        var service = new AwesomeApiCotacaoService(httpClient);

        var cotacoes = await service.ObterCotacoesAsync([Moeda.USD, Moeda.EUR, Moeda.BRL]);

        Assert.Equal(5.3210m, cotacoes[Moeda.USD]);
        Assert.Equal(5.7650m, cotacoes[Moeda.EUR]);
        Assert.Equal(1m, cotacoes[Moeda.BRL]);
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoMoedaEhSomenteBrl_NaoChamaHttp()
    {
        var handler = FakeHttpMessageHandler.ComResposta(_ =>
            throw new InvalidOperationException("Não deveria chamar HTTP para BRL."));

        var httpClient = new HttpClient(handler);
        var service = new AwesomeApiCotacaoService(httpClient);

        var cotacoes = await service.ObterCotacoesAsync([Moeda.BRL]);

        Assert.Equal(1m, cotacoes[Moeda.BRL]);
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoApiRetornaErroHttp_LancaCotacaoIndisponivelException()
    {
        var handler = FakeHttpMessageHandler.ComResposta(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var httpClient = new HttpClient(handler);
        var service = new AwesomeApiCotacaoService(httpClient);

        await Assert.ThrowsAsync<CotacaoIndisponivelException>(
            () => service.ObterCotacoesAsync([Moeda.USD]));
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoApiExpiraTempoLimite_LancaCotacaoIndisponivelException()
    {
        var handler = FakeHttpMessageHandler.ComTimeout();
        var httpClient = new HttpClient(handler);
        var service = new AwesomeApiCotacaoService(httpClient, timeout: TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAsync<CotacaoIndisponivelException>(
            () => service.ObterCotacoesAsync([Moeda.USD]));
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoRespostaVemEmFormatoInesperado_LancaCotacaoIndisponivelException()
    {
        var handler = FakeHttpMessageHandler.ComResposta(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("não é um json válido", System.Text.Encoding.UTF8, "application/json"),
        });

        var httpClient = new HttpClient(handler);
        var service = new AwesomeApiCotacaoService(httpClient);

        await Assert.ThrowsAsync<CotacaoIndisponivelException>(
            () => service.ObterCotacoesAsync([Moeda.USD]));
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoApiRetornaCorpoJsonNulo_LancaCotacaoIndisponivelException()
    {
        var handler = FakeHttpMessageHandler.ComResposta(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json"),
        });

        var httpClient = new HttpClient(handler);
        var service = new AwesomeApiCotacaoService(httpClient);

        await Assert.ThrowsAsync<CotacaoIndisponivelException>(
            () => service.ObterCotacoesAsync([Moeda.USD]));
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoRespostaNaoContemChaveDaMoedaSolicitada_LancaCotacaoIndisponivelException()
    {
        var json = """
        {
            "EURBRL": { "code": "EUR", "codein": "BRL", "bid": "5.7650" }
        }
        """;

        var handler = FakeHttpMessageHandler.ComResposta(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        });

        var httpClient = new HttpClient(handler);
        var service = new AwesomeApiCotacaoService(httpClient);

        await Assert.ThrowsAsync<CotacaoIndisponivelException>(
            () => service.ObterCotacoesAsync([Moeda.USD]));
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoBidNaoEhNumerico_LancaCotacaoIndisponivelException()
    {
        var json = """
        {
            "USDBRL": { "code": "USD", "codein": "BRL", "bid": "indisponível" }
        }
        """;

        var handler = FakeHttpMessageHandler.ComResposta(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        });

        var httpClient = new HttpClient(handler);
        var service = new AwesomeApiCotacaoService(httpClient);

        await Assert.ThrowsAsync<CotacaoIndisponivelException>(
            () => service.ObterCotacoesAsync([Moeda.USD]));
    }
}
