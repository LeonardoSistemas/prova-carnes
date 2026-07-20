using Moq;
using Prova.Data.Repositories;
using Prova.Model.Entities;
using Prova.Model.Enums;
using Prova.Service.Services;
using Xunit;

namespace Prova.Service.Tests.Services;

/// <summary>
/// Testes de <see cref="DashboardService.ObterResumoAsync"/> (T59): conversão
/// de <see cref="PeriodoDashboard"/> em intervalo de datas, cálculo de
/// faturamento/ticket médio e contagem de compradores ativos vs.
/// cadastrados. Também cobre <see cref="DashboardService.ObterFaturamentoPorDiaAsync"/>
/// (T61): série diária para os últimos N dias, sem buracos nos dias sem
/// pedido. As datas dos pedidos de cada teste são relativas a
/// <see cref="DateTime.Today"/> para não depender da data em que a suíte é
/// executada.
/// </summary>
public class DashboardServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRepository<Pedido>> _pedidoRepositoryMock = new();
    private readonly Mock<IRepository<Comprador>> _compradorRepositoryMock = new();
    private readonly Mock<IRepository<Carne>> _carneRepositoryMock = new();
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Repository<Pedido>()).Returns(_pedidoRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Comprador>()).Returns(_compradorRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Carne>()).Returns(_carneRepositoryMock.Object);
        _service = new DashboardService(_unitOfWorkMock.Object);

        // Repositórios não usados por um teste específico de
        // ObterTopCarnesECompradoresAsync ainda precisam de um retorno
        // configurado (o método sempre consulta os três), então cada teste
        // configura explicitamente o que importa e os demais caem no default
        // de lista vazia definido aqui.
        _carneRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Carne>());
        _compradorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Comprador>());
    }

    private static Pedido CriarPedido(int id, int compradorId, DateTime data, decimal valorItem)
    {
        return new Pedido
        {
            Id = id,
            CompradorId = compradorId,
            Data = data,
            Itens = new List<PedidoItem>
            {
                new() { Id = id, CarneId = 1, Preco = valorItem, Moeda = Moeda.BRL, CotacaoUsada = 1m },
            },
        };
    }

    /// <summary>
    /// Monta um Pedido com um único PedidoItem para uma Carne específica,
    /// usado pelos testes de <see cref="ObterTopCarnesECompradoresAsync_MaisDe5Carnes_RetornaApenasAsTop5PorValorDesc"/>
    /// e correlatos, onde o que importa é o agrupamento por CarneId/CompradorId,
    /// não o conteúdo do item em si.
    /// </summary>
    private static Pedido CriarPedidoComItemDeCarne(int id, int compradorId, int carneId, DateTime data, decimal valorItem)
    {
        return new Pedido
        {
            Id = id,
            CompradorId = compradorId,
            Data = data,
            Itens = new List<PedidoItem>
            {
                new() { Id = id, CarneId = carneId, Preco = valorItem, Moeda = Moeda.BRL, CotacaoUsada = 1m },
            },
        };
    }

    private void ConfigurarPedidos(IReadOnlyList<Pedido> pedidos)
    {
        _pedidoRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Pedido, object>>[]>()))
            .ReturnsAsync(pedidos);
    }

    private void ConfigurarCompradores(IReadOnlyList<Comprador> compradores)
    {
        _compradorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(compradores);
    }

    private void ConfigurarCarnes(IReadOnlyList<Carne> carnes)
    {
        _carneRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(carnes);
    }

    [Fact]
    public async Task ObterResumoAsync_PeriodoHoje_ContaApenasPedidosDeHoje()
    {
        var hoje = DateTime.Today;
        var pedidos = new List<Pedido>
        {
            CriarPedido(1, compradorId: 1, data: hoje, valorItem: 100m),
            CriarPedido(2, compradorId: 2, data: hoje.AddDays(-1), valorItem: 200m),
            CriarPedido(3, compradorId: 3, data: hoje.AddDays(-10), valorItem: 300m),
        };
        ConfigurarPedidos(pedidos);
        ConfigurarCompradores(new List<Comprador>());

        var resultado = await _service.ObterResumoAsync(PeriodoDashboard.Hoje);

        Assert.Equal(1, resultado.TotalPedidos);
        Assert.Equal(100m, resultado.FaturamentoTotal);
    }

    [Fact]
    public async Task ObterResumoAsync_PeriodoSemana_ContaUltimos7DiasIncluindoHojeEExcluiMaisAntigos()
    {
        var hoje = DateTime.Today;
        var pedidos = new List<Pedido>
        {
            CriarPedido(1, compradorId: 1, data: hoje, valorItem: 100m),
            CriarPedido(2, compradorId: 2, data: hoje.AddDays(-6), valorItem: 50m), // limite inferior, ainda dentro
            CriarPedido(3, compradorId: 3, data: hoje.AddDays(-7), valorItem: 999m), // fora (8º dia)
        };
        ConfigurarPedidos(pedidos);
        ConfigurarCompradores(new List<Comprador>());

        var resultado = await _service.ObterResumoAsync(PeriodoDashboard.Semana);

        Assert.Equal(2, resultado.TotalPedidos);
        Assert.Equal(150m, resultado.FaturamentoTotal);
    }

    [Fact]
    public async Task ObterResumoAsync_PeriodoMes_ContaApenasPedidosDoMesCorrenteAPartirDoDia1()
    {
        var hoje = DateTime.Today;
        var primeiroDiaDoMes = new DateTime(hoje.Year, hoje.Month, 1);
        var pedidos = new List<Pedido>
        {
            CriarPedido(1, compradorId: 1, data: hoje, valorItem: 100m),
            CriarPedido(2, compradorId: 2, data: primeiroDiaDoMes, valorItem: 50m), // limite inferior, dentro
            CriarPedido(3, compradorId: 3, data: primeiroDiaDoMes.AddDays(-1), valorItem: 999m), // mês anterior, fora
        };
        ConfigurarPedidos(pedidos);
        ConfigurarCompradores(new List<Comprador>());

        var resultado = await _service.ObterResumoAsync(PeriodoDashboard.Mes);

        Assert.Equal(2, resultado.TotalPedidos);
        Assert.Equal(150m, resultado.FaturamentoTotal);
    }

    [Fact]
    public async Task ObterResumoAsync_ComPedidosNoPeriodo_CalculaTicketMedioComoFaturamentoDivididoPorTotalDePedidos()
    {
        var hoje = DateTime.Today;
        var pedidos = new List<Pedido>
        {
            CriarPedido(1, compradorId: 1, data: hoje, valorItem: 100m),
            CriarPedido(2, compradorId: 2, data: hoje, valorItem: 300m),
        };
        ConfigurarPedidos(pedidos);
        ConfigurarCompradores(new List<Comprador>());

        var resultado = await _service.ObterResumoAsync(PeriodoDashboard.Hoje);

        Assert.Equal(400m, resultado.FaturamentoTotal);
        Assert.Equal(200m, resultado.TicketMedio);
    }

    [Fact]
    public async Task ObterResumoAsync_SemPedidosNoPeriodo_RetornaTicketMedioZeroSemLancarExcecao()
    {
        ConfigurarPedidos(new List<Pedido>());
        ConfigurarCompradores(new List<Comprador>());

        var resultado = await _service.ObterResumoAsync(PeriodoDashboard.Hoje);

        Assert.Equal(0, resultado.TotalPedidos);
        Assert.Equal(0m, resultado.FaturamentoTotal);
        Assert.Equal(0m, resultado.TicketMedio);
    }

    [Fact]
    public async Task ObterResumoAsync_CompradoresAtivos_ContaApenasCompradoresDistintosComPedidoNoPeriodo()
    {
        var hoje = DateTime.Today;
        var pedidos = new List<Pedido>
        {
            CriarPedido(1, compradorId: 1, data: hoje, valorItem: 100m),
            CriarPedido(2, compradorId: 1, data: hoje, valorItem: 50m), // mesmo comprador, 2º pedido
            CriarPedido(3, compradorId: 2, data: hoje, valorItem: 80m),
            CriarPedido(4, compradorId: 3, data: hoje.AddDays(-30), valorItem: 500m), // fora do período "hoje"
        };
        ConfigurarPedidos(pedidos);
        ConfigurarCompradores(new List<Comprador>());

        var resultado = await _service.ObterResumoAsync(PeriodoDashboard.Hoje);

        Assert.Equal(2, resultado.CompradoresAtivos);
    }

    [Fact]
    public async Task ObterResumoAsync_CompradoresCadastrados_ContaTodosOsCompradoresIndependenteDoPeriodoEDePedidos()
    {
        ConfigurarPedidos(new List<Pedido>());
        ConfigurarCompradores(new List<Comprador>
        {
            new() { Id = 1, Nome = "Comprador 1", Documento = "111", CidadeId = 1 },
            new() { Id = 2, Nome = "Comprador 2", Documento = "222", CidadeId = 1 },
            new() { Id = 3, Nome = "Comprador 3", Documento = "333", CidadeId = 1 },
        });

        var resultado = await _service.ObterResumoAsync(PeriodoDashboard.Mes);

        Assert.Equal(3, resultado.CompradoresCadastrados);
        Assert.Equal(0, resultado.CompradoresAtivos);
    }

    [Fact]
    public async Task ObterTopCarnesECompradoresAsync_MaisDe6Carnes_RetornaApenasTop5PorValorDesc()
    {
        var hoje = DateTime.Today;
        var pedidos = new List<Pedido>
        {
            CriarPedidoComItemDeCarne(1, compradorId: 1, carneId: 1, data: hoje, valorItem: 100m),
            CriarPedidoComItemDeCarne(2, compradorId: 1, carneId: 2, data: hoje, valorItem: 600m),
            CriarPedidoComItemDeCarne(3, compradorId: 1, carneId: 3, data: hoje, valorItem: 500m),
            CriarPedidoComItemDeCarne(4, compradorId: 1, carneId: 4, data: hoje, valorItem: 400m),
            CriarPedidoComItemDeCarne(5, compradorId: 1, carneId: 5, data: hoje, valorItem: 300m),
            CriarPedidoComItemDeCarne(6, compradorId: 1, carneId: 6, data: hoje, valorItem: 200m),
        };
        ConfigurarPedidos(pedidos);
        ConfigurarCarnes(new List<Carne>
        {
            new() { Id = 1, Descricao = "Picanha", Origem = OrigemCarne.Bovina },
            new() { Id = 2, Descricao = "Alcatra", Origem = OrigemCarne.Bovina },
            new() { Id = 3, Descricao = "Fraldinha", Origem = OrigemCarne.Bovina },
            new() { Id = 4, Descricao = "Costela", Origem = OrigemCarne.Bovina },
            new() { Id = 5, Descricao = "Cupim", Origem = OrigemCarne.Bovina },
            new() { Id = 6, Descricao = "Maminha", Origem = OrigemCarne.Bovina },
        });

        var resultado = await _service.ObterTopCarnesECompradoresAsync(PeriodoDashboard.Hoje);

        Assert.Equal(5, resultado.TopCarnes.Count);
        Assert.Equal(new[] { "Alcatra", "Fraldinha", "Costela", "Cupim", "Maminha" },
            resultado.TopCarnes.Select(c => c.Descricao));
        Assert.DoesNotContain(resultado.TopCarnes, c => c.Descricao == "Picanha");
        Assert.Equal(600m, resultado.TopCarnes[0].ValorTotal);
    }

    [Fact]
    public async Task ObterTopCarnesECompradoresAsync_EmpateDeValorEntreCarnes_DesempataPorDescricaoAscendente()
    {
        var hoje = DateTime.Today;
        var pedidos = new List<Pedido>
        {
            CriarPedidoComItemDeCarne(1, compradorId: 1, carneId: 1, data: hoje, valorItem: 100m), // "Zebu"
            CriarPedidoComItemDeCarne(2, compradorId: 1, carneId: 2, data: hoje, valorItem: 100m), // "Angus" - mesmo valor
        };
        ConfigurarPedidos(pedidos);
        ConfigurarCarnes(new List<Carne>
        {
            new() { Id = 1, Descricao = "Zebu", Origem = OrigemCarne.Bovina },
            new() { Id = 2, Descricao = "Angus", Origem = OrigemCarne.Bovina },
        });

        var resultado = await _service.ObterTopCarnesECompradoresAsync(PeriodoDashboard.Hoje);

        Assert.Equal(new[] { "Angus", "Zebu" }, resultado.TopCarnes.Select(c => c.Descricao));
    }

    [Fact]
    public async Task ObterTopCarnesECompradoresAsync_MaisDe6Compradores_RetornaApenasTop5PorValorDesc()
    {
        var hoje = DateTime.Today;
        var pedidos = new List<Pedido>
        {
            CriarPedidoComItemDeCarne(1, compradorId: 1, carneId: 1, data: hoje, valorItem: 100m),
            CriarPedidoComItemDeCarne(2, compradorId: 2, carneId: 1, data: hoje, valorItem: 600m),
            CriarPedidoComItemDeCarne(3, compradorId: 3, carneId: 1, data: hoje, valorItem: 500m),
            CriarPedidoComItemDeCarne(4, compradorId: 4, carneId: 1, data: hoje, valorItem: 400m),
            CriarPedidoComItemDeCarne(5, compradorId: 5, carneId: 1, data: hoje, valorItem: 300m),
            CriarPedidoComItemDeCarne(6, compradorId: 6, carneId: 1, data: hoje, valorItem: 200m),
        };
        ConfigurarPedidos(pedidos);
        ConfigurarCompradores(new List<Comprador>
        {
            new() { Id = 1, Nome = "Comprador 1", Documento = "1", CidadeId = 1 },
            new() { Id = 2, Nome = "Comprador 2", Documento = "2", CidadeId = 1 },
            new() { Id = 3, Nome = "Comprador 3", Documento = "3", CidadeId = 1 },
            new() { Id = 4, Nome = "Comprador 4", Documento = "4", CidadeId = 1 },
            new() { Id = 5, Nome = "Comprador 5", Documento = "5", CidadeId = 1 },
            new() { Id = 6, Nome = "Comprador 6", Documento = "6", CidadeId = 1 },
        });

        var resultado = await _service.ObterTopCarnesECompradoresAsync(PeriodoDashboard.Hoje);

        Assert.Equal(5, resultado.TopCompradores.Count);
        Assert.Equal(
            new[] { "Comprador 2", "Comprador 3", "Comprador 4", "Comprador 5", "Comprador 6" },
            resultado.TopCompradores.Select(c => c.Nome));
        Assert.DoesNotContain(resultado.TopCompradores, c => c.Nome == "Comprador 1");
        Assert.Equal(600m, resultado.TopCompradores[0].ValorTotal);
    }

    [Fact]
    public async Task ObterTopCarnesECompradoresAsync_EmpateDeValorEntreCompradores_DesempataPorNomeAscendente()
    {
        var hoje = DateTime.Today;
        var pedidos = new List<Pedido>
        {
            CriarPedidoComItemDeCarne(1, compradorId: 1, carneId: 1, data: hoje, valorItem: 100m), // "Zeca"
            CriarPedidoComItemDeCarne(2, compradorId: 2, carneId: 1, data: hoje, valorItem: 100m), // "Ana" - mesmo valor
        };
        ConfigurarPedidos(pedidos);
        ConfigurarCompradores(new List<Comprador>
        {
            new() { Id = 1, Nome = "Zeca", Documento = "1", CidadeId = 1 },
            new() { Id = 2, Nome = "Ana", Documento = "2", CidadeId = 1 },
        });

        var resultado = await _service.ObterTopCarnesECompradoresAsync(PeriodoDashboard.Hoje);

        Assert.Equal(new[] { "Ana", "Zeca" }, resultado.TopCompradores.Select(c => c.Nome));
    }

    [Fact]
    public async Task ObterTopCarnesECompradoresAsync_PeriodoSemPedidos_RetornaListasVaziasSemLancarExcecao()
    {
        ConfigurarPedidos(new List<Pedido>());
        ConfigurarCarnes(new List<Carne>
        {
            new() { Id = 1, Descricao = "Picanha", Origem = OrigemCarne.Bovina },
        });
        ConfigurarCompradores(new List<Comprador>
        {
            new() { Id = 1, Nome = "Comprador 1", Documento = "1", CidadeId = 1 },
        });

        var resultado = await _service.ObterTopCarnesECompradoresAsync(PeriodoDashboard.Hoje);

        Assert.Empty(resultado.TopCarnes);
        Assert.Empty(resultado.TopCompradores);
    }

    [Fact]
    public async Task ObterFaturamentoPorDiaAsync_7Dias_RetornaListaCom7ItensEmOrdemCronologicaAscendente()
    {
        ConfigurarPedidos(new List<Pedido>());

        var resultado = await _service.ObterFaturamentoPorDiaAsync(7);

        var hoje = DateTime.Today;
        Assert.Equal(7, resultado.Count);
        Assert.Equal(hoje.AddDays(-6), resultado[0].Data);
        Assert.Equal(hoje, resultado[^1].Data);
        Assert.True(resultado.Zip(resultado.Skip(1), (anterior, proximo) => proximo.Data > anterior.Data).All(x => x));
    }

    [Fact]
    public async Task ObterFaturamentoPorDiaAsync_30Dias_RetornaListaCom30ItensIniciandoNoDiaCorreto()
    {
        ConfigurarPedidos(new List<Pedido>());

        var resultado = await _service.ObterFaturamentoPorDiaAsync(30);

        var hoje = DateTime.Today;
        Assert.Equal(30, resultado.Count);
        Assert.Equal(hoje.AddDays(-29), resultado[0].Data);
        Assert.Equal(hoje, resultado[^1].Data);
    }

    [Fact]
    public async Task ObterFaturamentoPorDiaAsync_DiaSemPedido_ApareceNaListaComFaturamentoZeroSemFicarAusente()
    {
        var hoje = DateTime.Today;
        var pedidos = new List<Pedido>
        {
            CriarPedido(1, compradorId: 1, data: hoje, valorItem: 100m),
            CriarPedido(2, compradorId: 2, data: hoje.AddDays(-2), valorItem: 50m),
            // hoje.AddDays(-1) fica sem nenhum pedido, no meio da série.
        };
        ConfigurarPedidos(pedidos);

        var resultado = await _service.ObterFaturamentoPorDiaAsync(7);

        var diaSemPedido = resultado.Single(item => item.Data == hoje.AddDays(-1));
        Assert.Equal(0m, diaSemPedido.Faturamento);
        Assert.Equal(7, resultado.Count); // dia sem pedido não foi omitido da lista
    }

    [Fact]
    public async Task ObterFaturamentoPorDiaAsync_MaisDeUmPedidoNoMesmoDia_SomaOsValoresDoDia()
    {
        var hoje = DateTime.Today;
        var pedidos = new List<Pedido>
        {
            CriarPedido(1, compradorId: 1, data: hoje, valorItem: 100m),
            CriarPedido(2, compradorId: 2, data: hoje, valorItem: 250m),
        };
        ConfigurarPedidos(pedidos);

        var resultado = await _service.ObterFaturamentoPorDiaAsync(7);

        var faturamentoDeHoje = resultado.Single(item => item.Data == hoje).Faturamento;
        Assert.Equal(350m, faturamentoDeHoje);
    }

    [Fact]
    public async Task ObterFaturamentoPorDiaAsync_DiasZeroOuNegativo_LancaArgumentOutOfRangeException()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.ObterFaturamentoPorDiaAsync(0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.ObterFaturamentoPorDiaAsync(-1));
    }
}
