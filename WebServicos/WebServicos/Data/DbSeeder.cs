using Microsoft.AspNetCore.Identity;
using WebServicos.Models;

namespace WebServicos.Data
{
    /// <summary>
    /// Responsável por inicializar roles e utilizadores padrão na base de dados.
    /// Executado uma única vez ao arrancar a aplicação, através do Program.cs.
    /// Credenciais disponíveis na página "Sobre" da aplicação.
    /// </summary>
    public static class DbSeeder
    {
        /// <summary>Nome do role de administrador do sistema.</summary>
        public const string RoleAdmin = "Administrador";

        /// <summary>Nome do role de cliente da plataforma.</summary>
        public const string RoleCliente = "Cliente";

        /// <summary>
        /// Cria os roles e utilizadores de demonstração caso ainda não existam.
        /// Utiliza o padrão idempotente: seguro para executar múltiplas vezes.
        /// </summary>
        /// <param name="serviceProvider">Serviços de injeção de dependências do ASP.NET Core.</param>
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // ── Criar roles se não existirem ──
            foreach (var role in new[] { RoleAdmin, RoleCliente })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ── Criar utilizador Administrador (acesso completo ao sistema) ──
            const string adminEmail = "admin@webservicos.pt";
            const string adminPassword = "Admin@123";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NomeCompleto = "Administrador do Sistema",
                    EmailConfirmed = true,   // Sem necessidade de confirmar email
                    DataRegisto = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, RoleAdmin);
            }

            // ── Criar utilizador Cliente de demonstração (para testes) ──
            const string clienteEmail = "cliente@webservicos.pt";
            const string clientePassword = "Cliente@123";

            if (await userManager.FindByEmailAsync(clienteEmail) == null)
            {
                var cliente = new ApplicationUser
                {
                    UserName = clienteEmail,
                    Email = clienteEmail,
                    NomeCompleto = "Cliente Demonstração",
                    EmailConfirmed = true,
                    DataRegisto = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(cliente, clientePassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(cliente, RoleCliente);
            }
        }
    }
}
