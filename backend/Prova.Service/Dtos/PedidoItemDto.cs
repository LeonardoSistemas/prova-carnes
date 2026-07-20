using Prova.Model.Enums;

namespace Prova.Service.Dtos;

/// <summary>
/// Item de entrada de um Pedido (usado dentro de <see cref="PedidoDto"/>,
/// tanto na criação quanto na edição — a lista de itens da edição sempre
/// substitui a lista anterior por completo, não há "patch" parcial de item).
/// </summary>
public record PedidoItemDto(int CarneId, decimal Preco, Moeda Moeda);
