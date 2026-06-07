using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

//ST10445500 - PROG7311 - GLMS POE
//Repository

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T?> FindAsync(params object[] keyValues);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task SaveChangesAsync();
    }

    //..............................................................................//
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        //..............................................................................//

        public virtual async Task<List<T>> GetAllAsync()
        {
            return await Query().ToListAsync();
        }

        //..............................................................................//

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await FindAsync(id);
        }

        //..............................................................................//

        public virtual async Task<T?> FindAsync(params object[] keyValues)
        {
            return await _dbSet.FindAsync(keyValues);
        }

        //..............................................................................//

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await Query().FirstOrDefaultAsync(predicate);
        }

        //..............................................................................//

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await Query().AnyAsync(predicate);
        }

        //..............................................................................//

        public virtual async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
        }

        //..............................................................................//

        public virtual void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
        }

        //..............................................................................//

        public virtual void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
        }

        //..............................................................................//

        public virtual async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        //..............................................................................//

        protected IQueryable<T> Query()
        {
            return _dbSet.AsNoTracking();
        }

        //..............................................................................//

        protected IQueryable<T> TrackedQuery()
        {
            return _dbSet;
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//
