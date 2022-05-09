using DesafioFinanceiro_Oscar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioFinanceiro_Oscar.Infrastructure.Repository.BankRecordRepository
{
    public class BankRecordRepository : GenericRepository<BankRecord>, IBankRecordRepository
    {

        private readonly DataContext _context;

        public BankRecordRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<BankRecord> GetAll(PageParameter pageParameter)
        {
            return _context.Set<BankRecord>().OrderBy(x => x.Id)
                .Skip((pageParameter.PageNumber - 1) * pageParameter.PageSize)
                .Take(pageParameter.PageSize).AsNoTracking();
        }

        public async Task<BankRecord> GetByIdAsync(Guid id)
        {
            return await _context.Set<BankRecord>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

    }
}
