using FluentValidation;
using Prova.Data.Repositories;
using Prova.Model.Entities;
using Prova.Service.Dtos;
using Prova.Service.Exceptions;

namespace Prova.Service.Services;

/// <summary>
/// Regras de negócio de Carne: CRUD + bloqueio de delete quando existir
/// PedidoItem vinculado (ver <see cref="Carne.PedidoItens"/>).
/// </summary>
public class CarneService : ICarneService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CarneDto> _validator;

    public CarneService(IUnitOfWork unitOfWork, IValidator<CarneDto> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<IReadOnlyList<CarneResponseDto>> ObterTodasAsync()
    {
        var carnes = await _unitOfWork.Repository<Carne>().GetAllAsync();
        return carnes.Select(MapParaResponse).ToList();
    }

    public async Task<CarneResponseDto?> ObterPorIdAsync(int id)
    {
        var carne = await _unitOfWork.Repository<Carne>().GetByIdAsync(id);
        return carne is null ? null : MapParaResponse(carne);
    }

    public async Task<CarneResponseDto> CriarAsync(CarneDto dto)
    {
        await _validator.ValidateAndThrowAsync(dto);

        var carne = new Carne
        {
            Descricao = dto.Descricao,
            Origem = dto.Origem,
        };

        await _unitOfWork.Repository<Carne>().AddAsync(carne);
        await _unitOfWork.SaveChangesAsync();

        return MapParaResponse(carne);
    }

    public async Task AtualizarAsync(int id, CarneDto dto)
    {
        await _validator.ValidateAndThrowAsync(dto);

        var carne = await _unitOfWork.Repository<Carne>().GetByIdAsync(id)
            ?? throw new EntidadeNaoEncontradaException($"Carne {id} não encontrada.");

        carne.Descricao = dto.Descricao;
        carne.Origem = dto.Origem;

        _unitOfWork.Repository<Carne>().Update(carne);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var carne = await _unitOfWork.Repository<Carne>().GetByIdAsync(id, c => c.PedidoItens)
            ?? throw new EntidadeNaoEncontradaException($"Carne {id} não encontrada.");

        if (carne.PedidoItens.Count > 0)
        {
            throw new EntidadeVinculadaException(
                "Não é possível excluir a carne pois existe pedido vinculado a ela.");
        }

        _unitOfWork.Repository<Carne>().Remove(carne);
        await _unitOfWork.SaveChangesAsync();
    }

    private static CarneResponseDto MapParaResponse(Carne carne)
    {
        return new CarneResponseDto(carne.Id, carne.Descricao, carne.Origem);
    }
}
