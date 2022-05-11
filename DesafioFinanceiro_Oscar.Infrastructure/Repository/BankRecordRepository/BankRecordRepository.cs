using DesafioFinanceiro_Oscar.Domain.DTO_s;
using DesafioFinanceiro_Oscar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Net.Http.Json;
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

        public async Task<BankRecord> GetByIdAsync(Guid id)
        {
            return await _context.Set<BankRecord>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<HttpResponseMessage> CreateBankRecord(Origin origin, Guid originId, string description, Domain.Entities.Type type, decimal amount)
        {
            var client = new HttpClient();
            string ApiUrl = "https://localhost:44359/api/BankRequest";

            var bankRecord = new BankRecordDTO()
            {
                Origin = origin,
                OriginId = originId,
                Description = description,
                Type = type,
                Amount = amount
            };

            var response = await client.PostAsJsonAsync(ApiUrl, bankRecord);
            return response;
        }

    }
}
