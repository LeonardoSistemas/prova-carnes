using Prova.Service.Dtos;

namespace Prova.Service.Services;

/// <summary>Leitura de Estado/Cidade, somente para alimentar combobox do frontend.</summary>
public interface IEstadoService
{
    Task<IReadOnlyList<EstadoComCidadesDto>> ObterEstadosComCidadesAsync();
}
