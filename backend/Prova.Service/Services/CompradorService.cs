using FluentValidation;
using Prova.Data.Repositories;
using Prova.Model.Entities;
using Prova.Service.Dtos;
using Prova.Service.Exceptions;

namespace Prova.Service.Services;

/// <summary>
/// Regras de negócio de Comprador: CRUD + bloqueio de delete quando existir
/// Pedido vinculado + validação de existência de Cidade.
///
/// Nota sobre "validação de Cidade/Estado existente" (T12): o Model de
/// <see cref="Comprador"/> só possui <c>CidadeId</c> (não há campo de
/// EstadoId separado no Comprador — Estado é alcançado indiretamente via
/// <c>Cidade.EstadoId</c>). Portanto a validação possível e suficiente aqui é
/// "a Cidade informada existe"; não há combinação Cidade/Estado a
/// contra-validar porque não existe um EstadoId próprio do Comprador que
/// pudesse divergir do Estado da Cidade.
/// </summary>
public class CompradorService : ICompradorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CompradorDto> _validator;

    public CompradorService(IUnitOfWork unitOfWork, IValidator<CompradorDto> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<IReadOnlyList<CompradorResponseDto>> ObterTodosAsync()
    {
        var compradores = await _unitOfWork.Repository<Comprador>().GetAllAsync();
        return compradores.Select(MapParaResponse).ToList();
    }

    public async Task<CompradorResponseDto?> ObterPorIdAsync(int id)
    {
        var comprador = await _unitOfWork.Repository<Comprador>().GetByIdAsync(id);
        return comprador is null ? null : MapParaResponse(comprador);
    }

    public async Task<CompradorResponseDto> CriarAsync(CompradorDto dto)
    {
        await _validator.ValidateAndThrowAsync(dto);
        await GarantirCidadeExisteAsync(dto.CidadeId);

        var comprador = new Comprador
        {
            Nome = dto.Nome,
            Documento = dto.Documento,
            CidadeId = dto.CidadeId,
        };

        await _unitOfWork.Repository<Comprador>().AddAsync(comprador);
        await _unitOfWork.SaveChangesAsync();

        return MapParaResponse(comprador);
    }

    public async Task AtualizarAsync(int id, CompradorDto dto)
    {
        await _validator.ValidateAndThrowAsync(dto);

        var comprador = await _unitOfWork.Repository<Comprador>().GetByIdAsync(id)
            ?? throw new EntidadeNaoEncontradaException($"Comprador {id} não encontrado.");

        await GarantirCidadeExisteAsync(dto.CidadeId);

        comprador.Nome = dto.Nome;
        comprador.Documento = dto.Documento;
        comprador.CidadeId = dto.CidadeId;

        _unitOfWork.Repository<Comprador>().Update(comprador);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var comprador = await _unitOfWork.Repository<Comprador>().GetByIdAsync(id, c => c.Pedidos)
            ?? throw new EntidadeNaoEncontradaException($"Comprador {id} não encontrado.");

        if (comprador.Pedidos.Count > 0)
        {
            throw new EntidadeVinculadaException(
                "Não é possível excluir o comprador pois existe pedido vinculado a ele.");
        }

        _unitOfWork.Repository<Comprador>().Remove(comprador);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task GarantirCidadeExisteAsync(int cidadeId)
    {
        var cidade = await _unitOfWork.Repository<Cidade>().GetByIdAsync(cidadeId);
        if (cidade is null)
        {
            throw new EntidadeNaoEncontradaException($"Cidade {cidadeId} não encontrada.");
        }
    }

    private static CompradorResponseDto MapParaResponse(Comprador comprador)
    {
        return new CompradorResponseDto(comprador.Id, comprador.Nome, comprador.Documento, comprador.CidadeId);
    }
}
