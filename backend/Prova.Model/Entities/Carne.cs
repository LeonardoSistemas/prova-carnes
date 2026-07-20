using Prova.Model.Enums;

namespace Prova.Model.Entities;

/// <summary>
/// Carne cadastrada no sistema. Não pode ser excluída se existir algum
/// PedidoItem vinculado a ela (regra aplicada na camada Service).
/// </summary>
public class Carne : EntidadeBase
{
    public string Descricao { get; set; } = string.Empty;

    public OrigemCarne Origem { get; set; }

    /// <summary>
    /// Navegação inversa usada pela camada Service para checar vínculo com
    /// PedidoItem antes de permitir exclusão (regra de bloqueio de delete).
    /// </summary>
    public ICollection<PedidoItem> PedidoItens { get; set; } = new List<PedidoItem>();
}
