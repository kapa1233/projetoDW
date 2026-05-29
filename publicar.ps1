# ============================================================
# publicar.ps1 — Atualiza e publica o WebServicos no servidor
# Executar como Administrador em C:\Projectos\KapaDW
# ============================================================

$ErrorActionPreference = "Stop"

$appPath    = "C:\WebServicos\app"
$apiPath    = "C:\WebServicos\api"
$projectApp = "$PSScriptRoot\WebServicos\WebServicos"
$projectApi = "$PSScriptRoot\WebServicos\WebServicos.API"

Write-Host ""
Write-Host "=== WEBSERVICOS — PUBLICACAO ===" -ForegroundColor Cyan
Write-Host ""

# ── 1. Buscar ultimas alteracoes do GitHub ──
Write-Host "--> A fazer git pull..." -ForegroundColor Yellow
git -C $PSScriptRoot pull
Write-Host ""

# ── 2. Parar servicos antes de publicar ──
foreach ($svc in @("WebServicos", "WebServicosAPI")) {
    $s = Get-Service $svc -ErrorAction SilentlyContinue
    if ($s -and $s.Status -eq "Running") {
        Write-Host "--> A parar servico $svc..." -ForegroundColor Yellow
        sc.exe stop $svc 2>&1 | Out-Null
        Start-Sleep 3
    }
}

# ── 3. Publicar App principal ──
Write-Host "--> A publicar App principal..." -ForegroundColor Cyan
dotnet publish "$projectApp" -c Release -r win-x64 --self-contained true -o "$appPath" --nologo
if ($LASTEXITCODE -ne 0) { Write-Error "Erro ao publicar App."; exit 1 }

# ── 4. Publicar API ──
Write-Host "--> A publicar API..." -ForegroundColor Cyan
dotnet publish "$projectApi" -c Release -r win-x64 --self-contained true -o "$apiPath" --nologo
if ($LASTEXITCODE -ne 0) { Write-Error "Erro ao publicar API."; exit 1 }

# ── 5. Copiar base de dados (so na primeira vez) ──
$dbOrig = "$projectApp\webservicos.db"
if ((Test-Path $dbOrig) -and !(Test-Path "$appPath\webservicos.db")) {
    Copy-Item $dbOrig "$appPath\webservicos.db"
    Write-Host "--> Base de dados copiada." -ForegroundColor Green
}

# ── 6. Instalar/reiniciar servicos ──
function Instalar-Servico {
    param($Nome, $Label, $Exe, $Porta)

    $svc = Get-Service $Nome -ErrorAction SilentlyContinue
    if ($svc) {
        sc.exe delete $Nome 2>&1 | Out-Null
        Start-Sleep 2
    }

    New-Service -Name $Nome -DisplayName $Label `
                -BinaryPathName "`"$Exe`"" `
                -StartupType Automatic | Out-Null

    $reg = "HKLM:\SYSTEM\CurrentControlSet\Services\$Nome"
    New-ItemProperty -Path $reg -Name "Environment" -PropertyType MultiString `
        -Value @("ASPNETCORE_ENVIRONMENT=Production", "ASPNETCORE_URLS=http://localhost:$Porta") `
        -Force | Out-Null

    Start-Service $Nome
    Start-Sleep 2
    $estado = (Get-Service $Nome).Status
    if ($estado -eq "Running") {
        Write-Host "   OK - http://localhost:$Porta" -ForegroundColor Green
    } else {
        Write-Host "   ERRO - estado: $estado" -ForegroundColor Red
    }
}

Write-Host "--> A instalar servicos..." -ForegroundColor Cyan
Instalar-Servico -Nome "WebServicos"    -Label "WebServicos - App Principal" -Exe "$appPath\WebServicos.exe"    -Porta 5000
Instalar-Servico -Nome "WebServicosAPI" -Label "WebServicos - API REST"       -Exe "$apiPath\WebServicos.API.exe" -Porta 5001

Write-Host ""
Write-Host "Concluido!" -ForegroundColor Green
Write-Host "  App:     http://localhost:5000  (C:\WebServicos\app)"
Write-Host "  API:     http://localhost:5001  (C:\WebServicos\api)"
Write-Host "  Swagger: http://localhost:5001/index.html"
Write-Host ""
Write-Host "Para expor ao exterior:" -ForegroundColor Yellow
Write-Host "  cloudflared tunnel --url http://localhost:5000 --protocol http2"
