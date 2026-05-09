using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

var builder = WebApplication.CreateBuilder(args);

// Suporte a Windows Service
builder.Host.UseWindowsService();

// ── Base de dados (SQLite) — caminho absoluto para funcionar como serviço ──
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

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "WebServicos API", Version = "v1" });
});

// CORS — permitir pedidos de qualquer origem (demo académico)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Swagger sempre ativo (necessário para demonstração académica)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebServicos API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
