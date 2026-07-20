using Prova.Model.Entities;

namespace Prova.Data.Repositories;

/// <summary>
/// Unidade de trabalho: fronteira transacional única para persistir
/// alterações feitas através de um ou mais <see cref="IRepository{T}"/> na
/// mesma requisição/operação de negócio.
///
/// Expõe um método genérico <see cref="Repository{T}"/> em vez de uma
/// propriedade por entidade (ex.: <c>Carnes</c>, <c>Compradores</c>) para
/// não precisar crescer a interface a cada nova entidade cadastrada — só o
/// projeto Model muda, o contrato de UoW permanece estável (aberto para
/// extensão de entidades, fechado para modificação da interface).
/// </summary>
public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : EntidadeBase;

    Task<int> SaveChangesAsync();
}
