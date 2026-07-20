using Prova.Model.Enums;

namespace Prova.Service.Dtos;

/// <summary>
/// DTO de saída de um item de Pedido. Carrega apenas <c>CarneId</c> (não a
/// descrição da Carne) — mesma decisão de <see cref="CompradorResponseDto"/>,
/// enriquecimento de exibição fica com o consumidor (frontend já tem a lista
/// de Carnes carregada via T16/T23).
/// </summary>
public record PedidoItemResponseDto(int Id, int CarneId, decimal Preco, Moeda Moeda, decimal CotacaoUsada, decimal ValorEmReal);
