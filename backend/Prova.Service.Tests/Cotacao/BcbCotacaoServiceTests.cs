using System.Net;
using Prova.Model.Enums;
using Prova.Service.Cotacao;
using Prova.Service.Exceptions;
using Prova.Service.Tests.Fakes;
using Xunit;

namespace Prova.Service.Tests.Cotacao;

public class BcbCotacaoServiceTests
{
    private const string JsonDiaDePregao = """
    {
        "value": [
            { "paridadeCompra": 1.0, "paridadeVenda": 1.0, "cotacaoCompra": 4.8531, "cotacaoVenda": 4.8537, "dataHoraCotacao": "2024-01-15 10:07:28.933", "tipoBoletim": "Abertura" },
            { "paridadeCompra": 1.0, "paridadeVenda": 1.0, "cotacaoCompra": 4.8690, "cotacaoVenda": 4.8696, "dataHoraCotacao": "2024-01-15 12:07:41.512", "tipoBoletim": "Intermediário" },
            { "paridadeCompra": 1.0, "paridadeVenda": 1.0, "cotacaoCompra": 4.8735, "cotacaoVenda": 4.8741, "dataHoraCotacao": "2024-01-15 13:38:08.191", "tipoBoletim": "Fechamento PTAX" }
        ]
    }
    """;

    [Fact]
    public async Task ObterCotacoesAsync_QuandoApiRespondeComSucesso_RetornaCotacaoDeFechamentoParaUsd()
    {
        var handler = FakeHttpMessageHandler.ComResposta(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonDiaDePregao, System.Text.Encoding.UTF8, "application/json"),
        });

        var httpClient = new HttpClient(handler);
        var service = new BcbCotacaoService(httpClient);

        var cotacoes = await service.ObterCotacoesAsync([Moeda.USD]);

        Assert.Equal(4.8735m, cotacoes[Moeda.USD]);
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoApiRespondeComSucesso_RetornaCotacaoDeFechamentoParaEur()
    {
        var handler = FakeHttpMessageHandler.ComResposta(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonDiaDePregao, System.Text.Encoding.UTF8, "application/json"),
        });

        var httpClient = new HttpClient(handler);
        var service = new BcbCotacaoService(httpClient);

        var cotacoes = await service.ObterCotacoesAsync([Moeda.EUR]);

        Assert.Equal(4.8735m, cotacoes[Moeda.EUR]);
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoFaltaFechamentoPtax_UsaEntradaMaisRecentePorDataHora()
    {
        var json = """
        {
            "value": [
                { "cotacaoCompra": 4.8531, "cotacaoVenda": 4.8537, "dataHoraCotacao": "2024-01-15 10:07:28.933", "tipoBoletim": "Abertura" },
                { "cotacaoCompra": 4.8690, "cotacaoVenda": 4.8696, "dataHoraCotacao": "2024-01-15 12:07:41.512", "tipoBoletim": "Intermediário" }
            ]
        }
        """;

        var handler = FakeHttpMessageHandler.ComResposta(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        });

        var httpClient = new HttpClient(handler);
        var service = new BcbCotacaoService(httpClient);

        var cotacoes = await service.ObterCotacoesAsync([Moeda.USD]);

        Assert.Equal(4.8690m, cotacoes[Moeda.USD]);
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoMoedaEhSomenteBrl_NaoChamaHttp()
    {
        var handler = FakeHttpMessageHandler.ComResposta(_ =>
            throw new InvalidOperationException("Não deveria chamar HTTP para BRL."));

        var httpClient = new HttpClient(handler);
        var service = new BcbCotacaoService(httpClient);

        var cotacoes = await service.ObterCotacoesAsync([Moeda.BRL]);

        Assert.Equal(1m, cotacoes[Moeda.BRL]);
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoHojeVemVazioEJanelaTemDiaUtilAnterior_UsaEntradaMaisRecenteDaJanela()
    {
        // Simula o cenário real de fim de semana: CotacaoMoedaDia de hoje não
        // retorna nenhuma entrada, mas CotacaoMoedaPeriodo (janela de 7 dias)
        // traz a sexta-feira anterior — o serviço deve resolver a cotação sem
        // precisar escalar para o fallback externo (AwesomeAPI).
        var jsonJanela = """
        {
            "value": [
                { "cotacaoCompra": 4.9000, "cotacaoVenda": 4.9006, "dataHoraCotacao": "2024-01-11 10:07:28.933", "tipoBoletim": "Abertura" },
                { "cotacaoCompra": 4.9123, "cotacaoVenda": 4.9129, "dataHoraCotacao": "2024-01-12 13:38:08.191", "tipoBoletim": "Fechamento" }
            ]
        }
        """;

        var handler = FakeHttpMessageHandler.ComResposta(request =>
        {
            var url = request.RequestUri!.ToString();
            var conteudo = url.Contains("CotacaoMoedaPeriodo", StringComparison.Ordinal)
                ? jsonJanela
                : """{ "value": [] }""";

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(conteudo, System.Text.Encoding.UTF8, "application/json"),
            };
        });

        var httpClient = new HttpClient(handler);
        var service = new BcbCotacaoService(httpClient);

        var cotacoes = await service.ObterCotacoesAsync([Moeda.USD]);

        Assert.Equal(4.9123m, cotacoes[Moeda.USD]);
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoJanelaInteiraVemVazia_LancaCotacaoIndisponivelException()
    {
        var handler = FakeHttpMessageHandler.ComResposta(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{ "value": [] }""", System.Text.Encoding.UTF8, "application/json"),
        });

        var httpClient = new HttpClient(handler);
        var service = new BcbCotacaoService(httpClient);

        var excecao = await Assert.ThrowsAsync<CotacaoIndisponivelException>(
            () => service.ObterCotacoesAsync([Moeda.USD]));

        Assert.Contains("últimos", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoApiRetornaErroHttp_LancaCotacaoIndisponivelException()
    {
        var handler = FakeHttpMessageHandler.ComResposta(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var httpClient = new HttpClient(handler);
        var service = new BcbCotacaoService(httpClient);

        await Assert.ThrowsAsync<CotacaoIndisponivelException>(
            () => service.ObterCotacoesAsync([Moeda.USD]));
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoApiExpiraTempoLimite_LancaCotacaoIndisponivelException()
    {
        var handler = FakeHttpMessageHandler.ComTimeout();
        var httpClient = new HttpClient(handler);
        var service = new BcbCotacaoService(httpClient, timeout: TimeSpan.FromMilliseconds(50));

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
        var service = new BcbCotacaoService(httpClient);

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
        var service = new BcbCotacaoService(httpClient);

        await Assert.ThrowsAsync<CotacaoIndisponivelException>(
            () => service.ObterCotacoesAsync([Moeda.USD]));
    }
}
