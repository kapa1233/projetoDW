using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Suporte a Windows Service (no-op noutras plataformas como Linux/Docker) ──
builder.Host.UseWindowsService();

// ── Base de dados ──
// Em produção (Render) usa PostgreSQL via DATABASE_URL; localmente usa SQLite
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (databaseUrl != null)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(databaseUrl));
}
else
{
    var rawConn = builder.Configuration.GetConnectionString("DefaultConnection")!;
    const string sqlitePrefix = "Data Source=";
    var dbConn = rawConn;
    if (rawConn.StartsWith(sqlitePrefix, StringComparison.OrdinalIgnoreCase))
    {
        var dbFile = rawConn[sqlitePrefix.Length..];
        if (!Path.IsPathRooted(dbFile))
            dbConn = sqlitePrefix + Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, dbFile));
    }
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(dbConn));
}

// ── ASP.NET Core Identity ──
// Necessário para aceder às tabelas de utilizadores partilhadas com a app principal
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ── Controllers MVC (API REST) ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger / OpenAPI ──
// Interface web para explorar e testar os endpoints da API
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "WebServicos API", Version = "v1" });
});

// ── CORS (Cross-Origin Resource Sharing) ──
// Permite pedidos de qualquer origem (necessário para demo académico e integrações externas)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ── Inicialização da base de dados ──
// Aplica migrações pendentes ao arrancar (cria o ficheiro .db se não existir)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (Environment.GetEnvironmentVariable("DATABASE_URL") != null)
        db.Database.Migrate();        // PostgreSQL produção: aplica migrações
    else
        db.Database.EnsureCreated(); // SQLite local: cria schema diretamente
}

// ── Swagger sempre ativo ──
// Disponível em /index.html mesmo em produção (necessário para demonstração académica)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebServicos API v1");
    c.RoutePrefix = string.Empty; // Swagger na raiz: http://localhost:5001/
});

app.UseCors("AllowAll");        // Aplicar política CORS
app.UseAuthentication();        // Verificar autenticação (cookie/token)
app.UseAuthorization();         // Verificar autorização (roles)
app.MapControllers();           // Mapear rotas dos controllers

app.Run();
