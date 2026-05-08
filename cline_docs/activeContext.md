# Contexto Activo: Persistencia Fase 3

Estamos moviendo la lógica de Dominio a la base de datos.

## Requerimientos Técnicos para Infrastructure:
1. **Índices Únicos Compuestos (Crítico):**
   - Tabla `Competitions`: `LeagueId` + `CompetitionNumber`.
   - Tabla `CompetitionResults`: `CompetitionId` + `FishermanId`.
   - Tabla `CompetitionResults`: `CompetitionId` + `AssignedSpotNumber` (permitir NULLs).
2. **Mapeo de Datos:**
   - Todos los Enums deben guardarse como `string`.
   - `Venue` máximo 100 caracteres.
   - `Points` precisión decimal (18,2).
3. **DbContext:** Usar `ApplyConfigurationsFromAssembly` para registrar las nuevas clases.