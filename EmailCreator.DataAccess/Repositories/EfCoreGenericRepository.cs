using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using EmailCreator.DataAccess.Contexts;

namespace EmailCreator.DataAccess.Repositories;

public sealed class EfCoreGenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : class
{
    private readonly EmailCreatorDbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;

    public EfCoreGenericRepository(EmailCreatorDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<TEntity>();
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(predicate, orderBy, asNoTracking);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(predicate, orderBy, asNoTracking);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<TEntity> BuildQuery(
        Expression<Func<TEntity, bool>>? predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
        bool asNoTracking)
    {
        IQueryable<TEntity> query = _dbSet;

        // Okuma işlemlerinde tracking kapatılır; EF Core entity'yi izlemediği için listeleme/search daha hafif çalışır.
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        // Generic repository aynı filtreleme mekanizmasını tüm entity'ler için tekrar kullanılabilir hale getirir.
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        // Sıralama repository'ye parametre olarak verilir; böylece servis iş kuralına göre sıralamayı belirler.
        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        return query;
    }
}
