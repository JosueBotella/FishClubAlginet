# FishClubAlginet - Fase 3: Concursos y Resultados

## Visión General
Implementación del núcleo operativo del club: la gestión de concursos de pesca. Un concurso es una jornada ligada a una Liga donde los pescadores compiten, se les asigna un puesto y se registran sus capturas para generar un ranking automático.

## Objetivos Técnicos
1. **Dominio Rico:** Las entidades `Competition` y `CompetitionResult` deben gestionar su propio estado y reglas (p. ej., validación de asistencia y asignación de puntos mínimos).
2. **Integridad de Datos:** Garantizar mediante EF Core que no haya duplicidad de puestos en un concurso ni inscripciones dobles de un mismo pescador.
3. **Cálculo Automático:** El sistema debe ser capaz de procesar los pesos de las capturas y determinar la posición (Ranking) y los puntos correspondientes según la normativa del club.

## Restricciones y Reglas Críticas
- **Puntos de Participación:** Si un pescador asiste pero no pesca nada (0 gramos), recibe automáticamente 5 puntos.
- **Flujo de Estados:** Un concurso pasa por: Planned -> RegistrationOpen -> Closed -> ResultsDraft -> ResultsValidated.
- **Relaciones:** Un `Competition` siempre pertenece a una `League`. Un `CompetitionResult` vincula un `Fisherman` con una `Competition`.

## Stack Operativo
- Backend: .NET 10, EF Core, MediatR (CQRS).
- Frontend: React + Mantine v7.