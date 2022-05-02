using DesafioFinanceiro_Oscar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DesafioFinanceiro_Oscar.Infrastructure.Repository.DocumentRepository
{
    public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
    {
        private readonly DataContext _context;

        public DocumentRepository(DataContext context) : base(context)
        {

            _context = context;

        }

        public async Task<Document> GetByIdAsync(Guid id)
        {
            return await _context.Set<Document>()
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
