namespace Prova.Service.Dtos;

/// <summary>
/// DTO de entrada para criação e edição de Pedido. A lista de <c>Itens</c>
/// na edição substitui integralmente a lista anterior (ver
/// <see cref="Prova.Service.Services.IPedidoService.AtualizarAsync"/>) — mais
/// simples do que um "patch" item a item e suficiente para o escopo desta
/// prova (não há edição parcial de item isolado no PRD).
/// </summary>
public record PedidoDto(DateTime Data, int CompradorId, IReadOnlyList<PedidoItemDto> Itens);
