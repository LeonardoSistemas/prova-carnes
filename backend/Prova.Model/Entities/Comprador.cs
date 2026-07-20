namespace Prova.Model.Entities;

/// <summary>
/// Comprador de carnes. Não pode ser excluído se existir algum Pedido
/// vinculado a ele (regra aplicada na camada Service, mesma lógica de
/// integridade de Carne, por consistência).
/// </summary>
public class Comprador : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// CPF ou CNPJ. Texto livre, sem validação de dígito verificador
    /// (decisão de escopo documentada no PRD).
    /// </summary>
    public string Documento { get; set; } = string.Empty;

    public int CidadeId { get; set; }

    public Cidade? Cidade { get; set; }

    /// <summary>
    /// Navegação inversa usada pela camada Service para checar vínculo com
    /// Pedido antes de permitir exclusão (regra de bloqueio de delete).
    /// </summary>
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
