## Reglas de Entorno (Crítico)
- Tienes disponible el servidor MCP local de CodeGraph.
- Antes de leer archivos `.cs` de forma secuencial, DEBES invocar el MCP de CodeGraph para analizar las dependencias y la estructura del código.

# Contexto Activo — Fase 5: Clasificación Detallada y Pieza Mayor (Fases 5.B - 5.E)

## Estado actual
Fases 1 a 5.A completamente completadas. El **bug crítico de puntos** (`Points = WeightInGrams`) ha sido resuelto exitosamente mediante la implementación del servicio de dominio `PointsCalculatorService` y la persistencia correcta de puntos por ranking y empates (sistema confirmado de 20 a 1 puntos + asistencia + reparto de empates).

También se ha completado la configuración del **mínimo de peso para Pieza Mayor por competición** a nivel de base de datos, backend (PATCH endpoint) y frontend (Mantine input y actualización inline).

---

## Contexto Activo — Fase 5: Clasificación Detallada y Pieza Mayor (Fases 5.C - 5.E)

## Estado actual
Fases 1 a 5.B completamente completadas. El **bug crítico de puntos** (`Points = WeightInGrams`) ha sido resuelto exitosamente mediante la implementación del servicio de dominio `PointsCalculatorService` y la persistencia correcta de puntos por ranking y empates (sistema confirmado de 20 a 1 puntos + asistencia + reparto de empates).

También se ha completado la configuración del **mínimo de peso para Pieza Mayor por competición** a nivel de base de datos, backend (PATCH endpoint) y frontend (Mantine input y actualización inline).

Se ha completado en el Backend e Integración de Tests la **Fase 5.B: Clasificación Detallada (Matriz por Concurso)**. Esto incluye la creación de todos los DTOs de contrato matricial (`CompetitionHeaderDto`, `CompetitionCellDto`, `FishermanMatrixRowDto`, `LeagueStandingsMatrixDto`), el QueryHandler de MediatR `GetLeagueStandingsMatrixQueryHandler` (con la lógica avanzada de descartes secuenciales basándonos en `WorstResultsToDiscard`), la ruta del API Controller `GET /api/leagues/{id}/standings-matrix` y una robusta suite de pruebas unitarias cubriendo todas las especificaciones y edge cases, logrando 0 warnings y 193 pruebas exitosas en total.

Se ha completado en el Backend e Integración de Tests la **Fase 5.C: Agregaciones de Pieza Mayor (Global y por Jornada)**. Esto abarca la creación de los DTOs `SeasonBiggestCatchDto` y `CompetitionBiggestCatchDto`, el record modificado `CompetitionResultDto` con `IsBiggestCatch`, los QueryHandlers de MediatR `GetSeasonBiggestCatchQueryHandler` y `GetCompetitionBiggestCatchQueryHandler`, la integración del cálculo en vivo del ganador del trofeo en `GetCompetitionResultsQueryHandler`, las rutas de controlador `GET /api/leagues/{id}/biggest-catch` y `GET /api/competitions/{id}/biggest-catch`, y la implementación de sus respectivas pruebas unitarias con 0 fallos.

---

## ✅ Fase 5.D COMPLETADA (2026-06-11): Frontend de la Clasificación Detallada y Widgets

- `LeagueStandingsPage.tsx`: matriz scrollable horizontal (`StandingsMatrixTable`) con tabs Puntos/Peso/Pieza Mayor, descartes tachados con tooltip, y pie de tabla con filas de Asistentes, Totales y Promedio.
- `SeasonBiggestCatchView` integrada como tab "Pieza Mayor".
- `HomePage.tsx`: widget de resumen de la liga activa con Top 3 puntos (`pointsAfterDiscard`), Top 3 peso y Pieza Mayor del año, con enlace a la clasificación completa.
- Verificado con `tsc --noEmit` sin errores. Commits: `afee5cd` (matriz + tabs) y `5506ada` (widget HomePage).

---

## 🔲 Foco de Trabajo Actual — Fase 5.E — Snapshots al Archivar Temporada
- **Backend:**
  - Crear entidad `LeagueSeasonSnapshot` (Guid, LeagueId, CapturedAt, JsonPayload) para persistir de manera inmutable el estado final de las clasificaciones al archivar una liga.
  - Implementar el comando `ArchiveLeagueWithSnapshotCommand` para congelar el estado de la temporada al archivarla.

---

## 🔲 Próximas Fases del Roadmap

1. **Fase 6: Acta Oficial FPCV (Word/PDF):** Generación programática del Acta FPCV basada en la plantilla oficial. Integración de datos de participantes, pesajes, piezas mayores, datos federativos y filtros de edad automatizados (<14 y >14 años).
2. **Fase 7: Frontend rol Fisherman:** Vistas e interfaces adaptadas para pescadores no administradores (calendario `/calendar`, mis inscripciones `/my-registrations` y navegación acotada).
3. **Fase 8: Estadísticas y Reporting:** Dashboard global con gráficos (recharts), evolución de kg/concurso e histórico del club.
4. **Fase de Revisión de Tests y Mockeo:** Refactorizar y ampliar la suite de pruebas mediante el plan estructurado de [test_review_plan.md](file:///C:/Users/spawndevuser/.gemini/antigravity/brain/a6d74e53-07f9-449b-8472-052cbb9ec34c/test_review_plan.md) (crear tests unitarios puros para Core, estandarizar Builders de datos y robustecer verificaciones de UoW).

---

## Decisiones de Diseño y Normas Clave
- **Domain Services:** Toda la lógica matemática pesada de clasificaciones y reparto de puntos se aísla en servicios puros e independientes de la persistencia (como `PointsCalculatorService`), garantizando alta testabilidad unitaria.
- **Rich Domain Model:** Mantener el control de estados y validaciones dentro del modelo de dominio de las entidades (`League`, `Competition`, `CompetitionResult`).
- **Seguridad en Front/Back:** Restringir operaciones administrativas mediante `[Authorize(Roles = "Admin")]` en el backend y guards de estado/roles en el frontend.
