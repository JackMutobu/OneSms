using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using OneSms.Web.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OneSms.Web.Server.Data.Repositories
{
    public interface IBaseRepository<T>
    {
        Task AddAsync(T entity);
        Task DeleteAsync(T entity);
        Task<TResult> Get<TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> selector);
        Task<TResult> Get<TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> selector, Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null);
        IEnumerable<T> GetAll();
        Task<List<TResult>> GetAll<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> filter = null);
        Task<List<TResult>> GetAll<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null);
        Task<List<TResult>> GetAll<TResult>(Expression<Func<T, TResult>> selector, int take, int skipe = 0, Expression<Func<T, bool>> filter = null);
        Task<List<TResult>> GetAll<TResult>(Expression<Func<T, TResult>> selector, int take, int skipe = 0, Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null);
        T GetById(int id, Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null);
        Task<T> GetByIdAsync(int id);
        Task UpdateAsync(T entity);
    }

    public class BaseRepository<T> : IBaseRepository<T> where T:BaseModel
    {
        protected OneSmsDbContext _dbContext;
        public BaseRepository(OneSmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(T entity)
        {
            entity.CreatedOn = DateTime.UtcNow;
            _dbContext.Add(entity);

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }
        public IEnumerable<T> GetAll()
        {
            return _dbContext.Set<T>().AsNoTracking();
        }
        public T GetById(int id, Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null!)
        {
            IQueryable<T> queryable = _dbContext.Set<T>().AsNoTracking();

            if (includes != null)
            {
                queryable = includes(queryable);
            }

            return queryable.FirstOrDefault(x => x.Id == id);
        }

        public async Task<TResult> Get<TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> selector)
        {
            var result = await _dbContext.Set<T>()
                .Where(filter)
                .Select(selector)
                .SingleOrDefaultAsync();
            return result;
        }
        public async Task<TResult> Get<TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> selector, Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null!)
        {
            IQueryable<T> queryable = _dbContext.Set<T>();
            if (includes != null)
            {
                queryable = includes(queryable);
            }
            return await queryable
                .Where(filter)
                .Select(selector)
                .SingleOrDefaultAsync();
        }

        public async Task<List<TResult>> GetAll<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> filter = null!)
        {
            IQueryable<T> queryable = _dbContext.Set<T>().AsNoTracking();
            if (filter != null)
            {
                queryable = queryable.Where(filter);
            }
            var result = await queryable
                .Select(selector)
                .ToListAsync();
            return result;
        }
        public async Task<List<TResult>> GetAll<TResult>(Expression<Func<T, TResult>> selector, int take, int skipe = 0, Expression<Func<T, bool>> filter = null!)
        {
            IQueryable<T> queryable = _dbContext.Set<T>().AsNoTracking();
            if (filter != null)
            {
                queryable = queryable.Where(filter);
            }

            var result = await queryable
                .OrderByDescending(x => x.CreatedOn)
                .Select(selector)
                .Skip(skipe)
                .Take(take)
                .ToListAsync();
            return result;
        }
        public async Task<List<TResult>> GetAll<TResult>(Expression<Func<T, TResult>> selector, int take, int skipe = 0, Expression<Func<T, bool>> filter = null!, Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null!)
        {
            IQueryable<T> queryable = _dbContext.Set<T>().AsNoTracking();
            if (includes != null)
            {
                queryable = includes(queryable);
            }
            if (filter != null)
            {
                queryable = queryable.Where(filter);
            }

            var result = await queryable
                .OrderByDescending(x => x.CreatedOn)
                .Select(selector)
                .Skip(skipe)
                .Take(take)
                .ToListAsync();
            return result;
        }
        public async Task<List<TResult>> GetAll<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> filter = null!, Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null!)
        {
            IQueryable<T> queryable = _dbContext.Set<T>().AsNoTracking();
            if (includes != null)
            {
                queryable = includes(queryable);
            }
            if (filter != null)
            {
                queryable = queryable.Where(filter);
            }

            var result = await queryable
                .Select(selector)
                .ToListAsync();
            return result;
        }
    }
}
