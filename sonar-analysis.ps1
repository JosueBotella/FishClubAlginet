param(
    [string]$Token = $env:SONAR_TOKEN,
    [string]$SonarUrl = $env:SONAR_HOST_URL,
    [string]$ProjectKey = "FishClubAlginet"
)

if ([string]::IsNullOrWhiteSpace($SonarUrl)) {
    $SonarUrl = "http://localhost:9000"
}

if ([string]::IsNullOrWhiteSpace($Token)) {
    Write-Host "ADVERTENCIA: No se especificó el token mediante -Token ni `$env:SONAR_TOKEN." -ForegroundColor Yellow
    $Token = Read-Host -Prompt "Introduce tu token de SonarQube"
}

if ([string]::IsNullOrWhiteSpace($Token)) {
    Write-Error "Error: Se requiere un token válido de SonarQube para realizar el análisis."
    exit 1
}

Write-Host "=== Iniciando Análisis de SonarQube para $ProjectKey ===" -ForegroundColor Cyan

# 1. Comenzar análisis de SonarScanner para .NET Backend
Write-Host "1. Ejecutando sonarscanner begin..." -ForegroundColor Yellow
dotnet sonarscanner begin `
    /k:"$ProjectKey" `
    /d:sonar.host.url="$SonarUrl" `
    /d:sonar.token="$Token" `
    /d:sonar.cs.vstest.reportsPaths="**/*.trx" `
    /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" `
    /d:sonar.coverage.exclusions="**/Migrations/**,**/Program.cs,**/FishClubAlginet.Tests/**"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Fallo en sonarscanner begin."
    exit 1
}

# 2. Compilar solución completa
Write-Host "2. Compilando solución .NET..." -ForegroundColor Yellow
dotnet build --no-incremental

if ($LASTEXITCODE -ne 0) {
    Write-Error "Fallo en la compilación de la solución."
    exit 1
}

# 3. Ejecutar pruebas con reporte de cobertura OpenCover
Write-Host "3. Ejecutando pruebas unitarias y de integración..." -ForegroundColor Yellow
dotnet test --no-build --logger "trx" --collect:"XPlat Code Coverage;Format=opencover"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Fallo en la ejecución de pruebas."
    exit 1
}

# 4. Finalizar análisis y enviar a SonarQube Server
Write-Host "4. Finalizando análisis sonarscanner end..." -ForegroundColor Yellow
dotnet sonarscanner end /d:sonar.token="$Token"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Fallo al finalizar el análisis de SonarQube."
    exit 1
}

Write-Host "=== Análisis completado con éxito. Revisa el dashboard en $SonarUrl ===" -ForegroundColor Green
