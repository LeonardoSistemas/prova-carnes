namespace Prova.Service.Dtos;

/// <summary>
/// DTO de saída de Estado com suas Cidades aninhadas — shape escolhido para
/// alimentar diretamente um combobox de Estado→Cidade em cascata no
/// frontend (T15) sem precisar de uma segunda chamada para cada Estado.
/// </summary>
public record EstadoComCidadesDto(int Id, string Nome, string Uf, IReadOnlyList<CidadeDto> Cidades);
