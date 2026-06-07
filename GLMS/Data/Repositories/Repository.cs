using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

//ST10445500 - PROG7311 - GLMS POE
//Repository

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Data.Repositories
{
    //this interface that lets you add, get, update and delete stuff from the database.
    //<typeparam name="T">The type of thing you want to work with.</typeparam>

    //..............................................................................//
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task SaveChangesAsync();
    }

    //..............................................................................//

    //this class handles all the basic database operations like creating, reading, updating, and deleting.
    // <typeparam name="T">The type of thing we're storing in the database.</typeparam>
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

        //gets everything from the table and returns it as a list.
        public virtual async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        //..............................................................................//

        //finds one thing in the table by its ID number
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        //..............................................................................//

        //checks if anything exists in the table that matches a certain condition.
        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AsNoTracking().AnyAsync(predicate);
        }

        //..............................................................................//

        //adds a new record to the table
        public virtual async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
        }

        //..............................................................................//

        //changes an existing record in the table
        public virtual void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
        }

        //..............................................................................//

        //removes a record from the table
        public virtual void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
        }

        //..............................................................................//

        //saves all the changes we made to the database.
        public virtual async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//