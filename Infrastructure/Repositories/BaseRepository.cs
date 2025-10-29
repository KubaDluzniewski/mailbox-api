using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly MailboxDbContext Context;
    private readonly DbSet<TEntity> _dbSet;

    public BaseRepository(MailboxDbContext context)
    {
        Context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<List<TEntity>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task AddAsync(TEntity entity) => await _dbSet.AddAsync(entity);

    public void Update(TEntity entity) => _dbSet.Update(entity);

    public void Remove(TEntity entity) => _dbSet.Remove(entity);

    public async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public async Task SaveChangesAsync() => await Context.SaveChangesAsync();

    public Task<TEntity?> FindSingleAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.SingleOrDefaultAsync(predicate);
    }
}
