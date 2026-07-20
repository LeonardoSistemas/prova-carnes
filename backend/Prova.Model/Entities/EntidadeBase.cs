namespace Prova.Model.Entities;

/// <summary>
/// Base para todas as entidades do domínio, evitando repetição da chave
/// primária. Mantém o projeto Model livre de qualquer dependência de EF
/// Core — é um POCO puro.
/// </summary>
public abstract class EntidadeBase
{
    public int Id { get; set; }
}
