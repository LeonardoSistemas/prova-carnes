namespace Prova.Service.Dtos;

/// <summary>DTO de saída de Cidade (somente leitura, T15).</summary>
public record CidadeDto(int Id, string Nome, int EstadoId);
