using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Hubs;
using WebServicos.Models;

var builder = WebApplication.CreateBuilder(args);

// Suporte a Windows Service (funciona também como consola em desenvolvimento)
builder.Host.UseWindowsService();

// ── Base de dados (SQLite via appsettings.json) ──
// Resolve o caminho relativo para absoluto — necessário quando corre como Windows Service
var rawConn = builder.Configuration.GetConnectionString("DefaultConnection")!;
const string sqlitePrefix = "Data Source=";
var dbConn = rawConn;
if (rawConn.StartsWith(sqlitePrefix, StringComparison.OrdinalIgnoreCase))
{
    var dbFile = rawConn[sqlitePrefix.Length..];
    if (!Path.IsPathRooted(dbFile))
        dbConn = sqlitePrefix + Path.Combine(AppContext.BaseDirectory, dbFile);
}
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(dbConn));

// ── ASP.NET Core Identity ──
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// ── Razor Pages ──
builder.Services.AddRazorPages(options =>
{
    // Áreas protegidas — requerem autenticação
    options.Conventions.AuthorizeFolder("/Pedidos");
    options.Conventions.AuthorizeFolder("/Clientes", "Administrador");
    options.Conventions.AuthorizePage("/Servicos/Create", "Administrador");
    options.Conventions.AuthorizePage("/Servicos/Edit", "Administrador");
    options.Conventions.AuthorizePage("/Servicos/Delete", "Administrador");
});

// ── Políticas de autorização ──
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireRole(DbSeeder.RoleAdmin));
    options.AddPolicy("Cliente", policy => policy.RequireRole(DbSeeder.RoleCliente, DbSeeder.RoleAdmin));
});

// ── SignalR (notificações em tempo real) ──
builder.Services.AddSignalR();

// ── Configuração de cookies ──
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Erro/AcessoNegado";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

var app = builder.Build();

// ── Pipeline HTTP ──
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
else
{
    app.UseExceptionHandler("/Erro/Geral");
    app.UseHsts();
}

// Páginas de erro personalizadas
app.UseStatusCodePagesWithRedirects("/Erro/{0}");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ── Rotas ──
app.MapRazorPages();
app.MapHub<NotificacoesHub>("/hub/notificacoes");

// ── Seed da base de dados ──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
