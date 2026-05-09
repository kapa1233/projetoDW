# ============================================================
# publicar.ps1 — Deploy do WebServicos + WebServicos.API
# Executar como Administrador
# ============================================================

$ErrorActionPreference = "Stop"

$appPath     = "C:\WebServicos"
$apiPath     = "C:\WebServicosAPI"
$projectApp  = "$PSScriptRoot\WebServicos\WebServicos"
$projectApi  = "$PSScriptRoot\WebServicos\WebServicos.API"

# ── Publicar App principal ──
Write-Host "==> A publicar App principal..." -ForegroundColor Cyan
dotnet publish "$projectApp" -c Release -r win-x64 --self-contained true -o "$appPath" --nologo
if ($LASTEXITCODE -ne 0) { Write-Error "Erro ao publicar App."; exit 1 }

# ── Publicar API ──
Write-Host "==> A publicar API..." -ForegroundColor Cyan
dotnet publish "$projectApi" -c Release -r win-x64 --self-contained true -o "$apiPath" --nologo
if ($LASTEXITCODE -ne 0) { Write-Error "Erro ao publicar API."; exit 1 }

# ── Copiar base de dados ──
$dbOrig = "$projectApp\webservicos.db"
if (Test-Path $dbOrig) {
    if (!(Test-Path "$appPath\webservicos.db")) {
        Copy-Item $dbOrig "$appPath\webservicos.db"
        Write-Host "==> Base de dados copiada para App." -ForegroundColor Green
    }
    if (!(Test-Path "$apiPath\webservicos.db")) {
        Copy-Item $dbOrig "$apiPath\webservicos.db"
        Write-Host "==> Base de dados copiada para API." -ForegroundColor Green
    }
}

# ── Gerar instalar.ps1 para o servidor ──
$installerContent = @'
# ============================================================
# instalar.ps1 - Corre no PC SERVIDOR como Administrador
# Set-Location "C:\WebServicos"; .\instalar.ps1
# ============================================================

$ErrorActionPreference = "Stop"

function Install-Servico {
    param($Nome, $Label, $ExePath, $Porta, $Descricao)

    $svc = Get-Service $Nome -ErrorAction SilentlyContinue
    if ($svc) {
        Write-Host "--> A remover '$Nome' anterior..." -ForegroundColor Yellow
        if ($svc.Status -eq "Running") { Stop-Service $Nome -Force }
        Start-Sleep 2
        sc.exe delete $Nome | Out-Null
        Start-Sleep 1
    }

    Write-Host "--> A instalar '$Nome'..." -ForegroundColor Cyan
    New-Service -Name $Nome -DisplayName $Label `
                -BinaryPathName "`"$ExePath`"" `
                -StartupType Automatic -Description $Descricao

    $regPath = "HKLM:\SYSTEM\CurrentControlSet\Services\$Nome"
    New-ItemProperty -Path $regPath -Name "Environment" -PropertyType MultiString `
        -Value @("ASPNETCORE_ENVIRONMENT=Production", "ASPNETCORE_URLS=http://localhost:$Porta") `
        -Force | Out-Null

    Start-Service $Nome
    Start-Sleep 2
    $estado = (Get-Service $Nome).Status
    if ($estado -eq "Running") {
        Write-Host "   OK - a correr em http://localhost:$Porta" -ForegroundColor Green
    } else {
        Write-Host "   ERRO - estado: $estado" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== INSTALACAO WEBSERVICOS ===" -ForegroundColor Cyan
Write-Host ""

Install-Servico -Nome "WebServicos" `
                -Label "WebServicos - App Principal" `
                -ExePath "C:\WebServicos\WebServicos.exe" `
                -Porta 5000 `
                -Descricao "Plataforma de gestao de servicos web - IPT"

Install-Servico -Nome "WebServicosAPI" `
                -Label "WebServicos - API REST" `
                -ExePath "C:\WebServicosAPI\WebServicos.API.exe" `
                -Porta 5001 `
                -Descricao "API REST WebServicos - IPT"

Write-Host ""
Write-Host "Instalacao concluida!" -ForegroundColor Green
Write-Host "  App:    http://localhost:5000" -ForegroundColor White
Write-Host "  API:    http://localhost:5001" -ForegroundColor White
Write-Host "  Swagger: http://localhost:5001/index.html" -ForegroundColor White
Write-Host ""
Write-Host "Proximo passo: Cloudflare Tunnel" -ForegroundColor Yellow
Write-Host "  cloudflared tunnel --url http://localhost:5000 --protocol http2"
'@

$installerContent | Out-File -FilePath "$appPath\instalar.ps1" -Encoding utf8
Write-Host "==> instalar.ps1 gerado em $appPath" -ForegroundColor Green

Write-Host ""
Write-Host "Publicacao concluida!" -ForegroundColor Green
Write-Host "  App -> $appPath"
Write-Host "  API -> $apiPath"
Write-Host ""
Write-Host "Copia ambas as pastas para o PC servidor e corre:" -ForegroundColor Yellow
Write-Host "  Set-Location C:\WebServicos; .\instalar.ps1"
