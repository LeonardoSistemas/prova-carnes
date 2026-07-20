using Prova.Service.Dtos;

namespace Prova.Service.Services;

public interface ICompradorService
{
    Task<IReadOnlyList<CompradorResponseDto>> ObterTodosAsync();

    Task<CompradorResponseDto?> ObterPorIdAsync(int id);

    /// <exception cref="Exceptions.EntidadeNaoEncontradaException">
    /// Quando <c>CidadeId</c> informado não existe.
    /// </exception>
    Task<CompradorResponseDto> CriarAsync(CompradorDto dto);

    /// <exception cref="Exceptions.EntidadeNaoEncontradaException">
    /// Quando o Comprador ou o <c>CidadeId</c> informado não existem.
    /// </exception>
    Task AtualizarAsync(int id, CompradorDto dto);

    /// <exception cref="Exceptions.EntidadeVinculadaException">
    /// Quando existe Pedido vinculado ao Comprador.
    /// </exception>
    Task ExcluirAsync(int id);
}
