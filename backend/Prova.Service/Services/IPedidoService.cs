using Prova.Service.Dtos;

namespace Prova.Service.Services;

public interface IPedidoService
{
    /// <summary>
    /// Lista pedidos, opcionalmente filtrados por <paramref name="compradorId"/>
    /// e/ou por intervalo de <c>Pedido.Data</c> (inclusive nos dois extremos).
    /// Filtros combinados aplicam AND. Sem filtros, retorna todos os pedidos
    /// (comportamento idêntico ao anterior à T35).
    /// </summary>
    Task<IReadOnlyList<PedidoResponseDto>> ObterTodosAsync(int? compradorId = null, DateTime? dataInicio = null, DateTime? dataFim = null);

    Task<PedidoResponseDto?> ObterPorIdAsync(int id);

    /// <exception cref="Exceptions.EntidadeNaoEncontradaException">
    /// Quando Comprador ou alguma Carne referenciada não existem.
    /// </exception>
    /// <exception cref="Exceptions.CotacaoIndisponivelException">
    /// Quando a cotação de alguma moeda estrangeira usada nos itens não pôde
    /// ser obtida — nenhuma escrita ocorre nesse caso.
    /// </exception>
    Task<PedidoResponseDto> CriarAsync(PedidoDto dto);

    /// <summary>
    /// Recotiza apenas quando a lista de itens (Carne/Preço/Moeda) muda em
    /// relação ao estado atual do pedido — edição só de Data/Comprador não
    /// dispara nova consulta de cotação.
    /// </summary>
    /// <exception cref="Exceptions.EntidadeNaoEncontradaException">
    /// Quando o Pedido, o Comprador ou alguma Carne referenciada não existem.
    /// </exception>
    /// <exception cref="Exceptions.CotacaoIndisponivelException">
    /// Quando a recotização é necessária e a cotação falha — nenhuma alteração
    /// é persistida nesse caso, o pedido permanece como estava.
    /// </exception>
    Task AtualizarAsync(int id, PedidoDto dto);

    Task ExcluirAsync(int id);
}
