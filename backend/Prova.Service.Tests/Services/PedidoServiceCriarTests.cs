using FluentValidation;
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

public class PedidoServiceCriarTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRepository<Pedido>> _pedidoRepositoryMock = new();
    private readonly Mock<IRepository<Comprador>> _compradorRepositoryMock = new();
    private readonly Mock<IRepository<Carne>> _carneRepositoryMock = new();
    private readonly Mock<ICotacaoService> _cotacaoServiceMock = new();
    private readonly PedidoService _service;

    public PedidoServiceCriarTests()
    {
        _unitOfWorkMock.Setup(u => u.Repository<Pedido>()).Returns(_pedidoRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Comprador>()).Returns(_compradorRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Carne>()).Returns(_carneRepositoryMock.Object);
        _service = new PedidoService(_unitOfWorkMock.Object, new PedidoDtoValidator(), _cotacaoServiceMock.Object);
    }

    [Fact]
    public async Task CriarAsync_ComItemBrlEItemUsd_CongelaCotacaoDeCadaItemEPersiste()
    {
        _compradorRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Comprador { Id = 1, Nome = "Comprador X", Documento = "1", CidadeId = 1 });
        _carneRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Carne { Id = 1, Descricao = "Picanha", Origem = OrigemCarne.Bovina });
        _carneRepositoryMock.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new Carne { Id = 2, Descricao = "Cordeiro importado", Origem = OrigemCarne.Bovina });

        _cotacaoServiceMock
            .Setup(c => c.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()))
            .ReturnsAsync(new Dictionary<Moeda, decimal> { [Moeda.BRL] = 1m, [Moeda.USD] = 5m });

        var dto = new PedidoDto(DateTime.Today, 1, new List<PedidoItemDto>
        {
            new(1, 100m, Moeda.BRL),
            new(2, 10m, Moeda.USD),
        });

        var resultado = await _service.CriarAsync(dto);

        Assert.Equal(2, resultado.Itens.Count);
        Assert.Equal(100m, resultado.Itens.First(i => i.CarneId == 1).ValorEmReal);
        Assert.Equal(50m, resultado.Itens.First(i => i.CarneId == 2).ValorEmReal);
        Assert.Equal(150m, resultado.ValorTotalEmReal);

        _pedidoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Pedido>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_ComListaDeItensVazia_LancaValidationExceptionENaoPersiste()
    {
        var dto = new PedidoDto(DateTime.Today, 1, new List<PedidoItemDto>());

        await Assert.ThrowsAsync<ValidationException>(() => _service.CriarAsync(dto));

        _pedidoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Pedido>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_QuandoCotacaoFalha_NaoCriaPedidoParcial()
    {
        _compradorRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Comprador { Id = 1, Nome = "Comprador X", Documento = "1", CidadeId = 1 });
        _carneRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Carne { Id = 1, Descricao = "Picanha", Origem = OrigemCarne.Bovina });

        _cotacaoServiceMock
            .Setup(c => c.ObterCotacoesAsync(It.IsAny<IEnumerable<Moeda>>()))
            .ThrowsAsync(new CotacaoIndisponivelException("Cotação indisponível."));

        var dto = new PedidoDto(DateTime.Today, 1, new List<PedidoItemDto> { new(1, 10m, Moeda.USD) });

        await Assert.ThrowsAsync<CotacaoIndisponivelException>(() => _service.CriarAsync(dto));

        _pedidoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Pedido>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
