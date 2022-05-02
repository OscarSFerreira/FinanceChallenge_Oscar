using DesafioFinanceiro_Oscar.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioFinanceiro_Oscar.Infrastructure.Repository.ProductRequestRepository
{
    public interface IProductRequestRepository : IGenericRepository<ProductRequest>
    {

        Task<ProductRequest> GetByIdAsync(Guid id);
        IQueryable<ProductRequest> GetAllByRequestId(Guid requestId);

    }
}
