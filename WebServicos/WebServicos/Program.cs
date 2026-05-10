using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Hubs;
using WebServicos.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Suporte a Windows Service (no-op noutras plataformas) ──
// Permite executar a aplicação como serviço Windows sem janela de consola
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
    // SQLite local: resolve caminho relativo para absoluto (necessário como Windows Service)
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
}

// ── ASP.NET Core Identity ──
// Configura autenticação com utilizadores personalizados (ApplicationUser) e roles
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;   // Sem verificação de email
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Lockout.MaxFailedAccessAttempts = 5;      // Bloqueia após 5 tentativas falhadas
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddRoles<IdentityRole>()                             // Suporte a roles (Administrador, Cliente)
.AddEntityFrameworkStores<ApplicationDbContext>();

// ── Razor Pages ──
// Configura autorização por pasta/página (sem necessidade de [Authorize] em cada page model)
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Pedidos");                              // Todos os pedidos requerem login
    options.Conventions.AuthorizeFolder("/Clientes", "Administrador");            // Apenas admins
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

// ── SignalR ──
// Usado para notificações em tempo real (novos pedidos, atualizações de estado)
builder.Services.AddSignalR();

// ── Configuração de cookies de autenticação ──
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";         // Redireciona para login se não autenticado
    options.AccessDeniedPath = "/Erro/AcessoNegado";       // Página de acesso negado
    options.ExpireTimeSpan = TimeSpan.FromHours(8);        // Sessão expira em 8 horas
});

var app = builder.Build();

// ── Pipeline HTTP ──
if (app.Environment.IsDevelopment())
{
    // Em desenvolvimento usa HTTPS local; em produção o proxy (Render/Cloudflare) trata do HTTPS
    app.UseHttpsRedirection();
}
else
{
    // Em produção, redireciona erros para página personalizada
    app.UseExceptionHandler("/Erro/Geral");
    app.UseHsts();
}

// Redireciona códigos de erro HTTP (404, 403, etc.) para páginas personalizadas
app.UseStatusCodePagesWithRedirects("/Erro/{0}");

app.UseStaticFiles();    // Serve ficheiros estáticos (CSS, JS, imagens)
app.UseRouting();
app.UseAuthentication(); // Verifica quem é o utilizador (cookie de sessão)
app.UseAuthorization();  // Verifica o que o utilizador pode fazer (roles e políticas)

// ── Mapeamento de rotas ──
app.MapRazorPages();
app.MapHub<NotificacoesHub>("/hub/notificacoes"); // Endpoint WebSocket do SignalR

// ── Inicialização da base de dados ──
// Aplica migrações pendentes e faz seed de dados iniciais ao arrancar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (Environment.GetEnvironmentVariable("DATABASE_URL") != null)
        db.Database.Migrate();        // PostgreSQL produção: aplica migrações
    else
        db.Database.EnsureCreated(); // SQLite local: cria schema diretamente
    await DbSeeder.SeedAsync(scope.ServiceProvider);   // Cria roles e utilizadores padrão
}

app.Run();
