using Prova.Service.Dtos;

namespace Prova.Service.Services;

public interface ICarneService
{
    Task<IReadOnlyList<CarneResponseDto>> ObterTodasAsync();

    Task<CarneResponseDto?> ObterPorIdAsync(int id);

    Task<CarneResponseDto> CriarAsync(CarneDto dto);

    Task AtualizarAsync(int id, CarneDto dto);

    /// <exception cref="Exceptions.EntidadeVinculadaException">
    /// Quando existe PedidoItem vinculado à Carne.
    /// </exception>
    Task ExcluirAsync(int id);
}
