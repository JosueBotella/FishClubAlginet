# Lanza backend (.NET API) y frontend (Vite) en paralelo
# Uso: .\dev.ps1

Write-Host "Arrancando Fish Club Alginet..." -ForegroundColor Cyan

# Frontend: npm run dev en background
$frontend = Start-Process -PassThru -NoNewWindow powershell -ArgumentList "-Command", "cd fishclubalginet-frontend; npm run dev"

# Backend: dotnet run
Write-Host "Frontend arrancado (PID: $($frontend.Id)) en http://localhost:5173" -ForegroundColor Green
Write-Host "Arrancando API en https://localhost:7179..." -ForegroundColor Green

try {
    dotnet run --project FishClubAlginet.API
} finally {
    # Al parar el backend (Ctrl+C), parar tambien el frontend
    if (!$frontend.HasExited) {
        Stop-Process -Id $frontend.Id -Force
        Write-Host "Frontend detenido." -ForegroundColor Yellow
    }
}
