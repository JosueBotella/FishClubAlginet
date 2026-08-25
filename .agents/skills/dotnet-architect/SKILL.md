---
name: dotnet-architect
description: Diseña, implementa o revisa cambios de backend .NET 10 y frontend React 19 en FishClubAlginet respetando su Clean Architecture, CQRS, ErrorOr, Unit of Work, Outbox y convenciones de pruebas.
---

# FishClubAlginet Architect

Antes de un cambio, contrasta el estado real del código con `FishClubAlginet_context.md`; `HANDOFF.md` y partes de `PROJECT_STATUS.md` pueden estar desactualizados.

## Invariantes del proyecto

- `Core` contiene el dominio y no depende de Infrastructure.
- `Application` contiene handlers y abstracciones, y no conoce EF Core.
- Los repositorios preparan cambios; `IUnitOfWork.SaveChangesAsync` persiste y traduce errores de infraestructura a `ErrorOr<int>`.
- Los cambios de estado pertenecen al modelo de dominio y sus eventos se persisten mediante Outbox.
- Conserva `MapInboundClaims = false`, `RoleClaimType = "role"` y el flujo JWT actual salvo que la tarea cambie explícitamente autenticación.
- En React usa `ProtectedRoute`, Context API, Axios, Mantine Form y tipos explícitos. No introduzcas Zustand, React Hook Form o Zod sin una decisión arquitectónica explícita.

## Verificación

Sigue las reglas de rama, pruebas, SonarQube y usings globales de `.agents/AGENTS.md`. Para relaciones transversales o refactorizaciones, usa la skill `codegraph-analysis` cuando CodeGraph esté disponible.

Para convenciones detalladas consulta `skills.md`, pero valida los ejemplos contra las APIs reales antes de copiarlos.
