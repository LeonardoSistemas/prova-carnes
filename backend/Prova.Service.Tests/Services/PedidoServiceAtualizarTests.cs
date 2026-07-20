using System.Linq.Expressions;
using Moq;
using Prova.Data.Repositories;
using Prova.Model.Entities;
using Prova.Model.Enums;
using Prova.Service.Cotacao;
using Prova.Service.Dtos;
using Prova.Service.Exceptions;
using Prova.Service.Services;
using Prova.Service.Validators;
using Xunit;

namespace Prova.Service.Tests.Services;

public class PedidoServiceAtualizarTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRepository<Pedido>> _pedidoRepositoryMock = new();
    private readonly Mock<IRepository<Comprador>> _compradorRepositoryMock = new();
    private readonly Mock<IRepository<Carne>> _carneRepositoryMock = new();
    private readonly Mock<ICotacaoService> _cotacaoServiceMock = new();
    private readonly PedidoService _service;

    public PedidoServiceAtualizarTests()
    {
        _unitOfWorkMock.Setup(u => u.Repository<Pedido>()).Returns(_pedidoRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Comprador>()).Returns(_compradorRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Carne>()).Returns(_carneRepositoryMock.Object);
        _service = new PedidoService(_unitOfWorkMock.Object, new PedidoDtoValidator(), _cotacaoServiceMock.Object);

        _carneRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Carne { Id = 1, Descricao = "Picanha", Origem = OrigemCarne.Bovina });
    }

    [Fact]
    public async Task AtualizarAsync_QuandoAlteraApenasDataOuComprador_NaoChamaCotacaoServiceNovamente()
    {
        var pedidoExistente = new Pedido { Id = 1, Data = DateTime.Today.AddDays(-10), CompradorId = 1 };
        pedidoExistente.Itens.Add(new PedidoItem { Id = 1, PedidoId = 1, CarneId = 1, Preco = 100m, Moeda = Moeda.BRL, CotacaoUsada = 1m });

        _pedidoRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<Expression<Func<Pedido, object>>[]>()))
            .ReturnsAsync(pedidoExistente);
        _compradorRepositoryMock.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new Comprador { Id = 2, Nome = "Comprador Y", Documento = "2", CidadeId = 1 });

        var dto = new PedidoDto(DateTime.Today, 2, new List<PedidoItemDto> { new(1, 100m, Moeda.BRL) });

        await _service.AtualizarAsync(1, dto);

        _cotacaoServiceMock.Verify(c => c.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()), Times.Never);
        Assert.Equal(2, pedidoExistente.CompradorId);
        Assert.Equal(DateTime.Today, pedidoExistente.Data);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_QuandoAlteraPrecoDeItem_DisparaNovaCotacao()
    {
        var pedidoExistente = new Pedido { Id = 1, Data = DateTime.Today, CompradorId = 1 };
        pedidoExistente.Itens.Add(new PedidoItem { Id = 1, PedidoId = 1, CarneId = 1, Preco = 100m, Moeda = Moeda.USD, CotacaoUsada = 5m });

        _pedidoRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<Expression<Func<Pedido, object>>[]>()))
            .ReturnsAsync(pedidoExistente);
        _compradorRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Comprador { Id = 1, Nome = "Comprador X", Documento = "1", CidadeId = 1 });

        _cotacaoServiceMock
            .Setup(c => c.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()))
            .ReturnsAsync(new Dictionary<Moeda, decimal> { [Moeda.USD] = 5.5m });

        var dto = new PedidoDto(DateTime.Today, 1, new List<PedidoItemDto> { new(1, 120m, Moeda.USD) });

        await _service.AtualizarAsync(1, dto);

        _cotacaoServiceMock.Verify(c => c.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()), Times.Once);
        Assert.Single(pedidoExistente.Itens);
        Assert.Equal(120m, pedidoExistente.Itens.First().Preco);
        Assert.Equal(5.5m, pedidoExistente.Itens.First().CotacaoUsada);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_QuandoCotacaoFalha_NaoAplicaAlteracaoEPreservaEstadoAnterior()
    {
        var dataOriginal = DateTime.Today.AddDays(-5);
        var pedidoExistente = new Pedido { Id = 1, Data = dataOriginal, CompradorId = 1 };
        pedidoExistente.Itens.Add(new PedidoItem { Id = 1, PedidoId = 1, CarneId = 1, Preco = 100m, Moeda = Moeda.USD, CotacaoUsada = 5m });

        _pedidoRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<Expression<Func<Pedido, object>>[]>()))
            .ReturnsAsync(pedidoExistente);
        _compradorRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Comprador { Id = 1, Nome = "Comprador X", Documento = "1", CidadeId = 1 });

        _cotacaoServiceMock
            .Setup(c => c.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()))
            .ThrowsAsync(new CotacaoIndisponivelException("Cotação indisponível."));

        // Preço mudou em relação ao item existente -> deveria disparar nova cotação, que falha.
        var dto = new PedidoDto(DateTime.Today, 1, new List<PedidoItemDto> { new(1, 999m, Moeda.USD) });

        await Assert.ThrowsAsync<CotacaoIndisponivelException>(() => _service.AtualizarAsync(1, dto));

        Assert.Equal(dataOriginal, pedidoExistente.Data);
        Assert.Equal(1, pedidoExistente.CompradorId);
        Assert.Single(pedidoExistente.Itens);
        Assert.Equal(100m, pedidoExistente.Itens.First().Preco);
        Assert.Equal(5m, pedidoExistente.Itens.First().CotacaoUsada);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
