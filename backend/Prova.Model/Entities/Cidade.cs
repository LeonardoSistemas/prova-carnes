namespace Prova.Model.Entities;

/// <summary>
/// Entidade de apoio (somente leitura para o usuário final), vinculada a um
/// Estado (N:1). Usada para popular o combobox de Cidade/Estado do
/// Comprador.
/// </summary>
public class Cidade : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;

    public int EstadoId { get; set; }

    public Estado? Estado { get; set; }
}
