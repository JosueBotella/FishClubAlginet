# Contexto Activo: Fase 3 completada

## Estado
Fase 3 (Concursos y Resultados) completamente implementada — backend + frontend.

## Lo que funciona
- CRUD de concursos dentro de una liga (Admin).
- Inscripción de pescadores a concursos abiertos.
- Consulta de resultados con ranking en tiempo real (ties compartidos).
- Navegación: Ligas → Concursos → Resultados/Inscripciones.

## Pendiente para Fase 3 extendida
- **AssignSpotsCommand**: asignar puesto de sorteo a cada inscrito.
- **EnterResultsCommand**: entrada bulk de pesos (DidAttend, WeightInGrams, BiggestCatchWeight).
- **Transiciones de estado**: RegistrationOpen → Closed → ResultsDraft → ResultsValidated.
- **Clasificación general**: suma de puntos por liga descartando los N peores resultados (`worstResultsToDiscard`).

## Rama activa
`branch_phase_three`
