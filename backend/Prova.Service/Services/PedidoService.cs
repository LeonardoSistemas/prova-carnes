using FluentValidation;
using Prova.Data.Repositories;
using Prova.Model.Entities;
using Prova.Model.Enums;
using Prova.Service.Cotacao;
using Prova.Service.Dtos;
using Prova.Service.Exceptions;

namespace Prova.Service.Services;

/// <summary>
/// Regras de negócio de Pedido: criação (T13) com cotação congelada por
/// item, e edição (T14) com recotização condicional. Em ambos os fluxos, as
/// validações de existência (Comprador/Carne) e a busca de cotação
/// acontecem antes de qualquer chamada a <see cref="IUnitOfWork.SaveChangesAsync"/>
/// — se a cotação falhar, nada foi persistido/alterado até aquele ponto.
/// </summary>
public class PedidoService : IPedidoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<PedidoDto> _validator;
    private readonly ICotacaoService _cotacaoService;

    public PedidoService(IUnitOfWork unitOfWork, IValidator<PedidoDto> validator, ICotacaoService cotacaoService)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _cotacaoService = cotacaoService;
    }

    public async Task<IReadOnlyList<PedidoResponseDto>> ObterTodosAsync(int? compradorId = null, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var pedidos = await _unitOfWork.Repository<Pedido>().GetAllAsync(p => p.Itens);

        // Filtro aplicado em memória (não no repositório genérico, T35):
        // volume de dados desta prova é pequeno e isso evita acoplar
        // IRepository<T> a uma necessidade específica de Pedido.
        // compradorId e intervalo de data (Pedido.Data, inclusive nos dois
        // extremos) combinam com AND quando mais de um filtro é informado.
        IEnumerable<Pedido> filtrados = pedidos;

        if (compradorId is not null)
        {
            filtrados = filtrados.Where(p => p.CompradorId == compradorId.Value);
        }

        if (dataInicio is not null)
        {
            filtrados = filtrados.Where(p => p.Data.Date >= dataInicio.Value.Date);
        }

        if (dataFim is not null)
        {
            filtrados = filtrados.Where(p => p.Data.Date <= dataFim.Value.Date);
        }

        return filtrados.Select(MapParaResponse).ToList();
    }

    public async Task<PedidoResponseDto?> ObterPorIdAsync(int id)
    {
        var pedido = await _unitOfWork.Repository<Pedido>().GetByIdAsync(id, p => p.Itens);
        return pedido is null ? null : MapParaResponse(pedido);
    }

    public async Task<PedidoResponseDto> CriarAsync(PedidoDto dto)
    {
        await _validator.ValidateAndThrowAsync(dto);

        await GarantirCompradorExisteAsync(dto.CompradorId);
        await GarantirCarnesExistemAsync(dto.Itens.Select(i => i.CarneId));

        // Busca a cotação ANTES de criar/persistir qualquer coisa: se falhar
        // aqui, nenhuma escrita ocorreu (pedido não é criado parcialmente).
        var cotacoes = await _cotacaoService.ObterCotacoesAsync(dto.Itens.Select(i => i.Moeda));

        var pedido = new Pedido
        {
            Data = dto.Data,
            CompradorId = dto.CompradorId,
        };

        foreach (var itemDto in dto.Itens)
        {
            pedido.Itens.Add(CriarItem(itemDto, cotacoes));
        }

        await _unitOfWork.Repository<Pedido>().AddAsync(pedido);
        await _unitOfWork.SaveChangesAsync();

        return MapParaResponse(pedido);
    }

    public async Task AtualizarAsync(int id, PedidoDto dto)
    {
        await _validator.ValidateAndThrowAsync(dto);

        var pedido = await _unitOfWork.Repository<Pedido>().GetByIdAsync(id, p => p.Itens)
            ?? throw new EntidadeNaoEncontradaException($"Pedido {id} não encontrado.");

        await GarantirCompradorExisteAsync(dto.CompradorId);
        await GarantirCarnesExistemAsync(dto.Itens.Select(i => i.CarneId));

        IReadOnlyDictionary<Moeda, decimal>? cotacoes = null;
        if (ItensForamAlterados(pedido.Itens, dto.Itens))
        {
            // Só busca cotação nova quando Carne/Preço/Moeda de algum item
            // mudou. Se falhar aqui, o pedido ainda não foi tocado (nenhuma
            // propriedade foi alterada até este ponto) — estado anterior
            // preservado.
            cotacoes = await _cotacaoService.ObterCotacoesAsync(dto.Itens.Select(i => i.Moeda));
        }

        pedido.Data = dto.Data;
        pedido.CompradorId = dto.CompradorId;

        if (cotacoes is not null)
        {
            pedido.Itens.Clear();
            foreach (var itemDto in dto.Itens)
            {
                pedido.Itens.Add(CriarItem(itemDto, cotacoes));
            }
        }

        _unitOfWork.Repository<Pedido>().Update(pedido);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var pedido = await _unitOfWork.Repository<Pedido>().GetByIdAsync(id)
            ?? throw new EntidadeNaoEncontradaException($"Pedido {id} não encontrado.");

        // Não há regra de bloqueio de delete para Pedido nem conceito de
        // cancelamento separado de exclusão; os PedidoItens são removidos em
        // cascade pela FK configurada em PedidoItemConfiguration.
        _unitOfWork.Repository<Pedido>().Remove(pedido);
        await _unitOfWork.SaveChangesAsync();
    }

    private static PedidoItem CriarItem(PedidoItemDto itemDto, IReadOnlyDictionary<Moeda, decimal> cotacoes)
    {
        return new PedidoItem
        {
            CarneId = itemDto.CarneId,
            Preco = itemDto.Preco,
            Moeda = itemDto.Moeda,
            CotacaoUsada = cotacoes[itemDto.Moeda],
        };
    }

    private async Task GarantirCompradorExisteAsync(int compradorId)
    {
        var comprador = await _unitOfWork.Repository<Comprador>().GetByIdAsync(compradorId);
        if (comprador is null)
        {
            throw new EntidadeNaoEncontradaException($"Comprador {compradorId} não encontrado.");
        }
    }

    private async Task GarantirCarnesExistemAsync(IEnumerable<int> carneIds)
    {
        foreach (var carneId in carneIds.Distinct())
        {
            var carne = await _unitOfWork.Repository<Carne>().GetByIdAsync(carneId);
            if (carne is null)
            {
                throw new EntidadeNaoEncontradaException($"Carne {carneId} não encontrada.");
            }
        }
    }

    /// <summary>
    /// Compara o conjunto atual de itens (CarneId, Preço, Moeda) contra o
    /// conjunto novo, independente de ordem. Diferença em quantidade,
    /// carne, preço ou moeda de qualquer item conta como alteração.
    /// </summary>
    private static bool ItensForamAlterados(ICollection<PedidoItem> itensAtuais, IReadOnlyList<PedidoItemDto> itensNovos)
    {
        if (itensAtuais.Count != itensNovos.Count)
        {
            return true;
        }

        var atuais = itensAtuais
            .Select(i => (i.CarneId, i.Preco, i.Moeda))
            .OrderBy(t => t.CarneId).ThenBy(t => t.Preco).ThenBy(t => t.Moeda)
            .ToList();

        var novos = itensNovos
            .Select(i => (i.CarneId, i.Preco, i.Moeda))
            .OrderBy(t => t.CarneId).ThenBy(t => t.Preco).ThenBy(t => t.Moeda)
            .ToList();

        return !atuais.SequenceEqual(novos);
    }

    private static PedidoResponseDto MapParaResponse(Pedido pedido)
    {
        var itens = pedido.Itens
            .Select(i => new PedidoItemResponseDto(i.Id, i.CarneId, i.Preco, i.Moeda, i.CotacaoUsada, i.ValorEmReal))
            .ToList();

        return new PedidoResponseDto(pedido.Id, pedido.Data, pedido.CompradorId, itens, pedido.ValorTotalEmReal);
    }
}
