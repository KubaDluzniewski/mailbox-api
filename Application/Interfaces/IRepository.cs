using System.Linq.Expressions;

namespace Application.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Pobiera encję po identyfikatorze
    /// </summary>
    Task<TEntity?> GetByIdAsync(int id);

    /// <summary>
    /// Pobiera wszystkie encje
    /// </summary>
    Task<List<TEntity>> GetAllAsync();

    /// <summary>
    /// Dodaje nową encję
    /// </summary>
    /// <param name="entity">Encja do dodania</param>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// Aktualizuje istniejącą encję
    /// </summary>
    /// <param name="entity">Encja do aktualizacji</param>
    void Update(TEntity entity);

    /// <summary>
    /// Usuwa encję
    /// </summary>
    /// <param name="entity">Encja do usunięcia</param>
    void Remove(TEntity entity);

    /// <summary>
    /// Znajduje encje spełniające warunek
    /// </summary>
    /// <param name="predicate">Wyrażenie warunkowe</param>
    Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Znajduje pojedynczą encję spełniającą warunek
    /// </summary>
    Task<TEntity?> FindSingleAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Zapisuje zmiany w bazie danych
    /// </summary>
    /// <returns></returns>
    Task SaveChangesAsync();
}
