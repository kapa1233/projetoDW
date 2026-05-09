# ============================================================
# publicar.ps1 — Deploy do WebServicos como Windows Service
# Executar como Administrador
# ============================================================

$ErrorActionPreference = "Stop"

$publishPath  = "C:\WebServicos"
$serviceName  = "WebServicos"
$serviceLabel = "WebServicos - Gestao de Servicos Web"
$projectPath  = "$PSScriptRoot\WebServicos\WebServicos"
$exePath      = "$publishPath\WebServicos.exe"

Write-Host "==> A publicar aplicacao..." -ForegroundColor Cyan
dotnet publish "$projectPath" -c Release -o "$publishPath" --nologo
if ($LASTEXITCODE -ne 0) { Write-Error "Erro no publish."; exit 1 }

# Copia a base de dados existente se ja houver uma no destino
$dbOrig  = "$projectPath\webservicos.db"
$dbDest  = "$publishPath\webservicos.db"
if ((Test-Path $dbOrig) -and !(Test-Path $dbDest)) {
    Copy-Item $dbOrig $dbDest
    Write-Host "==> Base de dados copiada." -ForegroundColor Green
}

# Para e remove servico anterior se existir
$svc = Get-Service $serviceName -ErrorAction SilentlyContinue
if ($svc) {
    Write-Host "==> A parar servico existente..." -ForegroundColor Yellow
    if ($svc.Status -eq "Running") { Stop-Service $serviceName -Force }
    Start-Sleep 2
    sc.exe delete $serviceName | Out-Null
    Start-Sleep 1
}

# Cria servico Windows
Write-Host "==> A criar Windows Service..." -ForegroundColor Cyan
New-Service -Name $serviceName `
            -DisplayName $serviceLabel `
            -BinaryPathName "`"$exePath`"" `
            -StartupType Automatic `
            -Description "Plataforma de gestao de servicos web — IPT"

# Define variavel de ambiente ASPNETCORE_ENVIRONMENT=Production no registo do servico
$regPath = "HKLM:\SYSTEM\CurrentControlSet\Services\$serviceName"
New-ItemProperty -Path $regPath -Name "Environment" -PropertyType MultiString `
    -Value "ASPNETCORE_ENVIRONMENT=Production","ASPNETCORE_URLS=http://localhost:5000" `
    -Force | Out-Null

Start-Service $serviceName
Write-Host ""
Write-Host "Servico '$serviceName' iniciado em http://localhost:5000" -ForegroundColor Green
Write-Host ""
Write-Host "Proximo passo: configura o Cloudflare Tunnel (ver DEPLOY.md)" -ForegroundColor Yellow
