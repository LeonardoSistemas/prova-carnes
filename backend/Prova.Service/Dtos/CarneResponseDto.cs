using Prova.Model.Enums;

namespace Prova.Service.Dtos;

/// <summary>DTO de saída de Carne — nunca expõe a entidade de Model.</summary>
public record CarneResponseDto(int Id, string Descricao, OrigemCarne Origem);
