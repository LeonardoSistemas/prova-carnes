namespace Prova.Service.Exceptions;

/// <summary>
/// Lançada ao tentar excluir uma entidade que possui vínculo obrigatório com
/// outra (Carne com PedidoItem, Comprador com Pedido). Não é
/// um bool de retorno "mágico": o chamador (Controller, na T16/T17) traduz
/// esta exceção para HTTP 409 Conflict.
/// </summary>
public class EntidadeVinculadaException : Exception
{
    public EntidadeVinculadaException(string mensagem) : base(mensagem)
    {
    }
}
