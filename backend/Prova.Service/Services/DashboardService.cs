using Prova.Data.Repositories;
using Prova.Model.Entities;
using Prova.Model.Enums;
using Prova.Service.Dtos;

namespace Prova.Service.Services;

/// <summary>
/// Métricas de topo do Dashboard (T59): total de pedidos, faturamento e
/// ticket médio no período, compradores ativos vs. cadastrados.
///
/// Sem <c>IValidator&lt;T&gt;</c> injetado — o único "input" é o enum
/// <see cref="PeriodoDashboard"/>, que já é validado pelo próprio compilador
/// (não existe valor de enum inválido alcançável sem cast explícito; o
/// <c>default</c> do <c>switch</c> em <see cref="ObterIntervaloDoPeriodo"/>
/// cobre esse caso defensivamente).
///
/// Segue o mesmo padrão de filtro em memória de
/// <see cref="PedidoService.ObterTodosAsync"/> (T35):
/// <see cref="IRepository{T}"/> não expõe <c>Where</c>/<c>AnyAsync</c> — os
/// dados completos são trazidos para memória e filtrados em LINQ, aceitável
/// dado o volume de dados desta prova.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardResumoDto> ObterResumoAsync(PeriodoDashboard periodo)
    {
        var (inicio, fim) = ObterIntervaloDoPeriodo(periodo);

        var pedidos = await _unitOfWork.Repository<Pedido>().GetAllAsync(p => p.Itens);
        var pedidosNoPeriodo = pedidos
            .Where(p => p.Data.Date >= inicio && p.Data.Date <= fim)
            .ToList();

        var totalPedidos = pedidosNoPeriodo.Count;
        var faturamentoTotal = pedidosNoPeriodo.Sum(p => p.ValorTotalEmReal);

        // Guarda explícita: ticket médio é 0 quando não há pedidos no
        // período, nunca DivideByZeroException/NaN (T59).
        var ticketMedio = totalPedidos == 0 ? 0m : faturamentoTotal / totalPedidos;

        var compradoresAtivos = pedidosNoPeriodo
            .Select(p => p.CompradorId)
            .Distinct()
            .Count();

        var compradores = await _unitOfWork.Repository<Comprador>().GetAllAsync();

        return new DashboardResumoDto(
            totalPedidos,
            faturamentoTotal,
            ticketMedio,
            compradoresAtivos,
            compradores.Count);
    }

    /// <summary>
    /// Calcula o Top 5 carnes e o Top 5 compradores por valor em Real, no
    /// mesmo período (T60). Reaproveita <see cref="ObterIntervaloDoPeriodo"/>
    /// (mesma conversão de período em intervalo de datas usada por
    /// <see cref="ObterResumoAsync"/>) para não duplicar essa regra.
    ///
    /// <see cref="PedidoItem.Carne"/> e <see cref="Pedido.Comprador"/> não são
    /// carregados via <c>Include</c> aqui porque <see cref="IRepository{T}"/>
    /// só suporta include de um nível (ver comentário em
    /// <c>Repository.ApplyIncludes</c>) — em vez de encadear includes,
    /// Carne e Comprador são carregados à parte e combinados em memória por
    /// Id, mesmo padrão de "trazer para memória e cruzar em LINQ" já usado
    /// nesta classe e em <see cref="PedidoService"/>.
    /// </summary>
    public async Task<DashboardTopDto> ObterTopCarnesECompradoresAsync(PeriodoDashboard periodo)
    {
        var (inicio, fim) = ObterIntervaloDoPeriodo(periodo);

        var pedidos = await _unitOfWork.Repository<Pedido>().GetAllAsync(p => p.Itens);
        var pedidosNoPeriodo = pedidos
            .Where(p => p.Data.Date >= inicio && p.Data.Date <= fim)
            .ToList();

        var carnes = await _unitOfWork.Repository<Carne>().GetAllAsync();
        var descricaoPorCarneId = carnes.ToDictionary(c => c.Id, c => c.Descricao);

        var compradores = await _unitOfWork.Repository<Comprador>().GetAllAsync();
        var nomePorCompradorId = compradores.ToDictionary(c => c.Id, c => c.Nome);

        var topCarnes = pedidosNoPeriodo
            .SelectMany(p => p.Itens)
            .GroupBy(item => item.CarneId)
            .Select(grupo => new TopCarneDto(
                grupo.Key,
                descricaoPorCarneId.GetValueOrDefault(grupo.Key, string.Empty),
                grupo.Sum(item => item.ValorEmReal)))
            .OrderByDescending(top => top.ValorTotal)
            .ThenBy(top => top.Descricao, StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();

        var topCompradores = pedidosNoPeriodo
            .GroupBy(p => p.CompradorId)
            .Select(grupo => new TopCompradorDto(
                grupo.Key,
                nomePorCompradorId.GetValueOrDefault(grupo.Key, string.Empty),
                grupo.Sum(p => p.ValorTotalEmReal)))
            .OrderByDescending(top => top.ValorTotal)
            .ThenBy(top => top.Nome, StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();

        return new DashboardTopDto(topCarnes, topCompradores);
    }

    /// <summary>
    /// Série de faturamento diário para o gráfico de linha do Dashboard
    /// (T61). Diferente de <see cref="ObterResumoAsync"/> e
    /// <see cref="ObterTopCarnesECompradoresAsync"/>, não recebe
    /// <see cref="PeriodoDashboard"/> — recebe diretamente a quantidade de
    /// dias corridos (<paramref name="dias"/>), decisão da task T61 para
    /// permitir granularidade livre (ex: 7 ou 30) independente do enum de
    /// período usado nas outras métricas.
    ///
    /// <paramref name="dias"/> deve ser positivo: é validado aqui com
    /// <see cref="ArgumentOutOfRangeException"/> em vez de retornar lista
    /// vazia silenciosamente, porque será exposto via query string no
    /// Controller (T62) — um valor inválido (0, negativo) é erro de input do
    /// chamador, não um "sem dados" legítimo, e deve virar 400 no Controller
    /// (mapeado pelo middleware de exceção global), não uma resposta 200
    /// enganosa com lista vazia.
    ///
    /// Dias sem nenhum pedido entram no resultado com faturamento 0 — a
    /// lista é sempre construída dia a dia a partir do intervalo (não a
    /// partir dos pedidos encontrados), para que um dia sem pedido não fique
    /// ausente e quebre a continuidade do gráfico de linha.
    /// </summary>
    public async Task<IReadOnlyList<FaturamentoPorDiaDto>> ObterFaturamentoPorDiaAsync(int dias)
    {
        if (dias <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dias), dias, "A quantidade de dias deve ser maior que zero.");
        }

        var hoje = DateTime.Today;
        var inicio = hoje.AddDays(-(dias - 1));

        var pedidos = await _unitOfWork.Repository<Pedido>().GetAllAsync(p => p.Itens);
        var faturamentoPorDia = pedidos
            .Where(p => p.Data.Date >= inicio && p.Data.Date <= hoje)
            .GroupBy(p => p.Data.Date)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(p => p.ValorTotalEmReal));

        var serie = new List<FaturamentoPorDiaDto>(dias);
        for (var dia = inicio; dia <= hoje; dia = dia.AddDays(1))
        {
            serie.Add(new FaturamentoPorDiaDto(dia, faturamentoPorDia.GetValueOrDefault(dia, 0m)));
        }

        return serie;
    }

    /// <summary>
    /// Converte o período em um intervalo de datas fechado (inclusive nos
    /// dois extremos, comparado via <c>DateTime.Date</c>), com
    /// <c>DateTime.Today</c> como referência de "agora":
    /// <list type="bullet">
    /// <item><description><c>Hoje</c>: só a data de hoje (00:00–23:59:59.999...).</description></item>
    /// <item><description><c>Semana</c>: últimos 7 dias corridos incluindo hoje (hoje - 6 dias até hoje).</description></item>
    /// <item><description>
    /// <c>Mes</c>: mês corrente, do dia 1 até hoje — não "últimos 30 dias".
    /// Escolhido porque alinha com a leitura mais comum de "mês" em
    /// dashboards de negócio (mês-calendário corrente) e porque o documento
    /// de referência (docs/Interface.txt) não especifica exatamente; se essa
    /// interpretação não for a esperada, ajustar aqui é uma mudança
    /// localizada a este método.
    /// </description></item>
    /// </list>
    /// </summary>
    private static (DateTime Inicio, DateTime Fim) ObterIntervaloDoPeriodo(PeriodoDashboard periodo)
    {
        var hoje = DateTime.Today;

        return periodo switch
        {
            PeriodoDashboard.Hoje => (hoje, hoje),
            PeriodoDashboard.Semana => (hoje.AddDays(-6), hoje),
            PeriodoDashboard.Mes => (new DateTime(hoje.Year, hoje.Month, 1), hoje),
            _ => throw new ArgumentOutOfRangeException(nameof(periodo), periodo, "Período de dashboard inválido."),
        };
    }
}
