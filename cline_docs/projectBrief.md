# FishClubAlginet — Dossier del Proyecto

## Visión General
**FishClubAlginet** es una plataforma de gestión integral diseñada específicamente para el club de pesca local de Alginet (perteneciente a la Federación de Pesca de la Comunidad Valenciana, club V42). El sistema automatiza las operaciones administrativas críticas del club: gestión de socios (Fishermen), control y ciclo de vida de ligas anuales, organización de competiciones por jornadas, sorteo de pesqueras, registro de capturas, y generación automatizada de las clasificaciones anuales y actas oficiales.

---

## Objetivos del Proyecto
1. **Dominio Rico y Encapsulado (Rich Domain Model):** Asegurar que las entidades del negocio (`League`, `Competition`, `CompetitionResult`) controlen sus propios estados y validen las reglas de negocio críticas directamente desde la capa Core, eliminando la anemia del modelo de dominio.
2. **Precisión Matemática:** Automatizar el sistema de puntos de ranking de liga resolviendo los empates y la asistencia base de forma consistente, eliminando errores humanos de transcripción manual de Excel.
3. **Usabilidad en Tiempo Real:** Proveer a los administradores del club de una interfaz moderna y fluida en React con Mantine UI para registrar capturas inline a pie de escenario, y a los pescadores de un acceso de lectura directo.
4. **Cumplimiento Federativo:** Generar de forma programática las actas oficiales en formato Word/PDF listas para enviar a la FPCV, categorizando automáticamente a los concursantes por edad (<14 o >14 años).

---

## Stack Técnico de Referencia
- **Backend:** .NET 10, ASP.NET Core API, Clean Architecture (Core, Application, Infrastructure, API).
- **Frontend:** React 19 + TypeScript, Mantine UI v7, @tabler/icons-react, Axios, PostCSS.
- **Base de Datos:** SQL Server Express, Entity Framework Core (Code-First), Patrón Unit of Work.
- **CQRS & Mensajería:** MediatR (Commands/Queries/Domain Events), FluentValidation para pipelines automáticos.
- **Fiabilidad Transaccional:** Outbox Pattern persistido para envío garantizado (ACID) de eventos de dominio a segundo plano.
- **Entorno de Despliegue:** Docker, Docker Compose, Portainer.

---

## Restricciones y Reglas de Negocio Críticas
- **Ligas Activas:** Solo puede haber una liga configurada como activa en el club de forma simultánea. Las ligas antiguas se archivan (y opcionalmente se congelan con snapshots de clasificación).
- **Ciclo del Concurso:** Un concurso transiciona rígidamente por: `Planned` $\rightarrow$ `RegistrationOpen` $\rightarrow$ `Closed` (cierre de registro e inicio de sorteo) $\rightarrow$ `ResultsDraft` (registro de pesajes e inicio del cálculo de puntos) $\rightarrow$ `ResultsValidated` (resultados bloqueados y listos para generación de acta).
- **Sistema de Puntuación (Fase 5.A):**
  - **Asistencia:** 5 puntos base mínimos para todo participante presente (`DidAttend = true`), pesque o no.
  - **Escala de Ranking:** El primero recibe +20 puntos, el segundo +19 puntos, ..., descendiendo hasta el 20º (+1 punto). Del 21º en adelante se añade +0 puntos.
  - **Empates:** Si hay empates en peso en el top 20, los competidores empatados conservan la posición original del grupo y se reparten equitativamente un bonus adicional de `+1 / nEmpatados` (reparto de puntos federativos).
  - **Ausencias:** 0 puntos de liga y clasificación 0. No se aplica el mínimo de asistencia.
- **Pieza Mayor (PM):** Por concurso o acumulada de la temporada. Puede configurarse un peso mínimo en gramos por jornada (`BiggestCatchMinWeightInGrams`); las capturas por debajo del umbral no califican para pieza mayor.

---

## Hitos de Desarrollo Actuales
- **Hito Actual:** **Fase 5: Clasificación Detallada y Pieza Mayor (Fases 5.B - 5.E)**. Implementación de la matriz scrollable en frontend tipo Excel con desgloses por concurso y las consultas agregadas para obtener las mayores capturas del año.
- **Hito Próximo:** **Fase 6: Generación automatizada del Acta Oficial FPCV (Word/PDF)**.