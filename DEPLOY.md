# Guia de Publicação — WebServicos

## 1. Pré-requisitos

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8) instalado
- PowerShell como Administrador
- Conta gratuita em [cloudflare.com](https://cloudflare.com)

---

## 2. Publicar como Windows Service

Abre o PowerShell **como Administrador** e corre:

```powershell
Set-Location "C:\Projectos\KapaDW"
.\publicar.ps1
```

A app fica disponível em `http://localhost:5000` e inicia automaticamente com o Windows.

### Gerir o serviço

```powershell
Stop-Service WebServicos       # parar
Start-Service WebServicos      # iniciar
Restart-Service WebServicos    # reiniciar
Get-Service WebServicos        # ver estado
```

### Ver logs

```powershell
Get-EventLog -LogName Application -Source WebServicos -Newest 20
```

---

## 3. HTTPS com Cloudflare Tunnel (grátis)

### 3.1 Instalar cloudflared

Descarrega o instalador em:  
https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/downloads/

Ou via PowerShell (winget):
```powershell
winget install Cloudflare.cloudflared
```

### 3.2 Autenticar

```powershell
cloudflared tunnel login
```
Abre o browser, faz login na Cloudflare e autoriza.

### 3.3 Criar o tunnel

```powershell
cloudflared tunnel create webservicos
```
Guarda o ID do tunnel que aparece (ex: `abc123-...`).

### 3.4 Criar ficheiro de configuração

Cria `C:\Users\<TeuUser>\.cloudflared\config.yml`:

```yaml
tunnel: <ID-DO-TUNNEL>
credentials-file: C:\Users\<TeuUser>\.cloudflared\<ID-DO-TUNNEL>.json

ingress:
  - hostname: webservicos.<teu-dominio>.com
    service: http://localhost:5000
  - service: http_status:404
```

> **Sem domínio próprio?** Usa o URL temporário gratuito:
> ```powershell
> cloudflared tunnel --url http://localhost:5000
> ```
> Dá-te um URL tipo `https://random-name.trycloudflare.com` com HTTPS incluído.

### 3.5 Instalar o tunnel como serviço Windows

```powershell
cloudflared service install
```

O tunnel inicia automaticamente com o Windows e mantém o HTTPS ativo.

---

## 4. Atualizar a aplicação

Sempre que fizeres alterações ao código:

```powershell
Set-Location "C:\Projectos\KapaDW"
.\publicar.ps1
```

O script para o serviço, re-publica e reinicia automaticamente.

---

## 5. Estrutura de ficheiros em produção

```
C:\WebServicos\
├── WebServicos.exe        ← executável principal
├── webservicos.db         ← base de dados SQLite
├── appsettings.json
├── appsettings.Production.json
└── wwwroot\               ← ficheiros estáticos (CSS, JS, imagens)
```
