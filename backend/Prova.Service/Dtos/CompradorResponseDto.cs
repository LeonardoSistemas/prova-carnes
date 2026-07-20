namespace Prova.Service.Dtos;

/// <summary>
/// DTO de saída de Comprador. Carrega apenas <c>CidadeId</c> (não o nome da
/// Cidade/Estado) — enriquecimento de exibição (nome da cidade/estado) é
/// responsabilidade do frontend, que já tem a lista completa de
/// Estado→Cidade carregada via T15/T18 para popular o combobox; evita a
/// Service de Comprador fazer join/eager loading só para exibição e manter
/// o DTO enxuto (mesma decisão aplicada a Pedido/PedidoItem, ver T13/T14).
/// </summary>
public record CompradorResponseDto(int Id, string Nome, string Documento, int CidadeId);
