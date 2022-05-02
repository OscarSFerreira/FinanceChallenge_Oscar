using DesafioFinanceiro_Oscar.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace DesafioFinanceiro_Oscar.Infrastructure.Repository.DocumentRepository
{
    public interface IDocumentRepository : IGenericRepository<Document>
    {

        Task<Document> GetByIdAsync(Guid id);

    }
}
