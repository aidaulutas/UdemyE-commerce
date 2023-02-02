using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UdemySiparis.Data.Repository.IRepository;

namespace UdemySiparis.Data.Repository
{
    public class Repository<T> : IRepository<T> where T : class // generic repository olacagı için hemen T bla bla yazalım 
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<T> _dbSet;
        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var item in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) // mesela productu getiriyor, donuyor sonra order getiriyor ve virgul ile onları ayırıyoe
                {
                    query = query.Include(item);
                }
            }
            return query.ToList();
        }



        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var item in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) // mesela productu getiriyor, donuyor sonra order getiriyor ve virgul ile onları ayırıyoe
                {
                    query = query.Include(item);
                }
            }
            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);                  // tek kayıt silme 
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);                         // birden fazla kayıt silme 
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
    }
}