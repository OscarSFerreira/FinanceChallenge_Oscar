using System.Threading.Tasks;

namespace DesafioFinanceiro_Oscar.Infrastructure
{
    public class SeedDb
    {
        public readonly DataContext _dataContext;

        public SeedDb(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task SeedDbAsync()
        {
            await _dataContext.Database.EnsureCreatedAsync();
        }

    }
}
