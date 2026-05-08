# WebServicos — Plataforma de Gestão de Serviços Web

## Contexto
Trabalho académico — IPT, Licenciatura em Engenharia Informática, 2º ano.
Plataforma onde clientes submetem pedidos de desenvolvimento web e o admin faz a gestão.

## Stack
- ASP.NET Core 8 (Razor Pages + MVC para API)
- Entity Framework Core 8 + SQLite
- ASP.NET Core Identity (papéis: Admin, Cliente)
- SignalR para notificações em tempo real
- Bootstrap 5.3.2, jQuery 3.7.1

## Estrutura da solução
- `WebServicos/` — App web Razor Pages (porta 7001)
- `WebServicos.API/` — API REST com Swagger (porta 7002)

## Convenções
- Código, comentários, nomes de variáveis e mensagens de UI em **português**.
- Nomes de classes e ficheiros em PascalCase (convenção C#).
- Migrations EF Core devem ser criadas via `dotnet ef migrations add`.

## A fazer (componente 3)
- Publicação em servidor.

## Notas para o assistente
- **Não** alterar `appsettings.Production.json` nem ficheiros de secrets.
- Antes de mexer em modelos/migrations, confirma comigo.
- Mantém compatibilidade com SQLite (evita features só de SQL Server).
