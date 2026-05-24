## Reglas de Entorno (Crítico)
- Tienes disponible el servidor MCP local de CodeGraph.
- Antes de leer archivos `.cs` de forma secuencial, DEBES invocar el MCP de CodeGraph para analizar las dependencias y la estructura del código.

# Contexto Activo — Fase 5: Clasificación Detallada y Pieza Mayor (Fases 5.B - 5.E)

## Estado actual
Fases 1 a 5.A completamente completadas. El **bug crítico de puntos** (`Points = WeightInGrams`) ha sido resuelto exitosamente mediante la implementación del servicio de dominio `PointsCalculatorService` y la persistencia correcta de puntos por ranking y empates (sistema confirmado de 20 a 1 puntos + asistencia + reparto de empates).

También se ha completado la configuración del **mínimo de peso para Pieza Mayor por competición** a nivel de base de datos, backend (PATCH endpoint) y frontend (Mantine input y actualización inline).

---

## 🔲 Foco de Trabajo Actual — Fase 5.B-E: Clasificación detallada + Pieza Mayor + Frontend

Estamos listos para abordar las siguientes sub-fases de clasificación avanzada:

### 1. Fase 5.B — Clasificación Detallada (Matriz por concurso)
- **Backend:**
  - Ampliar `GetLeagueStandingsQuery` para devolver el desglose por concurso mediante una estructura de tipo matriz/diccionario: `FishermanId` -> `Dictionary<Guid, decimal>` (puntos o peso por cada `CompetitionId`).
  - Crear nuevo DTO `LeagueStandingsDetailDto` para transportar esta información de forma eficiente.
  - Diseñar tests con datos reales de la temporada 2025 (`LIGA POR PESO 2025.xls`, ~43 pescadores, 18 concursos).
- **Columna "RESTA" (Descartes):**
  - Mantener tooltip en frontend indicando *"Pendiente de definir por el cliente"*, ya que no hay un patrón computable definido para los descartes de la temporada 2025 (ej. Juan Alcaraz con resta de 2.5).

### 2. Fase 5.C — Pieza Mayor Global e Individual
- **Backend:**
  - Implementar query `GetSeasonBiggestCatchQuery(Guid leagueId)` que calcule el pescador, peso y concurso de la mayor captura de la temporada (ej: "PM CRISTIAN VOINESCU — 4870 gr" en 2025).
  - Implementar query `GetCompetitionBiggestCatchQuery(Guid competitionId)` para obtener la mayor captura de un concurso específico.
  - Asegurar la integración del premio de "Pieza Mayor" en la respuesta del acta y en `CompetitionResultDto`.

### 3. Fase 5.D — Frontend para Clasificación Detallada y Widgets
- **React (Mantine v7):**
  - Ampliar la UI de `LeagueStandingsPage.tsx` con una **matriz scrollable horizontal** con columnas: `Posición` | `Nombre` | `[C1]` `[C2]` ... `[CN]` | `Total`.
  - Agregar fila de totales agregados del concurso en el pie de la matriz.
  - Implementar la pestaña "Pieza Mayor" (`/leagues/{id}/biggest-catches`) para visualizar el podio de capturas.
  - Añadir widget de resumen en `HomePage.tsx` que liste el top 3 de peso, top 3 de puntos y la pieza mayor del año.

### 4. Fase 5.E — Snapshots al Archivar Temporada
- **Backend:**
  - Crear entidad `LeagueSeasonSnapshot` (Guid, LeagueId, CapturedAt, JsonPayload) para persistir de manera inmutable el estado final de las clasificaciones al archivar una liga.
  - Implementar el comando `ArchiveLeagueWithSnapshotCommand` para congelar el estado de la temporada al archivarla.

---

## 🔲 Próximas Fases del Roadmap

1. **Fase 6: Acta Oficial FPCV (Word/PDF):** Generación programática del Acta FPCV basada en la plantilla oficial. Integración de datos de participantes, pesajes, piezas mayores, datos federativos y filtros de edad automatizados (<14 y >14 años).
2. **Fase 7: Frontend rol Fisherman:** Vistas e interfaces adaptadas para pescadores no administradores (calendario `/calendar`, mis inscripciones `/my-registrations` y navegación acotada).
3. **Fase 8: Estadísticas y Reporting:** Dashboard global con gráficos (recharts), evolución de kg/concurso e histórico del club.

---

## Decisiones de Diseño y Normas Clave
- **Domain Services:** Toda la lógica matemática pesada de clasificaciones y reparto de puntos se aísla en servicios puros e independientes de la persistencia (como `PointsCalculatorService`), garantizando alta testabilidad unitaria.
- **Rich Domain Model:** Mantener el control de estados y validaciones dentro del modelo de dominio de las entidades (`League`, `Competition`, `CompetitionResult`).
- **Seguridad en Front/Back:** Restringir operaciones administrativas mediante `[Authorize(Roles = "Admin")]` en el backend y guards de estado/roles en el frontend.
