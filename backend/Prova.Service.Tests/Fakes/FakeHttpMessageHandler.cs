namespace Prova.Service.Tests.Fakes;

/// <summary>
/// Handler de teste que substitui o handler real de rede do
/// <see cref="HttpClient"/>. Permite simular sucesso (JSON canned) ou falha
/// (timeout/erro HTTP) sem bater na AwesomeAPI de verdade — o
/// <see cref="AwesomeApiCotacaoService" /> continua recebendo um
/// <see cref="HttpClient"/> real (via construtor), só que apontando para
/// este handler fake, exatamente como o `IHttpClientFactory` faria em tempo
/// de produção.
/// </summary>
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responder;

    public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
    {
        _responder = responder;
    }

    /// <summary>Atalho para respostas síncronas simples.</summary>
    public static FakeHttpMessageHandler ComResposta(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        return new FakeHttpMessageHandler((request, _) => Task.FromResult(responder(request)));
    }

    /// <summary>Simula timeout: nunca completa antes do cancellation token expirar.</summary>
    public static FakeHttpMessageHandler ComTimeout()
    {
        return new FakeHttpMessageHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("Não deveria chegar aqui — o cancellation token deveria ter disparado antes.");
        });
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _responder(request, cancellationToken);
    }
}
