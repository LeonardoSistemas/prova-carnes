namespace Prova.Service.Dtos;

/// <summary>DTO de saída de Pedido, com o valor total já calculado em Real.</summary>
public record PedidoResponseDto(
    int Id,
    DateTime Data,
    int CompradorId,
    IReadOnlyList<PedidoItemResponseDto> Itens,
    decimal ValorTotalEmReal);
