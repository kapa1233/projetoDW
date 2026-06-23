using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebServicos.Data;
using WebServicos.Hubs;
using WebServicos.Models;
using WebServicos.Services;

// Permite DateTime sem Kind=Utc no seed data das migrations do PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ── Suporte a Windows Service (no-op noutras plataformas) ──
// Permite executar a aplicação como serviço Windows sem janela de consola
builder.Host.UseWindowsService();

// ── Base de dados ──
// Prioridade: 1) DATABASE_URL (env var, formato postgresql://...)
//             2) Connection string PostgreSQL no appsettings.json (formato ADO.NET)
//             3) SQLite local (desenvolvimento, "Data Source=...")
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var connString = builder.Configuration.GetConnectionString("DefaultConnection")!;

if (databaseUrl != null)
{
    // Variável de ambiente DATABASE_URL (formato postgresql://user:pass@host:port/db)
    var npgsqlConn = ConvertDatabaseUrl(databaseUrl);
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(npgsqlConn));
}
else if (!connString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
{
    // Connection string no appsettings.Production.json (formato ADO.NET para PostgreSQL)
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connString));
}
else
{
    // SQLite local: resolve caminho relativo para absoluto (necessário como Windows Service)
    const string sqlitePrefix = "Data Source=";
    var dbConn = connString;
    var dbFile = connString[sqlitePrefix.Length..];
    if (!Path.IsPathRooted(dbFile))
        dbConn = sqlitePrefix + Path.Combine(AppContext.BaseDirectory, dbFile);
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

// ── Controladores de API ──
builder.Services.AddControllers();

// ── Swagger / OpenAPI ──
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KapaDW API",
        Version = "v1",
        Description = "API REST da plataforma KapaDW — gestão de pedidos de serviços web.\n\n" +
                      "Para testar os endpoints protegidos, inicia sessão na aplicação web " +
                      "e volta a esta página: o cookie de autenticação é partilhado automaticamente.",
        Contact = new OpenApiContact
        {
            Name = "Diogo Godinho & Kanstantsin Khomchanka",
            Email = "admin@webservicos.pt"
        }
    });

    // Esquema de segurança por cookie (partilhado com o site Razor Pages)
    options.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = ".AspNetCore.Identity.Application",
        Description = "Cookie de sessão gerado após login na aplicação web."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "cookieAuth" }
            },
            Array.Empty<string>()
        }
    });

    // Inclui os comentários XML dos controllers no Swagger UI
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ── Serviços da aplicação ──
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();

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

// ── Swagger UI ──
// Disponível em /swagger — acessível em desenvolvimento e produção
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "KapaDW API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "KapaDW — Documentação da API";
    options.DefaultModelsExpandDepth(-1); // Oculta a secção "Schemas" por defeito
});

// ── Mapeamento de rotas ──
app.MapRazorPages();
app.MapControllers();    // Mapeia controladores de API
app.MapHub<NotificacoesHub>("/hub/notificacoes"); // Endpoint WebSocket do SignalR

// ── Inicialização da base de dados ──
// EnsureCreated cria as tabelas se não existirem (SQLite e PostgreSQL)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
    await DbSeeder.SeedAsync(scope.ServiceProvider);   // Cria roles e utilizadores padrão
}

app.Run();

static string ConvertDatabaseUrl(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':', 2);
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');
    var username = userInfo[0];
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}
