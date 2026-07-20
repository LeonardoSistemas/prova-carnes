using System.Linq.Expressions;
using Moq;
using Prova.Data.Repositories;
using Prova.Model.Entities;
using Prova.Service.Dtos;
using Prova.Service.Exceptions;
using Prova.Service.Services;
using Prova.Service.Validators;
using Xunit;

namespace Prova.Service.Tests.Services;

public class CompradorServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRepository<Comprador>> _compradorRepositoryMock = new();
    private readonly Mock<IRepository<Cidade>> _cidadeRepositoryMock = new();
    private readonly CompradorService _service;

    public CompradorServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Repository<Comprador>()).Returns(_compradorRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Cidade>()).Returns(_cidadeRepositoryMock.Object);
        _service = new CompradorService(_unitOfWorkMock.Object, new CompradorDtoValidator());
    }

    [Fact]
    public async Task ExcluirAsync_QuandoExistePedidoVinculado_LancaEntidadeVinculadaExceptionENaoRemove()
    {
        var comprador = new Comprador { Id = 1, Nome = "Comprador A", Documento = "123", CidadeId = 1 };
        comprador.Pedidos.Add(new Pedido { Id = 5, CompradorId = 1 });

        _compradorRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<Expression<Func<Comprador, object>>[]>()))
            .ReturnsAsync(comprador);

        await Assert.ThrowsAsync<EntidadeVinculadaException>(() => _service.ExcluirAsync(1));

        _compradorRepositoryMock.Verify(r => r.Remove(It.IsAny<Comprador>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ExcluirAsync_QuandoNaoExisteVinculo_RemoveComSucesso()
    {
        var comprador = new Comprador { Id = 2, Nome = "Comprador B", Documento = "456", CidadeId = 1 };

        _compradorRepositoryMock
            .Setup(r => r.GetByIdAsync(2, It.IsAny<Expression<Func<Comprador, object>>[]>()))
            .ReturnsAsync(comprador);

        await _service.ExcluirAsync(2);

        _compradorRepositoryMock.Verify(r => r.Remove(comprador), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_QuandoCidadeNaoExiste_LancaEntidadeNaoEncontradaExceptionENaoPersiste()
    {
        var dto = new CompradorDto("Comprador C", "789", CidadeId: 999);

        _cidadeRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Cidade?)null);

        await Assert.ThrowsAsync<EntidadeNaoEncontradaException>(() => _service.CriarAsync(dto));

        _compradorRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Comprador>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_QuandoCidadeExiste_PersisteERetornaDto()
    {
        var dto = new CompradorDto("Comprador D", "789", CidadeId: 1);

        _cidadeRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Cidade { Id = 1, Nome = "Bebedouro", EstadoId = 1 });

        var resultado = await _service.CriarAsync(dto);

        Assert.Equal("Comprador D", resultado.Nome);
        _compradorRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Comprador>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoIdExiste_RetornaDto()
    {
        var comprador = new Comprador { Id = 6, Nome = "Comprador H", Documento = "555", CidadeId = 1 };

        _compradorRepositoryMock
            .Setup(r => r.GetByIdAsync(6))
            .ReturnsAsync(comprador);

        var resultado = await _service.ObterPorIdAsync(6);

        Assert.NotNull(resultado);
        Assert.Equal("Comprador H", resultado!.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoIdNaoExiste_RetornaNull()
    {
        _compradorRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Comprador?)null);

        var resultado = await _service.ObterPorIdAsync(99);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task AtualizarAsync_ComIdECidadeExistentes_AtualizaEPersiste()
    {
        var comprador = new Comprador { Id = 3, Nome = "Comprador E", Documento = "111", CidadeId = 1 };
        var dto = new CompradorDto("Comprador E Atualizado", "222", CidadeId: 2);

        _compradorRepositoryMock
            .Setup(r => r.GetByIdAsync(3))
            .ReturnsAsync(comprador);

        _cidadeRepositoryMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new Cidade { Id = 2, Nome = "Ribeirão Preto", EstadoId = 1 });

        await _service.AtualizarAsync(3, dto);

        Assert.Equal("Comprador E Atualizado", comprador.Nome);
        Assert.Equal("222", comprador.Documento);
        Assert.Equal(2, comprador.CidadeId);
        _compradorRepositoryMock.Verify(r => r.Update(comprador), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_QuandoIdNaoExiste_LancaEntidadeNaoEncontradaExceptionENaoConsultaCidade()
    {
        var dto = new CompradorDto("Comprador F", "333", CidadeId: 1);

        _compradorRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Comprador?)null);

        await Assert.ThrowsAsync<EntidadeNaoEncontradaException>(() => _service.AtualizarAsync(99, dto));

        _cidadeRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _compradorRepositoryMock.Verify(r => r.Update(It.IsAny<Comprador>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AtualizarAsync_QuandoCidadeNovaNaoExiste_LancaEntidadeNaoEncontradaExceptionENaoPersiste()
    {
        var comprador = new Comprador { Id = 4, Nome = "Comprador G", Documento = "444", CidadeId = 1 };
        var dto = new CompradorDto("Comprador G", "444", CidadeId: 999);

        _compradorRepositoryMock
            .Setup(r => r.GetByIdAsync(4))
            .ReturnsAsync(comprador);

        _cidadeRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Cidade?)null);

        await Assert.ThrowsAsync<EntidadeNaoEncontradaException>(() => _service.AtualizarAsync(4, dto));

        Assert.Equal(1, comprador.CidadeId);
        _compradorRepositoryMock.Verify(r => r.Update(It.IsAny<Comprador>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ExcluirAsync_QuandoIdNaoExiste_LancaEntidadeNaoEncontradaExceptionENaoRemove()
    {
        _compradorRepositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<Expression<Func<Comprador, object>>[]>()))
            .ReturnsAsync((Comprador?)null);

        await Assert.ThrowsAsync<EntidadeNaoEncontradaException>(() => _service.ExcluirAsync(99));

        _compradorRepositoryMock.Verify(r => r.Remove(It.IsAny<Comprador>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
