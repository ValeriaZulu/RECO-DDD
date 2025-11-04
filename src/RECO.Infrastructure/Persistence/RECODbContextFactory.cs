using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RECO.Infrastructure.Persistence
{
    public class RECODbContextFactory : IDesignTimeDbContextFactory<RECODbContext>
    {
        public RECODbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RECODbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=reco;Username=postgres;Password=postgres");

            return new RECODbContext(optionsBuilder.Options);
        }
    }
}
