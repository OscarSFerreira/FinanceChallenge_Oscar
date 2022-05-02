using DesafioFinanceiro_Oscar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DesafioFinanceiro_Oscar.Infrastructure
{
    public class DataContext : DbContext
    {

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<BankRecord> BankRecords { get; set; }
        public DbSet<BuyRequest> BuyRequests { get; set; }
        public DbSet<ProductRequest> ProductRequests { get; set; }
        public DbSet<Document> Documents { get; set; }


    }
}
