# WebServicos — Trabalho Prático de Desenvolvimento Web

**Instituto Politécnico de Tomar (IPT)**
Licenciatura em Engenharia Informática — 2º Ano, 2º Semestre
Unidade Curricular: Desenvolvimento Web | Ano Letivo: 2025/2026

---

## Tema

Plataforma de gestão de serviços de criação de websites. Permite aos clientes registados submeter pedidos de desenvolvimento web, e ao administrador gerir serviços, pedidos e clientes.

---

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- Visual Studio 2022 / VS Code / Rider JetBrains

---

## Estrutura do Projeto

```
WebServicos/
├── WebServicos/            ← Aplicação Web (Razor Pages)
│   ├── Data/               ← DbContext e Seeder
│   ├── Hubs/               ← Hub SignalR
│   ├── Models/             ← Entidades da BD
│   ├── Pages/              ← Razor Pages (UI)
│   │   ├── Clientes/       ← CRUD Clientes (Admin)
│   │   ├── Pedidos/        ← CRUD Pedidos
│   │   ├── Servicos/       ← CRUD Serviços
│   │   └── Shared/         ← Layout, partials
│   └── wwwroot/            ← CSS, JS estáticos
└── WebServicos.API/        ← API REST (ASP.NET Core MVC)
    └── Controllers/        ← ServicosController, PedidosController
```

---

## Como Executar

### 1. Aplicação Web (Razor Pages)

```bash
cd WebServicos/WebServicos
dotnet restore
dotnet ef database update    # cria e migra a BD SQLite
dotnet run
```

Aceder em: https://localhost:7001

### 2. API REST

```bash
cd WebServicos/WebServicos.API
dotnet restore
dotnet run
```

Swagger UI disponível em: https://localhost:7002/swagger

---

## Base de Dados

- Motor: **SQLite** (ficheiro `webservicos.db`)
- ORM: **Entity Framework Core 8**
- Migrações automáticas ao arrancar

### Tabelas principais

| Tabela | Descrição |
|--------|-----------|
| `AspNetUsers` | Utilizadores (ASP.NET Identity) |
| `Servicos` | Serviços oferecidos |
| `Pedidos` | Pedidos dos clientes |
| `PedidoServicos` | Junção muitos-para-muitos (Pedido ↔ Serviço) |
| `CategoriasServico` | Categorias de serviços |

**Relacionamentos:**
- `Pedido` → `ApplicationUser` (muitos-para-um)
- `Pedido` ↔ `Servico` via `PedidoServico` (muitos-para-muitos)

---

## Credenciais de Acesso

| Perfil | Email | Password |
|--------|-------|----------|
| **Administrador** | `admin@webservicos.pt` | `Admin@123` |
| **Cliente** | `cliente@webservicos.pt` | `Cliente@123` |

> Estas credenciais estão também disponíveis na página **Sobre** da aplicação.

---

## Tecnologias Utilizadas

### Backend
- ASP.NET Core 8.0 — Razor Pages (aplicação web)
- ASP.NET Core 8.0 MVC — API REST
- Entity Framework Core 8.0 (ORM + migrações)
- LINQ (consultas tipadas)
- SignalR (notificações em tempo real)
- ASP.NET Core Identity (autenticação/autorização)

### Frontend
- [Bootstrap 5.3.2](https://getbootstrap.com) — Framework CSS
- [Bootstrap Icons 1.11.3](https://icons.getbootstrap.com) — Ícones
- [jQuery 3.7.1](https://jquery.com)
- Microsoft SignalR JS Client 8.0

### Base de Dados
- [SQLite](https://sqlite.org)

---

## Funcionalidades Implementadas

### Componente 1 — Razor Pages (60%)
- [x] CRUD completo de Serviços (com validação)
- [x] CRUD completo de Pedidos (com relação muitos-para-muitos)
- [x] Listagem e detalhes de Clientes (admin)
- [x] Autenticação e autorização (2 papéis: Admin / Cliente)
- [x] Notificações em tempo real com SignalR
- [x] Páginas de erro personalizadas (404, 403, geral)
- [x] Layout responsivo com Bootstrap 5
- [x] Página "Sobre" com informações do projeto

### Componente 2 — API REST (30%)
- [x] `GET /api/servicos` — listar serviços
- [x] `GET /api/servicos/{id}` — detalhe de serviço
- [x] `POST /api/servicos` — criar serviço
- [x] `PUT /api/servicos/{id}` — atualizar serviço
- [x] `DELETE /api/servicos/{id}` — eliminar serviço
- [x] `GET /api/servicos/estatisticas` — estatísticas
- [x] `GET /api/pedidos` — listar pedidos
- [x] `GET /api/pedidos/{id}` — detalhe de pedido
- [x] `PATCH /api/pedidos/{id}/estado` — alterar estado
- [x] Swagger UI para documentação e teste da API

### Componente 3 — Publicação (10%)
- [ ] Publicar em servidor web (a completar pelo grupo)

---

## Notas de Desenvolvimento

- O código está devidamente formatado e comentado em português
- A utilização de código de terceiros está referenciada na página "Sobre" e nos comentários
- Foram implementadas validações nos formulários (client-side e server-side)
- As operações de remoção validam dependências antes de eliminar
