using System;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioFinanceiro_Oscar.Infrastructure.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        IQueryable<T> GetAll();

        //Task<T> GetByIdAsync(Guid id);

        //Task<bool> ExistAsync(Guid id);
    }
}
