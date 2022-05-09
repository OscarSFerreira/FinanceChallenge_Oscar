using DesafioFinanceiro_Oscar.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DesafioFinanceiro_Oscar.Infrastructure.Repository.BankRecordRepository
{
    public interface IBankRecordRepository : IGenericRepository<BankRecord>
    {

        Task<BankRecord> GetByIdAsync(Guid id);

        IEnumerable<BankRecord> GetAll(PageParameter pageParameter);

    }
}
