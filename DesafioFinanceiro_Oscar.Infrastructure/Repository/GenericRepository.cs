using DesafioFinanceiro_Oscar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioFinanceiro_Oscar.Infrastructure.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class/*, IEntity*/
    {
        private readonly DataContext _context;

        public GenericRepository(DataContext context)
        {
            _context = context;
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<T> GetAll()
        {
            return _context.Set<T>().AsNoTracking();
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }
        private async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
        //public async Task<bool> ExistAsync(Guid id)
        //{
        //    return await _context.Set<T>().AnyAsync(x => x.Id == id);
        //}
        //public async Task<T> GetByIdAsync(Guid id)
        //{
        //    return await _context.Set<T>()
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(e => e.Id == id);
        //}
    }
}
