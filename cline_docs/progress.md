# Roadmap de Fase 3: Concursos y Resultados

## Capa 1: Dominio (Core)
- [x] Crear Enums: `Subspecialty`, `Category`, `CompetitionStatus`.
- [x] Crear Entidad `Competition` (con reglas de negocio y estados).
- [x] Crear Entidad `CompetitionResult` (vinculación pescador-concurso y lógica de puntos).

## Capa 2: Infraestructura (Infrastructure)

- [x] Configuración Fluent API para `Competition` (Índice único: LeagueId + Number).
- [x] Configuración Fluent API para `CompetitionResult` (Índice único: CompetitionId + FishermanId).
- [x] Crear y ejecutar Migración de EF Core: `AddPhase3Competitions`.

## Capa 3: Aplicación (Application)
- [ ] Definir DTOs de Fase 3 (`CompetitionDto`, `RegisterToCompetitionRequest`, etc.).
- [ ] Implementar Command: `CreateCompetition` (Solo Admin).
- [ ] Implementar Command: `RegisterFishermanToCompetition`.
- [ ] Implementar Query: `GetCompetitionResults` (Cálculo de rankings en tiempo real).

## Capa 4: Presentación (React)
- [ ] Vista de listado de concursos en la Liga.
- [ ] Formulario de inscripción para el pescador.
- [ ] Panel de gestión de pesos y posiciones (Solo Admin).
