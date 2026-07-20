using System.Linq.Expressions;
using FluentValidation;
using Moq;
using Prova.Data.Repositories;
using Prova.Model.Entities;
using Prova.Model.Enums;
using Prova.Service.Dtos;
using Prova.Service.Exceptions;
using Prova.Service.Services;
using Prova.Service.Validators;
using Xunit;

namespace Prova.Service.Tests.Services;

public class CarneServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRepository<Carne>> _carneRepositoryMock = new();
    private readonly CarneService _service;

    public CarneServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Repository<Carne>()).Returns(_carneRepositoryMock.Object);
        _service = new CarneService(_unitOfWorkMock.Object, new CarneDtoValidator());
    }

    [Fact]
    public async Task ExcluirAsync_QuandoExistePedidoItemVinculado_LancaEntidadeVinculadaExceptionENaoRemove()
    {
        var carne = new Carne { Id = 1, Descricao = "Picanha", Origem = OrigemCarne.Bovina };
        carne.PedidoItens.Add(new PedidoItem { Id = 10, CarneId = 1 });

        _carneRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<Expression<Func<Carne, object>>[]>()))
            .ReturnsAsync(carne);

        await Assert.ThrowsAsync<EntidadeVinculadaException>(() => _service.ExcluirAsync(1));

        _carneRepositoryMock.Verify(r => r.Remove(It.IsAny<Carne>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ExcluirAsync_QuandoNaoExisteVinculo_RemoveComSucesso()
    {
        var carne = new Carne { Id = 2, Descricao = "Filé", Origem = OrigemCarne.Bovina };

        _carneRepositoryMock
            .Setup(r => r.GetByIdAsync(2, It.IsAny<Expression<Func<Carne, object>>[]>()))
            .ReturnsAsync(carne);

        await _service.ExcluirAsync(2);

        _carneRepositoryMock.Verify(r => r.Remove(carne), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_QuandoDescricaoVazia_LancaValidationExceptionENaoPersiste()
    {
        var dto = new CarneDto(string.Empty, OrigemCarne.Bovina);

        await Assert.ThrowsAsync<ValidationException>(() => _service.CriarAsync(dto));

        _carneRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Carne>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_ComDadosValidos_PersisteERetornaDto()
    {
        var dto = new CarneDto("Alcatra", OrigemCarne.Bovina);

        var resultado = await _service.CriarAsync(dto);

        Assert.Equal("Alcatra", resultado.Descricao);
        _carneRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Carne>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoIdExiste_RetornaDto()
    {
        var carne = new Carne { Id = 5, Descricao = "Cupim", Origem = OrigemCarne.Bovina };

        _carneRepositoryMock
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(carne);

        var resultado = await _service.ObterPorIdAsync(5);

        Assert.NotNull(resultado);
        Assert.Equal("Cupim", resultado!.Descricao);
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoIdNaoExiste_RetornaNull()
    {
        _carneRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Carne?)null);

        var resultado = await _service.ObterPorIdAsync(99);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task AtualizarAsync_ComIdExistenteEDadosValidos_AtualizaEPersiste()
    {
        var carne = new Carne { Id = 3, Descricao = "Fraldinha", Origem = OrigemCarne.Bovina };
        var dto = new CarneDto("Fraldinha Especial", OrigemCarne.Suina);

        _carneRepositoryMock
            .Setup(r => r.GetByIdAsync(3))
            .ReturnsAsync(carne);

        await _service.AtualizarAsync(3, dto);

        Assert.Equal("Fraldinha Especial", carne.Descricao);
        Assert.Equal(OrigemCarne.Suina, carne.Origem);
        _carneRepositoryMock.Verify(r => r.Update(carne), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_QuandoIdNaoExiste_LancaEntidadeNaoEncontradaExceptionENaoPersiste()
    {
        var dto = new CarneDto("Fraldinha", OrigemCarne.Bovina);

        _carneRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Carne?)null);

        await Assert.ThrowsAsync<EntidadeNaoEncontradaException>(() => _service.AtualizarAsync(99, dto));

        _carneRepositoryMock.Verify(r => r.Update(It.IsAny<Carne>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ExcluirAsync_QuandoIdNaoExiste_LancaEntidadeNaoEncontradaExceptionENaoRemove()
    {
        _carneRepositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<Expression<Func<Carne, object>>[]>()))
            .ReturnsAsync((Carne?)null);

        await Assert.ThrowsAsync<EntidadeNaoEncontradaException>(() => _service.ExcluirAsync(99));

        _carneRepositoryMock.Verify(r => r.Remove(It.IsAny<Carne>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
