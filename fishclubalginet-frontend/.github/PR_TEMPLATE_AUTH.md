## PR: docs — Domain model update from real league data analysis

### Descripción
Actualización del modelo de dominio en `FishClubAlginet_context.md` basada en el análisis de ficheros reales de gestión de liga (Excel + Acta federativa). Se revisaron los datos de la temporada 2025 del club V42.

### Cambios principales

**Modelo de dominio revisado:**
- **League:** añadidos `MinPoints` (default 5) y `WorstResultsToDiscard` para sistema dual de puntuación
- **Competition:** añadidos `CompetitionNumber`, `Venue`, `Zone`, `Subspecialty`, `Category`, `ParticipantCount`
- **CompetitionResult:** nueva entidad que sustituye a `CompetitionRegistration`. Incluye `WeightInGrams`, `BiggestCatchWeight`, `Points`, `Ranking`, `AssignedSpotNumber`
- **FishingSpot:** eliminada como entidad (el nº de puesto es un campo int en CompetitionResult)
- **Fisherman:** añadido campo `FederationNumber` (ej: "V-552")

**Reglas de negocio documentadas:**
- Sistema de puntuación por ranking inverso con empates (media de posiciones)
- Puntos mínimos = 5 (incluso con 0 capturas)
- Clasificación dual: por peso acumulado + por puntos con resta de N peores
- Premio Pieza Mayor por concurso y por temporada

**Roadmap actualizado:**
- Auth marcado como completado (PR #9)
- Phase 2-3 ampliadas con ítems detallados de implementación

### Ficheros modificados
- `FishClubAlginet_context.md`

### Testing
- Sin cambios de código — solo documentación
- Tests backend verificados: 14 ficheros, todos basados en mocks, sin dependencias de BD
