namespace Prova.Service.Dtos;

/// <summary>
/// DTO de entrada para criação e edição de Comprador (mesmo formato para as
/// duas operações, mesma justificativa de <see cref="CarneDto"/>). Só carrega
/// <c>CidadeId</c> porque é o único vínculo geográfico que
/// <see cref="Prova.Model.Entities.Comprador"/> possui no Model atual — não
/// há campo de Estado separado a validar (ver decisão documentada na Service
/// de Comprador).
/// </summary>
public record CompradorDto(string Nome, string Documento, int CidadeId);
