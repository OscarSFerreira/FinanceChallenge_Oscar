using DesafioFinanceiro_Oscar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioFinanceiro_Oscar.Infrastructure.Repository.ProductRequestRepository
{
    public class ProductRequestRepository : GenericRepository<ProductRequest>, IProductRequestRepository
    {
        private readonly DataContext _context;

        public ProductRequestRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ProductRequest> GetByIdAsync(Guid id)
        {
            return await _context.Set<ProductRequest>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public IQueryable<ProductRequest> GetAllByRequestId(Guid requestId)
        {
            return _context.Set<ProductRequest>()
                .AsNoTracking()
                .Where(e => e.RequestId == requestId);
        }

    }
}
