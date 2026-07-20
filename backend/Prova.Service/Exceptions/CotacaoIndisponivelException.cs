namespace Prova.Service.Exceptions;

/// <summary>
/// Lançada quando não é possível obter a cotação de uma moeda estrangeira
/// junto à AwesomeAPI (timeout, erro HTTP, resposta em formato inesperado).
/// Encapsula qualquer exceção técnica (HttpRequestException,
/// TaskCanceledException, JsonException) para que ela nunca escape "crua"
/// para quem chama o serviço: se a API externa estiver indisponível, o
/// pedido não é criado e retorna erro claro (422). O mapeamento para
/// HTTP 422 é responsabilidade da Controller
/// (T19); aqui garantimos apenas que a causa raiz fique disponível para log
/// (via <see cref="Exception.InnerException"/>) sem vazar para o cliente.
/// </summary>
public class CotacaoIndisponivelException : Exception
{
    public CotacaoIndisponivelException(string mensagem, Exception? causaRaiz = null)
        : base(mensagem, causaRaiz)
    {
    }
}
