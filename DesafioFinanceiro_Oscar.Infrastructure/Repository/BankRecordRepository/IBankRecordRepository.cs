using DesafioFinanceiro_Oscar.Domain.Entities;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DesafioFinanceiro_Oscar.Infrastructure.Repository.BankRecordRepository
{
    public interface IBankRecordRepository : IGenericRepository<BankRecord>
    {

        Task<BankRecord> GetByIdAsync(Guid id);

        Task<HttpResponseMessage> CreateBankRecord(Origin origin, Guid originId, string description, DesafioFinanceiro_Oscar.Domain.Entities.Type type, decimal amount);

    }
}
