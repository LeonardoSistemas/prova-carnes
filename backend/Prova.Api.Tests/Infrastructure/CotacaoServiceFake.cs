using Prova.Model.Enums;
using Prova.Service.Cotacao;
using Prova.Service.Exceptions;

namespace Prova.Api.Tests.Infrastructure;

/// <summary>
/// Dublê de teste de <see cref="ICotacaoService"/> — evita qualquer chamada
/// HTTP real à AwesomeAPI durante os testes de integração de
/// <c>PedidoController</c>. Duas variantes: sempre bem-sucedida (cotações
/// fixas) ou sempre lançando <see cref="CotacaoIndisponivelException"/>,
/// simulando a API externa indisponível (cenário do critério de 422 da T19).
/// </summary>
public class CotacaoServiceFake : ICotacaoService
{
    private readonly IReadOnlyDictionary<Moeda, decimal>? _cotacoes;
    private readonly Exception? _excecao;

    private CotacaoServiceFake(IReadOnlyDictionary<Moeda, decimal>? cotacoes, Exception? excecao)
    {
        _cotacoes = cotacoes;
        _excecao = excecao;
    }

    public static CotacaoServiceFake ComSucesso() => new(
        new Dictionary<Moeda, decimal>
        {
            [Moeda.BRL] = 1m,
            [Moeda.USD] = 5m,
            [Moeda.EUR] = 6m,
        },
        excecao: null);

    public static CotacaoServiceFake Indisponivel() => new(
        cotacoes: null,
        new CotacaoIndisponivelException("Cotação indisponível para teste (AwesomeAPI simulada como fora do ar)."));

    public Task<IReadOnlyDictionary<Moeda, decimal>> ObterCotacoesAsync(IEnumerable<Moeda> moedas)
    {
        if (_excecao is not null)
        {
            throw _excecao;
        }

        return Task.FromResult(_cotacoes!);
    }
}
