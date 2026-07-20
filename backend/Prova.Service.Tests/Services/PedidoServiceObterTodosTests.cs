using Moq;
using Prova.Data.Repositories;
using Prova.Model.Entities;
using Prova.Model.Enums;
using Prova.Service.Cotacao;
using Prova.Service.Services;
using Prova.Service.Validators;
using Xunit;

namespace Prova.Service.Tests.Services;

/// <summary>
/// Testes de <see cref="PedidoService.ObterTodosAsync"/> (T35): filtros
/// opcionais e combináveis por <c>compradorId</c> e intervalo de
/// <c>Pedido.Data</c>, aplicados em memória sobre a lista completa retornada
/// pelo repositório (não acopla <c>IRepository&lt;T&gt;</c> a uma
/// necessidade específica de Pedido).
/// </summary>
public class PedidoServiceObterTodosTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRepository<Pedido>> _pedidoRepositoryMock = new();
    private readonly Mock<ICotacaoService> _cotacaoServiceMock = new();
    private readonly PedidoService _service;

    public PedidoServiceObterTodosTests()
    {
        _unitOfWorkMock.Setup(u => u.Repository<Pedido>()).Returns(_pedidoRepositoryMock.Object);
        _service = new PedidoService(_unitOfWorkMock.Object, new PedidoDtoValidator(), _cotacaoServiceMock.Object);
    }

    private static Pedido CriarPedido(int id, int compradorId, DateTime data)
    {
        return new Pedido
        {
            Id = id,
            CompradorId = compradorId,
            Data = data,
            Itens = new List<PedidoItem>
            {
                new() { Id = id, CarneId = 1, Preco = 10m, Moeda = Moeda.BRL, CotacaoUsada = 1m },
            },
        };
    }

    [Fact]
    public async Task ObterTodosAsync_SemFiltros_RetornaTodosOsPedidos()
    {
        var pedidos = new List<Pedido>
        {
            CriarPedido(1, compradorId: 1, data: new DateTime(2026, 1, 5)),
            CriarPedido(2, compradorId: 2, data: new DateTime(2026, 2, 10)),
        };
        _pedidoRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Pedido, object>>[]>()))
            .ReturnsAsync(pedidos);

        var resultado = await _service.ObterTodosAsync();

        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public async Task ObterTodosAsync_ComFiltroDeCompradorId_RetornaApenasPedidosDoComprador()
    {
        var pedidos = new List<Pedido>
        {
            CriarPedido(1, compradorId: 1, data: new DateTime(2026, 1, 5)),
            CriarPedido(2, compradorId: 2, data: new DateTime(2026, 2, 10)),
        };
        _pedidoRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Pedido, object>>[]>()))
            .ReturnsAsync(pedidos);

        var resultado = await _service.ObterTodosAsync(compradorId: 1);

        var pedido = Assert.Single(resultado);
        Assert.Equal(1, pedido.CompradorId);
    }

    [Fact]
    public async Task ObterTodosAsync_ComFiltroDeIntervaloDeData_RetornaApenasPedidosNoIntervalo()
    {
        var pedidos = new List<Pedido>
        {
            CriarPedido(1, compradorId: 1, data: new DateTime(2026, 1, 5)),
            CriarPedido(2, compradorId: 2, data: new DateTime(2026, 2, 10)),
            CriarPedido(3, compradorId: 3, data: new DateTime(2026, 3, 20)),
        };
        _pedidoRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Pedido, object>>[]>()))
            .ReturnsAsync(pedidos);

        var resultado = await _service.ObterTodosAsync(
            dataInicio: new DateTime(2026, 1, 1),
            dataFim: new DateTime(2026, 2, 28));

        Assert.Equal(2, resultado.Count);
        Assert.Contains(resultado, p => p.Id == 1);
        Assert.Contains(resultado, p => p.Id == 2);
    }

    [Fact]
    public async Task ObterTodosAsync_ComFiltroCombinado_AplicaAndENaoRetornaPedidoQueBateApenasUmCriterio()
    {
        var pedidos = new List<Pedido>
        {
            // Comprador certo, mas fora do intervalo de data.
            CriarPedido(1, compradorId: 1, data: new DateTime(2026, 5, 1)),
            // Dentro do intervalo, mas comprador diferente.
            CriarPedido(2, compradorId: 2, data: new DateTime(2026, 1, 15)),
            // Bate os dois critérios.
            CriarPedido(3, compradorId: 1, data: new DateTime(2026, 1, 20)),
        };
        _pedidoRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Pedido, object>>[]>()))
            .ReturnsAsync(pedidos);

        var resultado = await _service.ObterTodosAsync(
            compradorId: 1,
            dataInicio: new DateTime(2026, 1, 1),
            dataFim: new DateTime(2026, 1, 31));

        var pedido = Assert.Single(resultado);
        Assert.Equal(3, pedido.Id);
    }
}
