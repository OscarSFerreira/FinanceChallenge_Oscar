using DesafioFinanceiro_Oscar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DesafioFinanceiro_Oscar.Infrastructure.Repository.BuyRequestRepository
{
    public class BuyRequestRepository : GenericRepository<BuyRequest>, IBuyRequestRepository
    {
        private readonly DataContext _context;

        public BuyRequestRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<BuyRequest> GetByIdAsync(Guid id)
        {
            return await _context.Set<BuyRequest>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<BuyRequest> GetByClientIdAsync(Guid clientId)
        {
            return await _context.Set<BuyRequest>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ClientId == clientId);
        }

    }
}
