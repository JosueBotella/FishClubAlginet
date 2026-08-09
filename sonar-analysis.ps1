param(
    [string]$Token = "squ_34a47cd6f1e5aabd66dfbff41bf4ce7efae694fe",
    [string]$SonarUrl = "http://localhost:9000",
    [string]$ProjectKey = "FishClubAlginet"
)

Write-Host "=== Iniciando Análisis de SonarQube para FishClubAlginet ===" -ForegroundColor Cyan

# 1. Comenzar análisis de SonarScanner para .NET Backend
Write-Host "1. Ejecutando sonarscanner begin..." -ForegroundColor Yellow
dotnet sonarscanner begin `
    /k:"$ProjectKey" `
    /d:sonar.host.url="$SonarUrl" `
    /d:sonar.token="$Token" `
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
dotnet test --no-build --collect:"XPlat Code Coverage;Format=opencover"

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
