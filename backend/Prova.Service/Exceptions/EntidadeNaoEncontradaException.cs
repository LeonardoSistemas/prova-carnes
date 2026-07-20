namespace Prova.Service.Exceptions;

/// <summary>
/// Lançada quando uma referência informada (ex.: CompradorId, CarneId,
/// CidadeId) não existe no banco. Traduzível pela Controller (futura, T16-T19)
/// para HTTP 404 (quando é o recurso principal da rota) ou 422/400 (quando é
/// uma referência dentro do corpo da requisição) — a decisão de status code
/// específico fica a cargo de quem consome a exceção.
/// </summary>
public class EntidadeNaoEncontradaException : Exception
{
    public EntidadeNaoEncontradaException(string mensagem) : base(mensagem)
    {
    }
}
