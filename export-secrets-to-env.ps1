# ============================================================
# export-secrets-to-env.ps1
# Exporta los user-secrets de HorreumStack.MiddleEnd a un
# archivo .env para usarlo con Docker en desarrollo local.
#
# Uso:
#   .\export-secrets-to-env.ps1
#   docker run --env-file .env -p 5033:80 horreumstack-middleend
# ============================================================

$projectPath = "$PSScriptRoot\HorreumStack.MiddleEnd.csproj"
$envFile      = "$PSScriptRoot\.env"

Write-Host "Leyendo user-secrets de HorreumStack.MiddleEnd..." -ForegroundColor Cyan

# Leer secrets en formato texto: "Key = Value"
$secretLines = dotnet user-secrets list --project $projectPath 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "No se pudieron leer los user-secrets. ¿Está inicializado el proyecto con 'dotnet user-secrets init'?"
    exit 1
}

# Escribir archivo .env
$lines = @()
$lines += "# Generado automáticamente por export-secrets-to-env.ps1"
$lines += "# NO commitear este archivo — está en .gitignore"
$lines += ""
$lines += "ASPNETCORE_ENVIRONMENT=Development"
$lines += ""

foreach ($line in $secretLines) {
    # Formato: "Key = Value"  (el separador es " = ")
    if ($line -match "^(.+?)\s*=\s*(.*)$") {
        $key      = $Matches[1].Trim()
        $value    = $Matches[2].Trim()
        # .NET usa ":" como separador de secciones; Docker env vars usan "__"
        $envKey   = $key -replace ":", "__"
        $lines   += "$envKey=$value"
    }
}

$lines | Set-Content -Path $envFile -Encoding UTF8
Write-Host ".env generado en: $envFile" -ForegroundColor Green
Write-Host "Secrets exportados:" -ForegroundColor Yellow
$lines | Where-Object { $_ -notmatch "^#" -and $_ -ne "" } | ForEach-Object {
    $parts = $_ -split "=", 2
    if ($parts.Length -eq 2) {
        $masked = $parts[1].Substring(0, [Math]::Min(4, $parts[1].Length)) + "****"
        Write-Host "  $($parts[0])=$masked" -ForegroundColor Gray
    }
}
