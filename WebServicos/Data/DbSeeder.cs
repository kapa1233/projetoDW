using Microsoft.AspNetCore.Identity;
using WebServicos.Models;

namespace WebServicos.Data
{
    /// <summary>
    /// Inicializa roles e utilizadores padrão na base de dados.
    /// Credenciais disponíveis na página "Sobre" da aplicação.
    /// </summary>
    public static class DbSeeder
    {
        public const string RoleAdmin = "Administrador";
        public const string RoleCliente = "Cliente";

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // ── Criar roles ──
            foreach (var role in new[] { RoleAdmin, RoleCliente })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ── Criar utilizador Administrador ──
            const string adminEmail = "admin@webservicos.pt";
            const string adminPassword = "Admin@123";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NomeCompleto = "Administrador do Sistema",
                    EmailConfirmed = true,
                    DataRegisto = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, RoleAdmin);
            }

            // ── Criar utilizador Cliente de demonstração ──
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
