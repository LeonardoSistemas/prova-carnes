namespace Prova.Service.Dtos;

/// <summary>
/// Agrega os dois rankings de topo do Dashboard (T60) para o mesmo período,
/// evitando duas chamadas de API separadas do frontend para uma única tela.
/// </summary>
/// <param name="TopCarnes">Top 5 carnes por valor em Real, desc; empate desempatado por <c>Descricao</c> asc.</param>
/// <param name="TopCompradores">Top 5 compradores por valor em Real, desc; empate desempatado por <c>Nome</c> asc.</param>
public record DashboardTopDto(
    IReadOnlyList<TopCarneDto> TopCarnes,
    IReadOnlyList<TopCompradorDto> TopCompradores);
