using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Prova.Data.Context;
using Prova.Model.Entities;

namespace Prova.Data.Repositories;

/// <summary>
/// Implementação genérica de <see cref="IRepository{T}"/> usando
/// <see cref="AppDbContext"/>. Não persiste nada sozinha — quem efetivamente
/// grava no banco é <see cref="IUnitOfWork.SaveChangesAsync"/>, mantendo a
/// unidade de trabalho como fronteira transacional.
/// </summary>
public class Repository<T> : IRepository<T> where T : EntidadeBase
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
    {
        var query = ApplyIncludes(_dbSet.AsNoTracking(), includes);
        return await query.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
    {
        var query = ApplyIncludes(_dbSet, includes);
        return await query.FirstOrDefaultAsync(entity => entity.Id == id);
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Aplica, em cadeia, as expressões de navegação recebidas como
    /// <c>Include</c> na query. Mantém <see cref="GetByIdAsync(int, Expression{Func{T, object}}[])"/>
    /// e <see cref="GetAllAsync(Expression{Func{T, object}}[])"/> enxutos e evita
    /// duplicar essa lógica nos dois métodos.
    /// </summary>
    private static IQueryable<T> ApplyIncludes(IQueryable<T> query, Expression<Func<T, object>>[] includes)
    {
        return includes.Aggregate(query, (current, include) => current.Include(include));
    }
}
