using Prova.Model.Enums;

namespace Prova.Service.Dtos;

/// <summary>
/// DTO de entrada para criação e edição de Carne. Usa o mesmo formato para
/// as duas operações (Descricao/Origem são os únicos campos editáveis em
/// ambos os casos) — evita duplicar uma classe idêntica só para diferenciar
/// Create de Update; o Id nunca é aceito aqui, é sempre passado como
/// parâmetro separado do método de Service (ex.: <c>AtualizarAsync(int id, CarneDto dto)</c>),
/// nunca lido do corpo do DTO.
/// </summary>
public record CarneDto(string Descricao, OrigemCarne Origem);
