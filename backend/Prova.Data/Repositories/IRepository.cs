using System.Linq.Expressions;
using Prova.Model.Entities;

namespace Prova.Data.Repositories;

/// <summary>
/// Contrato genérico de persistência para entidades de domínio. Conhece
/// apenas operações de acesso a dados — nenhuma regra de negócio, nenhum
/// conhecimento de DTO (isso é responsabilidade da camada Service).
///
/// Update/Remove são síncronos porque o EF Core apenas muda o estado da
/// entidade no change tracker; a operação de I/O de fato só acontece em
/// <see cref="IUnitOfWork.SaveChangesAsync"/>.
///
/// As sobrecargas com <c>includes</c> foram adicionadas para permitir eager
/// loading de navegação (ex.: checar <c>Carne.PedidoItens</c> antes de
/// bloquear delete, ou carregar <c>Pedido.Itens</c> para comparação na
/// edição) sem vazar <c>IQueryable</c>/<c>DbSet</c> para a camada Service —
/// o chamador só enxerga <c>Expression&lt;Func&lt;T, object&gt;&gt;</c>,
/// nunca detalhe de EF Core (ver decisão registrada na revisão da camada
/// Service). As assinaturas originais (sem includes) permanecem intactas
/// para não quebrar nenhum consumidor existente.
/// </summary>
/// <typeparam name="T">Tipo de entidade, restrito a <see cref="EntidadeBase"/>.</typeparam>
public interface IRepository<T> where T : EntidadeBase
{
    Task<IReadOnlyList<T>> GetAllAsync();

    Task<IReadOnlyList<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);

    Task<T?> GetByIdAsync(int id);

    Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);

    Task AddAsync(T entity);

    void Update(T entity);

    void Remove(T entity);
}
