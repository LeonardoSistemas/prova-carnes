using Moq;
using Prova.Model.Enums;
using Prova.Service.Cotacao;
using Prova.Service.Exceptions;
using Xunit;

namespace Prova.Service.Tests.Cotacao;

public class CotacaoServiceComFallbackTests
{
    private readonly Mock<ICotacaoService> _primariaMock = new();
    private readonly Mock<ICotacaoService> _fallbackMock = new();
    private readonly CotacaoServiceComFallback _service;

    public CotacaoServiceComFallbackTests()
    {
        _service = new CotacaoServiceComFallback(_primariaMock.Object, _fallbackMock.Object);
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoPrimariaRespondeComSucesso_NaoChamaFallback()
    {
        var cotacaoEsperada = new Dictionary<Moeda, decimal> { [Moeda.USD] = 5.10m };
        _primariaMock
            .Setup(s => s.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()))
            .ReturnsAsync(cotacaoEsperada);

        var resultado = await _service.ObterCotacoesAsync([Moeda.USD]);

        Assert.Equal(5.10m, resultado[Moeda.USD]);
        _fallbackMock.Verify(s => s.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()), Times.Never);
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoPrimariaFalha_ChamaFallbackERetornaSeuResultado()
    {
        _primariaMock
            .Setup(s => s.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()))
            .ThrowsAsync(new CotacaoIndisponivelException("Banco Central indisponível."));

        var cotacaoDoFallback = new Dictionary<Moeda, decimal> { [Moeda.USD] = 5.25m };
        _fallbackMock
            .Setup(s => s.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()))
            .ReturnsAsync(cotacaoDoFallback);

        var resultado = await _service.ObterCotacoesAsync([Moeda.USD]);

        Assert.Equal(5.25m, resultado[Moeda.USD]);
        _fallbackMock.Verify(s => s.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()), Times.Once);
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoPrimariaEFallbackFalham_PropagaExcecaoComAmbasAsCausas()
    {
        _primariaMock
            .Setup(s => s.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()))
            .ThrowsAsync(new CotacaoIndisponivelException("Banco Central indisponível."));

        _fallbackMock
            .Setup(s => s.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()))
            .ThrowsAsync(new CotacaoIndisponivelException("AwesomeAPI indisponível."));

        var excecao = await Assert.ThrowsAsync<CotacaoIndisponivelException>(
            () => _service.ObterCotacoesAsync([Moeda.USD]));

        Assert.Contains("Banco Central indisponível.", excecao.Message);
        Assert.Contains("AwesomeAPI indisponível.", excecao.Message);

        var causaAgregada = Assert.IsType<AggregateException>(excecao.InnerException);
        Assert.Equal(2, causaAgregada.InnerExceptions.Count);
        Assert.Contains(causaAgregada.InnerExceptions, e => e.Message == "Banco Central indisponível.");
        Assert.Contains(causaAgregada.InnerExceptions, e => e.Message == "AwesomeAPI indisponível.");
    }

    [Fact]
    public async Task ObterCotacoesAsync_QuandoPrimariaLancaExcecaoDeOutroTipo_PropagaSemAcionarFallback()
    {
        _primariaMock
            .Setup(s => s.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()))
            .ThrowsAsync(new InvalidOperationException("Bug real, não é falha de fonte de cotação."));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ObterCotacoesAsync([Moeda.USD]));

        _fallbackMock.Verify(s => s.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()), Times.Never);
    }
}
