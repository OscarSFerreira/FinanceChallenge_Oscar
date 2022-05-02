using DesafioFinanceiro_Oscar.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace DesafioFinanceiro_Oscar.Infrastructure.Repository.BuyRequestRepository
{
    public interface IBuyRequestRepository : IGenericRepository<BuyRequest>
    {
        
        Task<BuyRequest> GetByIdAsync(Guid id);

        Task<BuyRequest> GetByClientIdAsync(Guid clientId);

    }
}
