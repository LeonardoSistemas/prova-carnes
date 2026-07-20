namespace Prova.Model.Entities;

/// <summary>
/// Entidade de apoio (somente leitura para o usuário final). Relaciona-se
/// 1:N com Cidade. Dados semeados via script SQL (ver T07/T09).
/// </summary>
public class Estado : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;

    /// <summary>Sigla de duas letras (ex.: SP, MG).</summary>
    public string Uf { get; set; } = string.Empty;

    public ICollection<Cidade> Cidades { get; set; } = new List<Cidade>();
}
