using Prova.Data.Context;
using Prova.Model.Entities;

namespace Prova.Data.Repositories;

/// <summary>
/// Implementação de <see cref="IUnitOfWork"/> sobre um único
/// <see cref="AppDbContext"/> por requisição (escopo de vida gerenciado
/// pela DI da Api, T20). Os repositórios genéricos são criados sob demanda
/// e reaproveitados (cache por tipo) para não instanciar mais de um
/// <see cref="Repository{T}"/> para a mesma entidade dentro da mesma unidade
/// de trabalho.
/// </summary>
public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<T> Repository<T>() where T : EntidadeBase
    {
        var entityType = typeof(T);

        if (!_repositories.TryGetValue(entityType, out var repository))
        {
            repository = new Repository<T>(_context);
            _repositories[entityType] = repository;
        }

        return (IRepository<T>)repository;
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
