param(
    [string]$ProjectKey = "FishClubAlginet",
    [string]$SonarHostUrl = "http://localhost:9000",
    [string]$Token = "sqp_admin_token"
)

Write-Host "Verificando herramienta dotnet-sonarscanner..." -ForegroundColor Cyan

if (-not (Get-Command dotnet-sonarscanner -ErrorAction SilentlyContinue)) {
    Write-Host "Instalando dotnet-sonarscanner de forma global..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-sonarscanner
}

Write-Host "Iniciando sesion de analisis en SonarQube ($SonarHostUrl)..." -ForegroundColor Cyan

dotnet sonarscanner begin /k:"$ProjectKey" /d:sonar.host.url="$SonarHostUrl" /d:sonar.token="$Token" /d:sonar.cs.vstest.reportsPaths="**/*.trx" /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Error al iniciar SonarScanner."
    exit $LASTEXITCODE
}

Write-Host "Compilando la solucion..." -ForegroundColor Cyan
dotnet build --no-incremental

Write-Host "Ejecutando pruebas unitarias y generando cobertura..." -ForegroundColor Cyan
dotnet test --logger "trx" /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

Write-Host "Finalizando analisis y enviando informe a SonarQube..." -ForegroundColor Cyan
dotnet sonarscanner end /d:sonar.token="$Token"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Analisis de SonarQube completado con exito." -ForegroundColor Green
} else {
    Write-Error "El analisis de SonarQube finalizo con errores."
    exit $LASTEXITCODE
}
