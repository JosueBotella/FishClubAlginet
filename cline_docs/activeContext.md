# Contexto Activo — Próximo: Fase 5 (PointsCalculator)

## Estado actual
Fases 1–4 completadas. El **bug crítico de puntos** (`Points = WeightInGrams`) bloquea la correctitud de la clasificación "Por Puntos" de la liga. Debe resolverse antes de cualquier trabajo en vistas de clasificación.

---

## 🔴 BUG CRÍTICO — Points = WeightInGrams (Fase 5.A)

**Archivo afectado:** `FishClubAlginet.Core/Domain/Entities/CompetitionResult.cs` — método `RecordResult()`

```csharp
// INCORRECTO — esto es peso, no puntos de ranking:
Points = weightInGrams > 0 ? weightInGrams : minPoints;
```

**Síntoma:** La clasificación "Por Puntos" en `LeagueStandingsPage` muestra gramos acumulados, no puntos reales del sistema de liga por sorteo inverso. `ByWeight` y `ByPoints` son idénticas.

**Algoritmo correcto** (confirmado con `18º - CONCURSO.xls`):
1. Ordenar asistentes desc por `WeightInGrams`. Asignar `Ranking` con empates (mismo peso → mismo rank, el siguiente salta).
2. Puntos del 1º = `N` posiciones únicas. *Ej: 27 participantes − 2 empates dobles = 25 pts al 1º*.
3. Empates comparten media de los puntos de sus posiciones. *Ej: pos 14-15 → (12+11)/2 = 11,5. Pos 18-19 → (8+7)/2 = 7,5*.
4. Mínimo `League.MinPoints` (default 5) para todo asistente, aunque pese 0 g.
5. Ausencia (`DidAttend = false`) → `Points = 0`, sin mínimo.

**Fix requerido:** `IPointsCalculator` domain service en `Core` + `CalculateCompetitionPointsCommand` + invocar desde `MoveToResultsDraftCommandHandler`.

---

## ✅ Completado (Fases 1–4)

| Fase | Descripción | Fecha |
|------|-------------|-------|
| 1 | Auth, Identity, Users, Docker | 2026-05 |
| 2 | Leagues CRUD + Rich Domain Model | 2026-05 |
| 3 | Competitions + Results + Outbox Pattern | 2026-05-14 |
| 3.5 | Outbox estabilizado (TASK-A, B, C) + build fixes | 2026-05-15 |
| 4 | Estados avanzados (Reopen, AssignSpots, MoveToResultsDraft, ValidateResults, UnarchiveLeague), GetLeagueStandings básico, Frontend completo (ConfirmationModal, AdminArchivedLeaguesPage, LeagueStandingsPage, status guards) | 2026-05-16 |

---

## 🔲 Plan Fase 5 — PointsCalculator + Clasificación detallada

### Prioridad 1 (bloqueante): 5.A — PointsCalculator

**Backend:**
1. `IPointsCalculator` en `Core/Domain/Services/` — stateless, recibe lista de resultados + minPoints → devuelve lista con `Points` y `Ranking` calculados.
2. `CalculateCompetitionPointsCommand(Guid CompetitionId)` + Handler en `Application`.
3. Modificar `MoveToResultsDraftCommandHandler` para llamar al comando automáticamente.
4. Tests `PointsCalculatorTests` — casos: happy path, empate doble, empate triple, todos 0 g, 1 participante, mínimo aplicado, ausencia, mezcla.

**Frontend:**
- Una vez corregido el backend, `LeagueStandingsPage` pestañas "Por Puntos" mostrará datos correctos sin cambios frontend.

### Prioridad 2: 5.B — Clasificación en matriz (estilo Excel)

- Ampliar `GetLeagueStandingsQuery` con desglose por concurso: `CompetitionId → decimal` por pescador.
- Frontend: scroll horizontal con columna por concurso, totales en última fila.
- Tests con datos reales `LIGA POR PESO 2025.xls` (43 pescadores, 18 concursos, total 1.071.845 g).

### Prioridad 3: 5.C — Pieza Mayor

- `GetSeasonBiggestCatchQuery(leagueId)` + `GetCompetitionBiggestCatchQuery(competitionId)`.

---

## 🔲 Plan Fase 6 — Acta Oficial FPCV (Word/PDF)

- `GET /api/competitions/{id}/acta?format={pdf|docx}` — solo Admin, solo `ResultsValidated`.
- Generación Word con `OpenXml SDK`. Conversor → PDF con LibreOffice headless.
- Modal en frontend con campos editables (presidente, jueces, especies, tiempo).

---

## 🔲 Plan Fase 7 — Frontend rol Fisherman

- Calendario + Mis inscripciones + Sidebar Fisherman.

---

## Notas arquitecturales clave

- **Domain Events:** se lanzan en el **handler** (Application), NO en la entidad (Core) — evita dependencia circular.
- **Points vs Weight:** `CompetitionResult.Points` debe almacenar puntos de ranking (Fase 5.A), no gramos.
- **Standings:** `GetLeagueStandingsQuery` usa `r.Points` — correcto una vez arreglado el bug.
- **Docker:** `docker compose up -d` para desarrollo. Portainer en `http://localhost:19100`.
- **Tests:** patrón AAA, FluentAssertions, Moq. Naming: `Handle_WhenXxx_ShouldYyy`.
