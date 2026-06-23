using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebServicos.Data
{
    /// <summary>
    /// Factory usada pelo EF Core Tools (dotnet ef migrations add) para instanciar
    /// o DbContext em tempo de design com PostgreSQL, gerando migrações compatíveis
    /// com o ambiente de produção (Render). Não é usada em runtime.
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Usa timestamp without time zone (sem exigência de UTC) nas migrations
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connStr = Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? "Host=localhost;Database=webservicos;Username=postgres;Password=postgres";
            optionsBuilder.UseNpgsql(connStr);
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
