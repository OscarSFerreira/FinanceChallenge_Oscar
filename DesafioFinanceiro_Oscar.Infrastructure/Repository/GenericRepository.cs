using DesafioFinanceiro_Oscar.Domain.Entities;
using DesafioFinanceiro_Oscar.Domain.Entities.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DesafioFinanceiro_Oscar.Infrastructure.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class/*, IEntity*/
    {
        private readonly DataContext _context;
        public List<string> errorList = new List<string>();
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
        public IEnumerable<T> GetAllWithPaging(PageParameter page)        //testar no geral
        {
            return _context.Set<T>()
                .Skip((page.PageNumber - 1) * page.PageSize)
                .Take(page.PageSize)
                .AsNoTracking();
        }

        public ErrorMessage<T> BadRequestMessage(T entity, string msg)
        {
            errorList.Add(msg);
            var error = new ErrorMessage<T>(HttpStatusCode.BadRequest.GetHashCode().ToString(), errorList, entity);
            return error;
        }

        public ErrorMessage<T> NotFoundMessage(T entity)
        {
            errorList.Add("This database does not contain the data you requested!");
            var error = new ErrorMessage<T>(HttpStatusCode.NotFound.GetHashCode().ToString(), errorList, entity);
            return error;
        }

    }
}
