using DesafioFinanceiro_Oscar.Domain.Entities;
using DesafioFinanceiro_Oscar.Domain.Entities.Messages;
using System.Collections.Generic;
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

        IEnumerable<T> GetAllWithPaging(PageParameter page);

        ErrorMessage<T> BadRequestMessage(T entity, string msg);

        ErrorMessage<T> NotFoundMessage(T entity);
    }
}
