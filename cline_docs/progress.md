# Roadmap de Fase 3: Concursos y Resultados

## Capa 1: Dominio (Core)
- [x] Crear Enums: `Subspecialty`, `Category`, `CompetitionStatus`.
- [x] Crear Entidad `Competition` (con reglas de negocio y estados).
- [x] Crear Entidad `CompetitionResult` (vinculación pescador-concurso y lógica de puntos).

## Capa 2: Infraestructura (Infrastructure)
- [x] Configuración Fluent API para `Competition` (Índice único: LeagueId + Number).
- [x] Configuración Fluent API para `CompetitionResult` (Índice único: CompetitionId + FishermanId).
- [x] Crear y ejecutar Migración de EF Core: `AddCompetitions`.

## Capa 3: Aplicación (Application)
- [x] Definir DTOs de Fase 3 (`CompetitionDto`, `CompetitionResultDto`, `CreateCompetitionRequest`, `RegisterFishermanRequest`).
- [x] Implementar Command: `CreateCompetitionCommand` + Handler + Validator (Solo Admin).
- [x] Implementar Command: `RegisterFishermanCommand` + Handler + Validator.
- [x] Implementar Query: `GetCompetitionResultsQuery` (Rankings calculados en tiempo real).
- [x] Implementar Query: `GetCompetitionsByLeagueQuery` (listado de concursos por liga).

## Capa 4: API (Presentación backend)
- [x] `CompetitionsController`: GET /competitions?leagueId, POST /competitions, POST /competitions/{id}/register, GET /competitions/{id}/results.

## Capa 5: Presentación (React)
- [x] `types/competition.ts` — interfaces TypeScript para todos los DTOs.
- [x] `api/competitionsApi.ts` — cliente Axios (getByLeague, create, register, getResults).
- [x] `constants/endpoints.ts` + `routes.ts` — endpoints y rutas actualizados.
- [x] `AdminLeaguesPage` — icono para navegar a concursos de cada liga.
- [x] `AdminCompetitionsPage` — listado de concursos con estado y plazas.
- [x] `CreateCompetitionModal` — formulario completo (número, fecha, venue, zona, subesp., categoría, plazas).
- [x] `CompetitionResultsPage` — tabla de resultados con ranking en tiempo real + modal inscripción pescador.
- [x] `App.tsx` — rutas registradas con ProtectedRoute Admin.

## ✅ Fase 3 completada

## Próximos pasos sugeridos
- Implementar asignación de puestos de sorteo (`AssignSpots`).
- Implementar entrada de resultados bulk (`EnterResults`).
- Implementar transición de estado (`RegistrationOpen`, `Closed`, `ResultsDraft`, `ResultsValidated`).
- Clasificación general por liga (suma de puntos, descartando peores resultados).
