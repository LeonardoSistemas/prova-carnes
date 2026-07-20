using Prova.Data.Repositories;
using Prova.Model.Entities;
using Prova.Service.Dtos;

namespace Prova.Service.Services;

/// <summary>
/// Service somente leitura (T15): retorna todos os Estados já com suas
/// Cidades aninhadas, em uma única query com eager loading (evita N+1 —
/// combobox de Estado→Cidade em cascata do frontend precisa da lista
/// completa de uma vez).
/// </summary>
public class EstadoService : IEstadoService
{
    private readonly IUnitOfWork _unitOfWork;

    public EstadoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<EstadoComCidadesDto>> ObterEstadosComCidadesAsync()
    {
        var estados = await _unitOfWork.Repository<Estado>().GetAllAsync(e => e.Cidades);

        return estados
            .Select(estado => new EstadoComCidadesDto(
                estado.Id,
                estado.Nome,
                estado.Uf,
                estado.Cidades
                    .Select(cidade => new CidadeDto(cidade.Id, cidade.Nome, cidade.EstadoId))
                    .ToList()))
            .ToList();
    }
}
